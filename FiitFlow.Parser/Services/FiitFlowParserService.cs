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

        public async Task<string> ParseAsync(string configPath, string studentName)
        {
            if (!File.Exists(configPath))
                throw new FileNotFoundException($"Конфиг не найден: {configPath}");
            
            var config = await LoadJsonConfigAsync(configPath);

            if (string.IsNullOrWhiteSpace(studentName))
                throw new ArgumentException("Имя студента не указано");

            using var outputWriter = new StringOutputWriter();

            var searchService = new StudentSearchService(
                new ExcelDownloader(_httpClient),
                new ExcelParser(),
                outputWriter
            );

            await searchService.SearchStudentInAllTablesAsync(config, studentName);

            return outputWriter.GetContent();
        }

        public async Task<string> ParseAsync(string configPath)
        {
            if (!File.Exists(configPath))
                throw new FileNotFoundException($"Конфиг не найден: {configPath}");

            var config = await LoadJsonConfigAsync(configPath);

            if (string.IsNullOrWhiteSpace(config.StudentName))
                throw new ArgumentException("Имя студента не указано в конфиге");

            return await ParseAsync(configPath, config.StudentName);
        }

        private async Task<ParserConfig> LoadJsonConfigAsync(string configPath)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };

            string json = await File.ReadAllTextAsync(configPath);
            return JsonSerializer.Deserialize<ParserConfig>(json, options);
        }
    }
}
