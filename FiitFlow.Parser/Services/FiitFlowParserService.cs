using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace FiitFlow.Parser.Services
{
    public class FiitFlowParserService
    {
        private readonly HttpClient _httpClient;

        public FiitFlowParserService(HttpClient? httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient();
        }

        public async Task<string> ParseAsync(string configPath)
        {
            if (!File.Exists(configPath))
                throw new FileNotFoundException($"Конфиг не найден: {configPath}");

            var configContent = await File.ReadAllLinesAsync(configPath);
            var config = new ConfigParser().Parse(configContent);

            if (string.IsNullOrWhiteSpace(config.StudentName))
                throw new ArgumentException("Имя студента не указано");

            using var outputWriter = new StringOutputWriter();

            var searchService = new StudentSearchService(
                new ExcelDownloader(_httpClient),
                new ExcelParser(),
                outputWriter
            );

            await searchService.SearchStudentInAllTablesAsync(config);

            return outputWriter.GetContent();
        }
    }
}

