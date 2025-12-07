using Microsoft.AspNetCore.Mvc;
using FiitFlow.Parser.Services;
using FiitFlow.Parser.Models;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IOFile = System.IO.File;
namespace FiitFlow.Parser.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentDataController : ControllerBase
    {
        private readonly FiitFlowParserService _parserService;
        private readonly string _configPath;

        public StudentDataController()
        {
            _parserService = new FiitFlowParserService();
            _configPath = GetConfigPath();
        }

        [HttpGet("{studentName}")]
        public async Task<ActionResult<StudentSearchResult>> GetStudentData(string studentName)
        {
            try
            {
                Console.WriteLine($"Использую конфиг: {_configPath}");
                var result = await _parserService.ParseAsync(_configPath, studentName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("with-formulas/{studentName}")]
        public async Task<ActionResult<StudentResults>> GetStudentDataWithFormulas(string studentName)
        {
            try
            {
                Console.WriteLine($"Использую конфиг для формул: {_configPath}");

                var result = await _parserService.ParseWithFormulasAsync(_configPath, studentName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, details = ex.StackTrace });
            }
        }

        [HttpGet("sum-test/{studentName}/{columnName}")]
        public async Task<IActionResult> SumTest(string studentName, string columnName)
        {
            try
            {
                var result = await _parserService.ParseAsync(_configPath, studentName);

                var values = new List<object>();
                double sum = 0;

                foreach (var table in result.Tables)
                {
                    foreach (var kvp in table.Data)
                    {
                        if (string.Equals(kvp.Key, columnName, StringComparison.OrdinalIgnoreCase))
                        {
                            if (double.TryParse(kvp.Value.Replace(',', '.'), out double value))
                            {
                                values.Add(new
                                {
                                    Table = table.TableName,
                                    Sheet = table.SheetName,
                                    Value = kvp.Value,
                                    NumericValue = value
                                });
                                sum += value;
                            }
                            else
                            {
                                values.Add(new
                                {
                                    Table = table.TableName,
                                    Sheet = table.SheetName,
                                    Value = kvp.Value,
                                    NumericValue = 0
                                });
                            }
                        }
                    }
                }

                return Ok(new
                {
                    Student = studentName,
                    ColumnName = columnName,
                    Values = values,
                    Count = values.Count,
                    Sum = Math.Round(sum, 2)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("debug")]
        public IActionResult Debug()
        {
            var info = new
            {
                ConfigPath = _configPath,
                ConfigExists = IOFile.Exists(_configPath),
                FileSize = IOFile.Exists(_configPath) ? new FileInfo(_configPath).Length : 0,
                LastModified = IOFile.Exists(_configPath) ? IOFile.GetLastWriteTime(_configPath) : DateTime.MinValue
            };

            return Ok(info);
        }

        private string GetConfigPath()
        {
            var solutionRoot = Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory,
                "../../../../"));

            var configPath = Path.Combine(solutionRoot, "FiitFlow.Parser", "config.json");

            Console.WriteLine($"Путь к конфигу: {configPath}");

            if (!IOFile.Exists(configPath))
            {
                throw new FileNotFoundException($"Конфиг не найден: {configPath}");
            }

            return configPath;
        }
    }
}
