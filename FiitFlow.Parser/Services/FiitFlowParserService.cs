using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using FiitFlow.Parser.Models;

namespace FiitFlow.Parser.Services
{
    public class FiitFlowParserService
    {
        private readonly HttpClient _httpClient;
        private readonly FormulaCalculatorService _calculator;

        public FiitFlowParserService(HttpClient? httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient();
            _calculator = new FormulaCalculatorService();
        }

        public async Task<StudentSearchResult> ParseAsync(string configPath, string? studentName = null)
        {
            if (!File.Exists(configPath))
                throw new FileNotFoundException($"Конфиг не найден: {configPath}");

            var config = await LoadJsonConfigAsync(configPath);

            if (string.IsNullOrWhiteSpace(studentName))
            {
                studentName = config.StudentName;
            }

            var cacheSettings = config.CacheSettings ?? new CacheSettings();
            var cacheService = new CacheService(cacheSettings.CacheDirectory, cacheSettings.ForceRefresh);

            var excelDownloader = new ExcelDownloader(_httpClient, cacheService);

            var searchService = new StudentSearchService(
                    excelDownloader,
                    new ExcelParser()
                    );

            return await searchService.SearchStudentInAllTablesAsync(config, studentName);
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

            var cacheSettings = config.CacheSettings ?? new CacheSettings();
            var cacheService = new CacheService(cacheSettings.CacheDirectory, cacheSettings.ForceRefresh);

            var excelDownloader = new ExcelDownloader(_httpClient, cacheService);

            var searchService = new StudentSearchService(
                    excelDownloader,
                    new ExcelParser()
                    );

            var searchResult = await searchService.SearchStudentInAllTablesAsync(config, studentName);

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

            if (config.Formulas?.SubjectFormulas == null || !config.Formulas.SubjectFormulas.Any())
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

            foreach (var subjectFormula in config.Formulas.SubjectFormulas)
            {
                var subjectTables = searchResult.Tables
                    .Where(t => subjectFormula.TableNames.Contains(t.TableName))
                    .ToList();

                if (!subjectTables.Any())
                    continue;

                var subjectResult = new SubjectResults
                {
                    Tables = subjectTables,
                    Categories = new Dictionary<string, Dictionary<string, object>>()
                };

                if (subjectFormula.ComponentFormulas?.Any() == true)
                {
                    var components = _calculator.CalculateComponents(
                            subjectTables,
                            subjectFormula.ComponentFormulas,
                            subjectFormula.ValueMappings
                            );

                    foreach (var component in components)
                    {
                        subjectResult.Categories[component.Key] = new Dictionary<string, object>
                        {
                            ["value"] = component.Value
                        };
                    }

                    if (!string.IsNullOrWhiteSpace(subjectFormula.FinalFormula))
                    {
                        subjectResult.CalculatedScore = _calculator.CalculateFinalScore(
                                subjectFormula.FinalFormula,
                                components,
                                subjectTables,
                                subjectFormula.AggregateMethod
                                );

                        subjectResult.Overall["CalculatedScore"] = subjectResult.CalculatedScore;
                    }
                }

                ExtractOverallData(subjectResult, subjectTables);

                studentResults.Subjects[subjectFormula.SubjectName] = subjectResult;

                CalculateRatingScore(studentResults, subjectFormula.SubjectName, subjectResult);
            }

            var processedTables = config.Formulas.SubjectFormulas
                .SelectMany(sf => sf.TableNames)
                .ToHashSet();

            var remainingTables = searchResult.Tables
                .Where(t => !processedTables.Contains(t.TableName))
                .GroupBy(t => t.TableName)
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
                            if (double.TryParse(kvp.Value, out double numericValue))
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
                        if (double.TryParse(kvp.Value.ToString(), out double brsValue))
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
    }
}
