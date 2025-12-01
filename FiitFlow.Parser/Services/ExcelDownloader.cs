using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using FiitFlow.Parser.Models;

namespace FiitFlow.Parser.Services
{
    public class ExcelDownloader
    {
        private readonly HttpClient _httpClient;
        private readonly CacheService _cacheService;

        public ExcelDownloader(HttpClient httpClient, CacheService cacheService)
        {
            _httpClient = httpClient;
            _cacheService = cacheService;
            
            _httpClient.DefaultRequestHeaders.Add("User-Agent", 
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
        }

        public async Task<string> DownloadAsync(TableConfig table)
        {
            if (!_cacheService.ShouldDownload(table))
            {
                var cachedFile = _cacheService.GetCachedFile(table);
                if (cachedFile != null && File.Exists(cachedFile))
                {
                    return cachedFile;
                }
            }

            var downloadUrl = BuildDownloadUrl(table.Url);
            var response = await _httpClient.GetAsync(downloadUrl);
            response.EnsureSuccessStatusCode();

            var finalPath = _cacheService.GetCachedFilePath(table);

            await using var stream = await response.Content.ReadAsStreamAsync();
            await using var fileStream = new FileStream(finalPath, FileMode.Create, FileAccess.Write);
            await stream.CopyToAsync(fileStream);

            return finalPath;
        }

        private static string BuildDownloadUrl(string sheetUrl)
        {
            var fileId = ExtractFileId(sheetUrl);
            return $"https://docs.google.com/spreadsheets/d/{fileId}/export?format=xlsx";
        }

        private static string ExtractFileId(string url)
        {
            try
            {
                var uri = new Uri(url);
                var path = uri.AbsolutePath;
                var startIndex = path.IndexOf("/d/", StringComparison.Ordinal) + 3;
                
                if (startIndex < 3) throw new ArgumentException("Invalid Google Sheets URL");
                
                var endIndex = path.IndexOf("/", startIndex, StringComparison.Ordinal);
                if (endIndex == -1) endIndex = path.Length;
                
                return path.Substring(startIndex, endIndex - startIndex);
            }
            catch
            {
                throw new ArgumentException($"Неверный URL таблицы: {url}");
            }
        }
    }
}
