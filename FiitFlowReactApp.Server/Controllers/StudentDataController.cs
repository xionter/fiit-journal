using Microsoft.AspNetCore.Mvc;
using FiitFlow.Parser.Services;
using FiitFlow.Parser.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FiitFlow.Parser.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentDataController : ControllerBase
    {
        private readonly FiitFlowParserService _parserService;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;

        public StudentDataController(IWebHostEnvironment env, IConfiguration configuration)
        {
            _env = env;
            _configuration = configuration;
            _parserService = new FiitFlowParserService();
        }

        // Старый метод без формул
        [HttpGet("{studentName}")]
        public async Task<ActionResult<StudentSearchResult>> GetStudentData(string studentName)
        {
            try
            {
                var configPath = GetConfigPath();
                
                if (string.IsNullOrEmpty(configPath))
                {
                    return NotFound(new { 
                        error = "Конфиг не найден",
                        suggestion = "Убедитесь, что файл config.json находится в проекте FiitFlow.Parser"
                    });
                }
                
                Console.WriteLine($"Использую конфиг: {configPath}");
                var result = await _parserService.ParseAsync(configPath, studentName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // Новый метод с формулами - ВАЖНО: ДОБАВИТЬ ЭТОТ МЕТОД!
        [HttpGet("with-formulas/{studentName}")]
        public async Task<ActionResult<StudentResults>> GetStudentDataWithFormulas(string studentName)
        {
            try
            {
                var configPath = GetConfigPath();
                
                if (string.IsNullOrEmpty(configPath))
                {
                    return NotFound(new { 
                        error = "Конфиг не найден",
                        suggestion = "Убедитесь, что файл config.json находится в проекте FiitFlow.Parser"
                    });
                }
                
                Console.WriteLine($"Использую конфиг для формул: {configPath}");
                
                // Проверим, есть ли раздел Formulas в конфиге
                var configJson = await File.ReadAllTextAsync(configPath);
                var hasFormulas = configJson.Contains("\"Formulas\"");
                Console.WriteLine($"Конфиг содержит Formulas: {hasFormulas}");
                
                var result = await _parserService.ParseWithFormulasAsync(configPath, studentName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, details = ex.StackTrace });
            }
        }

        // Метод для расчета вручную - ДОБАВИТЬ ЭТОТ МЕТОД!
        [HttpGet("calculate/{studentName}")]
        public async Task<IActionResult> CalculateStudentPoints(string studentName)
        {
            try
            {
                var configPath = GetConfigPath();
                
                if (string.IsNullOrEmpty(configPath))
                {
                    return NotFound(new { error = "Конфиг не найден" });
                }
                
                var result = await _parserService.ParseAsync(configPath, studentName);
                
                // Простой расчет вручную на основе ваших данных
                var calculatedPoints = new Dictionary<string, double>();
                
                foreach (var table in result.Tables)
                {
                    if (table.TableName == "МАТАН ПРАКТИКА" && table.SheetName == "Все баллы")
                    {
                        if (table.Data.TryGetValue("Сумма", out var sumStr) && 
                            double.TryParse(sumStr, out var sum))
                        {
                            calculatedPoints["МАТАН_ПРАКТИКА"] = sum;
                        }
                    }
                    else if (table.TableName == "ТЕРВЕР ЛЕКЦИИ")
                    {
                        if (table.Data.TryGetValue("Итого (без халявы)", out var terverStr) &&
                            double.TryParse(terverStr, out var terver))
                        {
                            calculatedPoints["ТЕРВЕР_ЛЕКЦИИ"] = terver;
                        }
                    }
                    else if (table.TableName == "СЕТИ")
                    {
                        if (table.Data.TryGetValue("Итог", out var setiStr) &&
                            double.TryParse(setiStr, out var seti))
                        {
                            calculatedPoints["СЕТИ"] = seti;
                        }
                    }
                    else if (table.TableName == "ПРОЕКТИРОВАНИЕ НА ЯЗЫКЕ C#")
                    {
                        if (table.SheetName == "Актуальная ведомость" &&
                            table.Data.TryGetValue("Итого в БРС", out var csharpStr) &&
                            double.TryParse(csharpStr, out var csharp))
                        {
                            calculatedPoints["ПРОЕКТИРОВАНИЕ_C#"] = csharp;
                        }
                    }
                    else if (table.TableName == "ПЯТИМИНУТКИ ТЕРВЕР")
                    {
                        if (table.Data.TryGetValue("ИТОГО", out var pytStr) &&
                            double.TryParse(pytStr, out var pyt))
                        {
                            calculatedPoints["ПЯТИМИНУТКИ_ТЕРВЕР"] = pyt;
                        }
                    }
                }
                
                // Рассчитываем общий балл БРС (примерная формула с весами)
                double totalBRS = 0;
                double totalWeight = 0;
                
                if (calculatedPoints.ContainsKey("МАТАН_ПРАКТИКА")) 
                {
                    totalBRS += calculatedPoints["МАТАН_ПРАКТИКА"] * 0.3;
                    totalWeight += 0.3;
                }
                if (calculatedPoints.ContainsKey("ТЕРВЕР_ЛЕКЦИИ")) 
                {
                    totalBRS += calculatedPoints["ТЕРВЕР_ЛЕКЦИИ"] * 0.2;
                    totalWeight += 0.2;
                }
                if (calculatedPoints.ContainsKey("СЕТИ")) 
                {
                    totalBRS += calculatedPoints["СЕТИ"] * 0.2;
                    totalWeight += 0.2;
                }
                if (calculatedPoints.ContainsKey("ПРОЕКТИРОВАНИЕ_C#")) 
                {
                    totalBRS += calculatedPoints["ПРОЕКТИРОВАНИЕ_C#"] * 0.3;
                    totalWeight += 0.3;
                }
                
                // Нормализуем на случай, если не все веса учтены
                if (totalWeight > 0)
                {
                    totalBRS = totalBRS / totalWeight * 100; // Приводим к 100-балльной шкале
                }
                
                return Ok(new
                {
                    Student = studentName,
                    SubjectPoints = calculatedPoints,
                    TotalBRS = Math.Round(totalBRS, 2),
                    CalculationDate = DateTime.UtcNow,
                    Note = "Это примерный расчет. Реальные веса предметов могут отличаться."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // Метод для отладки - ДОБАВИТЬ ЭТОТ МЕТОД!
        [HttpGet("debug")]
        public IActionResult Debug()
        {
            var configPath = GetConfigPath();
            
            var info = new
            {
                ContentRootPath = _env.ContentRootPath,
                BaseDirectory = AppContext.BaseDirectory,
                CurrentDirectory = Directory.GetCurrentDirectory(),
                ConfigPath = configPath,
                ConfigExists = !string.IsNullOrEmpty(configPath) && System.IO.File.Exists(configPath),
                
                // Проверяем существование файлов
                PossiblePaths = new[]
                {
                    Path.Combine(_env.ContentRootPath, "config.json"),
                    Path.Combine(AppContext.BaseDirectory, "config.json"),
                    Path.Combine(Directory.GetCurrentDirectory(), "config.json"),
                    "../FiitFlow.Parser/config.json",
                    "../../FiitFlow.Parser/config.json"
                }.Select(p => new { 
                    Path = p, 
                    AbsolutePath = Path.GetFullPath(p),
                    Exists = System.IO.File.Exists(Path.GetFullPath(p))
                })
            };
            
            return Ok(info);
        }

        private string? GetConfigPath()
        {
            // Сначала пробуем получить путь из конфигурации
            var configPath = _configuration["ParserConfig:ConfigPath"];
            
            if (!string.IsNullOrEmpty(configPath))
            {
                var absolutePath = Path.GetFullPath(configPath);
                if (System.IO.File.Exists(absolutePath))
                {
                    return absolutePath;
                }
            }
            
            // Список возможных местоположений
            var possiblePaths = new[]
            {
                "../FiitFlow.Parser/config.json",
                "../../FiitFlow.Parser/config.json",
                "config.json",
                Path.Combine(_env.ContentRootPath, "config.json"),
                Path.Combine(AppContext.BaseDirectory, "config.json"),
                Path.Combine(Directory.GetCurrentDirectory(), "config.json")
            };
            
            foreach (var path in possiblePaths)
            {
                var absolutePath = Path.GetFullPath(path);
                if (System.IO.File.Exists(absolutePath))
                {
                    return absolutePath;
                }
            }
            
            return null;
        }
    }
}
