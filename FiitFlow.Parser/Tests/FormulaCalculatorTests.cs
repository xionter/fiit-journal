using NUnit.Framework;
using FiitFlow.Parser.Services;
using FiitFlow.Parser.Models;
using System.Collections.Generic;

namespace FiitFlow.Parser.Tests
{
    [TestFixture]
    public class FormulaCalculatorTests
    {
        [Test]
        public void CalculateFinalScore_SumsComponentsCorrectly()
        {
            var calculator = new FormulaCalculatorService();

            var tables = new List<TableResult>
            {
                new TableResult(
                    "Макарова Дарья",
                    "МАТАН ПРАКТИКА",
                    "",
                    "Sheet 1",
                    new Dictionary<string, string>
                    {
                        { "ДЗ", "10" },
                        { "КР", "25" },
                        { "Пров", "20" },
                        { "Акт", "20" }
                    }
                )
            };

            var components = new Dictionary<string, double>
            {
                { "ДЗ", 10 },
                { "КР", 25 },
                { "Пров", 20 },
                { "Акт", 20 }
            };

            var result = calculator.CalculateFinalScore(
                "ДЗ + КР + Пров + Акт",
                components,
                tables);

            Assert.That(result, Is.EqualTo(75));
        }
    }
}

