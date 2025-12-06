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

        // СТАРЫЙ МЕТОД для обратной совместимости
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

        // НОВЫЙ МЕТОД с формулами
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

            // Получаем результаты из таблиц
            var searchResult = await searchService.SearchStudentInAllTablesAsync(config, studentName);
            
            // Группируем по предметам и рассчитываем формулы
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

            // Если нет формул, просто группируем таблицы по названию
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

            // Обрабатываем каждый предмет с формулой
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

                // Рассчитываем компоненты, если есть формулы
                if (subjectFormula.ComponentFormulas?.Any() == true)
                {
                    var components = _calculator.CalculateComponents(
                        subjectTables,
                        subjectFormula.ComponentFormulas,
                        subjectFormula.ValueMappings
                    );

                    // Добавляем компоненты в категории
                    foreach (var component in components)
                    {
                        subjectResult.Categories[component.Key] = new Dictionary<string, object>
                        {
                            ["value"] = component.Value
                        };
                    }

                    // Рассчитываем итоговый балл
                    if (!string.IsNullOrWhiteSpace(subjectFormula.FinalFormula))
                    {
                        subjectResult.CalculatedScore = _calculator.CalculateFinalScore(
                            subjectFormula.FinalFormula,
                            components,
                            subjectTables
                        );
                        
                        // Добавляем в общие результаты
                        subjectResult.Overall["CalculatedScore"] = subjectResult.CalculatedScore;
                    }
                }

                // Извлекаем общие данные из таблиц
                ExtractOverallData(subjectResult, subjectTables);

                studentResults.Subjects[subjectFormula.SubjectName] = subjectResult;

                // Рассчитываем рейтинговый балл (можно настроить отдельную формулу)
                CalculateRatingScore(studentResults, subjectFormula.SubjectName, subjectResult);
            }

            // Добавляем остальные таблицы без формул
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
            // Извлекаем общие данные из всех таблиц предмета
            var overallData = new Dictionary<string, object>();
            
            foreach (var table in tables)
            {
                foreach (var kvp in table.Data)
                {
                    // Ищем ключевые поля
                    if (kvp.Key.Contains("Итог") || kvp.Key.Contains("Сумма") || 
                        kvp.Key.Contains("Всего") || kvp.Key.Contains("Total"))
                    {
                        overallData[$"{table.SheetName}_{kvp.Key}"] = kvp.Value;
                    }
                }
            }
            
            subjectResult.Overall["ExtractedData"] = overallData;
        }

        private void CalculateRatingScore(
            StudentResults studentResults, 
            string subjectName, 
            SubjectResults subjectResult)
        {
            // Пример: преобразуем в 100-балльную систему
            // Можно добавить отдельную конфигурацию для преобразования
            double ratingScore = subjectResult.CalculatedScore;
            
            // Если есть общие данные, можно использовать их
            if (subjectResult.Overall.TryGetValue("ExtractedData", out var extractedDataObj) &&
                extractedDataObj is Dictionary<string, object> extractedData)
            {
                // Ищем итоговый балл в извлеченных данных
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
