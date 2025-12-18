using NUnit.Framework;
using FiitFlow.Parser.Services;
using FiitFlow.Parser.Models;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace FiitFlow.Parser.Tests
{
    [TestFixture]
    public class FiitFlowParserTests
    {
        [Test]
        public async Task ParseWithFormulas_ReturnsStudentAndSubject()
        {
            var parser = new FiitFlowParserService(
                searchService: TestServices.CreateStudentSearchService(),
                calculator: new FormulaCalculatorService()
            );

            var configPath = TestData.Get("test_config.json");

            var result = await parser.ParseWithFormulasAsync(
                configPath,
                "Макарова Дарья"
            );

            Assert.That(result.Student, Is.EqualTo("Макарова Дарья"));
            Assert.That(result.Subjects.ContainsKey("МАТАН"));
        }
    }
}

