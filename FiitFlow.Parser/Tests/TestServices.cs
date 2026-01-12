using FiitFlow.Parser.Services;
using System.Net.Http;

namespace FiitFlow.Parser.Tests
{
    public static class TestServices
    {
        public static StudentSearchService CreateStudentSearchService()
        {
            var httpClient = new HttpClient();
            var downloader = new ExcelDownloader(httpClient);
            var parser = new ExcelParser();

            return new StudentSearchService(downloader, parser);
        }
    }
}
