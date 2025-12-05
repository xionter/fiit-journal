using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using FiitFlow.Parser.Models;

namespace FiitFlow.Parser.Services
{
    public class FiitFlowParserService
    {
        private readonly HttpClient _httpClient;

        public FiitFlowParserService(HttpClient? httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient();
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
