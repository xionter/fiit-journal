using FiitFlow.Parser.Services;
using System.Net.Http;

namespace FiitFlow.Parser.Tests
{
    public static class TestServices
    {
        public static StudentSearchService CreateStudentSearchService()
        {
            var cache = new CacheService("./Tests/Cache", forceRefresh: false);
            var httpClient = new HttpClient();
            var downloader = new ExcelDownloader(httpClient, cache);
            var parser = new ExcelParser();

            return new StudentSearchService(downloader, parser);
        }
    }
}

