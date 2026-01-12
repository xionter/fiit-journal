using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using FiitFlow.Parser.Models;
using FiitFlow.Parser.Interfaces;
using System.Collections.Generic;
using System.Globalization;

namespace FiitFlow.Parser.Services
{
    public class FiitFlowParserService
    {
        private readonly IStudentSearchService _searchService;
        private readonly FormulaCalculatorService _calculator;

        public FiitFlowParserService(
                IStudentSearchService searchService,
                FormulaCalculatorService calculator)
        {
            _searchService = searchService;
            _calculator = calculator;
        }


        public async Task<StudentSearchResult> ParseAsync(string configPath, string tableName, string? studentName = null)
        {
            if (!File.Exists(configPath))
                throw new FileNotFoundException($"Конфиг не найден: {configPath}");
            //var configEditor = new ConfigEditorService(configPath);
            //configEditor.SetCache("", false);
            var config = await LoadJsonConfigAsync(configPath);
            //configEditor.SetCache("", true);
            if (string.IsNullOrWhiteSpace(studentName))
            {
                studentName = config.StudentName;
            }
            
            return await _searchService.SearchStudentInTableAsync(config, tableName);
        }

        public async Task<StudentResults> ParseWithFormulasAsync(string configPath, string? studentName = null)
        {
            if (!File.Exists(configPath))
                throw new FileNotFoundException($"Конфиг не найден: {configPath}");

            var config = await LoadJsonConfigAsync(configPath);

            if (string.IsNullOrWhiteSpace(studentName))
            {
                studentName = config.StudentName;
            }

            var searchResult = await _searchService.SearchStudentInAllTablesAsync(config, studentName);

            return GroupAndCalculateResults(config, searchResult, studentName);
        }

        private StudentResults GroupAndCalculateResults(
                ParserConfig config, 
                StudentSearchResult searchResult,
                string studentName)
        {
            var studentResults = new StudentResults
            {
                Student = studentName,
                Subjects = new Dictionary<string, SubjectResults>(),
                RatingScores = new Dictionary<string, double>()
            };

            if (config.Subjects == null || !config.Subjects.Any())
            {
                var groupedTables = searchResult.Tables
                    .GroupBy(t => t.TableName)
                    .ToDictionary(
                            g => g.Key,
                            g => new SubjectResults
                            {
                            Tables = g.ToList(),
                            Categories = new Dictionary<string, Dictionary<string, object>>()
                            }
                            );

                studentResults.Subjects = groupedTables;
                return studentResults;
            }

            foreach (var subject in config.Subjects)
            {
                var subjectTableKeys = subject.Tables?
                    .Select(t => GetTableKey(t.Name, t.Url))
                    .Where(k => !string.IsNullOrWhiteSpace(k))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList() ?? new List<string>();

                if (!subjectTableKeys.Any())
                    continue;

                var subjectTables = searchResult.Tables
                    .Where(t => subjectTableKeys.Contains(GetTableKey(t.TableName, t.TableUrl), StringComparer.OrdinalIgnoreCase))
                    .ToList();

                if (!subjectTables.Any())
                    continue;

                var subjectResult = new SubjectResults
                {
                    Tables = subjectTables,
                    Categories = new Dictionary<string, Dictionary<string, object>>()
                };

                if (subject.Formula?.ComponentFormulas?.Any() == true)
                {
                    var components = _calculator.CalculateComponents(
                            subjectTables,
                            subject.Formula.ComponentFormulas,
                            subject.Formula.ValueMappings
                            );

                    foreach (var component in components)
                    {
                        subjectResult.Categories[component.Key] = new Dictionary<string, object>
                        {
                            ["value"] = component.Value
                        };
                    }

                    subjectResult.CalculatedScore = _calculator.CalculateFinalScore(
                            subject.Formula.FinalFormula ?? string.Empty,
                            components,
                            subjectTables,
                            subject.Formula.AggregateMethod
                            );
                }
                else if (!string.IsNullOrWhiteSpace(subject.Formula?.FinalFormula))
                {
                    subjectResult.CalculatedScore = _calculator.CalculateFinalScore(
                            subject.Formula.FinalFormula,
                            new Dictionary<string, double>(),
                            subjectTables,
                            subject.Formula.AggregateMethod
                            );
                }

                if (subjectResult.CalculatedScore > 0)
                {
                    subjectResult.Overall["CalculatedScore"] = subjectResult.CalculatedScore;
                }

                ExtractOverallData(subjectResult, subjectTables);

                studentResults.Subjects[subject.SubjectName] = subjectResult;

                CalculateRatingScore(studentResults, subject.SubjectName, subjectResult);
            }

            var processedTables = config.Subjects
                .SelectMany(sf => sf.Tables ?? Enumerable.Empty<TableConfig>())
                .Select(t => GetTableKey(t.Name, t.Url))
                .Where(k => !string.IsNullOrWhiteSpace(k))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var remainingTables = searchResult.Tables
                .Where(t => !processedTables.Contains(GetTableKey(t.TableName, t.TableUrl)))
                .GroupBy(t => GetTableKey(t.TableName, t.TableUrl))
                .ToDictionary(
                        g => g.Key,
                        g => new SubjectResults
                        {
                        Tables = g.ToList(),
                        Categories = new Dictionary<string, Dictionary<string, object>>()
                        }
                        );

            foreach (var remaining in remainingTables)
            {
                studentResults.Subjects[remaining.Key] = remaining.Value;
            }

            return studentResults;
        }

        private static string GetTableKey(string? name, string? url)
        {
            var cleanName = name?.Trim() ?? string.Empty;
            var cleanUrl = url?.Trim() ?? string.Empty;
            return string.IsNullOrWhiteSpace(cleanUrl) ? cleanName : $"{cleanName}|{cleanUrl}";
        }

        private void ExtractOverallData(SubjectResults subjectResult, List<TableResult> tables)
        {
            var overallData = new Dictionary<string, object>();
            var totals = new Dictionary<string, double>();

            foreach (var table in tables)
            {
                foreach (var kvp in table.Data)
                {
                    string key = kvp.Key.ToLower();

                    if (key.Contains("итог") || key.Contains("сумма") || 
                            key.Contains("всего") || key.Contains("total") || 
                            key.Contains("итого в брс") || key.Contains("итого"))
                    {
                        if (!key.Contains("кр_сумма") &&
                                !key.Contains("дз_сумма") &&
                                !key.Contains("проверочные_сумма") && 
                                !key.Contains("активность_сумма"))
                        {
                            if (TryParseDoubleInvariant(kvp.Value, out double numericValue))
                            {
                                string cleanKey = CleanKey(kvp.Key);
                                if (!totals.ContainsKey(cleanKey) || 
                                        (cleanKey.Contains("итог") && numericValue > totals[cleanKey]))
                                {
                                    totals[cleanKey] = numericValue;
                                }
                            }
                            else
                            {
                                overallData[$"{table.SheetName}_{kvp.Key}"] = kvp.Value;
                            }
                        }
                    }
                }
            }

            foreach (var total in totals)
            {
                overallData[total.Key] = total.Value;
            }

            subjectResult.Overall["ExtractedData"] = overallData;
        }

        private string CleanKey(string key)
        {
            return key.Replace("_Сумма", "")
                .Replace("_Итог", "")
                .Replace("_Всего", "")
                .Trim();
        }
        private void CalculateRatingScore(
                StudentResults studentResults, 
                string subjectName, 
                SubjectResults subjectResult)
        {
            double ratingScore = subjectResult.CalculatedScore;

            if (subjectResult.Overall.TryGetValue("ExtractedData", out var extractedDataObj) &&
                    extractedDataObj is Dictionary<string, object> extractedData)
            {
                foreach (var kvp in extractedData)
                {
                    if (kvp.Key.Contains("БРС") || kvp.Key.Contains("Рейтинг"))
                    {
                        if (TryParseDoubleInvariant(kvp.Value, out double brsValue))
                        {
                            ratingScore = brsValue;
                            break;
                        }
                    }
                }
            }

            studentResults.RatingScores[subjectName] = Math.Round(ratingScore, 2);
        }

        private async Task<ParserConfig> LoadJsonConfigAsync(string configPath)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };

            string json = await File.ReadAllTextAsync(configPath);
            return JsonSerializer.Deserialize<ParserConfig>(json, options) ?? new ParserConfig();
        }

        private static bool TryParseDoubleInvariant(object? rawValue, out double value)
        {
            value = 0;

            switch (rawValue)
            {
                case null:
                    return false;
                case double d:
                    value = d;
                    return true;
                case float f:
                    value = f;
                    return true;
                case int i:
                    value = i;
                    return true;
                case long l:
                    value = l;
                    return true;
                case decimal dec:
                    value = (double)dec;
                    return true;
                case string s:
                    return TryParseNormalizedString(s, out value);
                default:
                    return TryParseNormalizedString(rawValue.ToString() ?? string.Empty, out value);
            }
        }

        private static bool TryParseNormalizedString(string value, out double number)
        {
            number = 0;
            if (string.IsNullOrWhiteSpace(value))
                return false;

            var normalized = NormalizeNumber(value);
            return double.TryParse(
                    normalized,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out number);
        }

        private static string NormalizeNumber(string value) =>
            value.Trim()
            .Replace("\u00A0", string.Empty)
            .Replace(" ", string.Empty)
            .Replace(",", ".");
    }
}
