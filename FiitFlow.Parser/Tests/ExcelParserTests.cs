using NUnit.Framework;
using FiitFlow.Parser.Services;
using FiitFlow.Parser.Models;
using System.IO;
using System.Linq;

namespace FiitFlow.Parser.Tests
{
    public static class TestData
    {
        public static string Get(string fileName) =>
            Path.Combine(
                    TestContext.CurrentContext.TestDirectory,
                    "Tests", "Data", fileName
            );
    }

    [TestFixture]
    public class ExcelParserTests
    {
        [Test]
        public void FindStudents_FindsStudentAndParsesCategories()
        {
            var parser = new ExcelParser();
            var students = parser.FindStudents(
                TestData.Get("matan_practice.xlsx"),
                "Макарова Дарья",
                new TableConfig
                {
                    Name = "МАТАН ПРАКТИКА",
                    Sheets = { new SheetConfig { Name = "Sheet 1" } }
                }).ToList();

            Assert.That(students.Count, Is.EqualTo(1));
            var student = students.First();
            Assert.That(student.FullName, Is.EqualTo("Макарова Дарья"));
            Assert.That(student.Data.ContainsKey("ДЗ"));
            Assert.That(student.Data.ContainsKey("КР"));
            Assert.That(student.Data.ContainsKey("Пров"));
            Assert.That(student.Data.ContainsKey("Акт"));
            Assert.That(student.Data.ContainsKey("Сумма"));
        }

        [Test]
        public void HomeworkSheet_ParsesDatesAndSum()
        {
            var parser = new ExcelParser();
            var students = parser.FindStudents(
                    TestData.Get("matan_practice.xlsx"),
                    "Макарова Дарья",
                    new TableConfig
                    {
                        Name = "МАТАН ПРАКТИКА",
                        Sheets = { new SheetConfig { Name = "ДЗ" } }
                    })
                    .ToList();

            var student = students.Single();

            Assert.That(student.Data.ContainsKey("Сумма"));
            Assert.That(student.Data["Сумма"], Is.EqualTo("10"));

            Assert.That(student.Data.Keys.Any(k => k.Contains("09")));
        }

        [Test]
        public void AutoDetectCategoriesRow_WorksWithoutConfig()
        {
            var parser = new ExcelParser();

            var students = parser.FindStudents(
                    TestData.Get("matan_practice.xlsx"),
                    "Чигвинцев Иван",
                    new TableConfig
                    {
                    Name = "МАТАН ПРАКТИКА",
                    Sheets = { new SheetConfig { Name = "Sheet 1" } }
                    }).ToList();

            var student = students.Single();

            Assert.That(student.Data.ContainsKey("ДЗ"));
            Assert.That(student.Data.ContainsKey("Сумма"));
        }
        
        [Test]
        public void FindStudents_WithoutName_ReturnsAllStudents()
        {
            var parser = new ExcelParser();

            var students = parser.FindStudents(
                    TestData.Get("matan_practice.xlsx"),
                    null,
                    new TableConfig
                    {
                    Name = "МАТАН ПРАКТИКА",
                    Sheets = { new SheetConfig { Name = "Sheet 1" } }
                    }).ToList();

            Assert.That(students.Count, Is.GreaterThan(10));
            Assert.That(students.Count, Is.GreaterThan(10));
        }

        [Test]
        public void UnknownStudent_ReturnsEmpty()
        {
            var parser = new ExcelParser();

            var students = parser.FindStudents(
                    TestData.Get("matan_practice.xlsx"),
                    "Несуществующий Студент",
                    new TableConfig
                    {
                    Name = "МАТАН ПРАКТИКА",
                    Sheets = { new SheetConfig { Name = "Sheet 1" } }
                    });

            Assert.That(students, Is.Empty);
        }
        [Test]
        public void EmptyScores_AreParsedCorrectly()
        {
            var parser = new ExcelParser();

            var student = parser.FindStudents(
                    TestData.Get("matan_practice.xlsx"),
                    "Телятников Михаил",
                    new TableConfig
                    {
                    Name = "МАТАН ПРАКТИКА",
                    Sheets = { new SheetConfig { Name = "Sheet 1" } }
                    }).Single();

            Assert.That(student.Data["Сумма"], Is.EqualTo("0"));
        }
    }
}

