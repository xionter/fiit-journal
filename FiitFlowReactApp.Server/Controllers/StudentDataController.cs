using Microsoft.AspNetCore.Mvc;
using FiitFlow.Parser.Services;
using FiitFlow.Parser.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.IO;

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

        [HttpGet("{studentName}")]
        public async Task<ActionResult<StudentSearchResult>> GetStudentData(string studentName)
        {
            try
            {
                // Получаем путь из конфигурации
                var configPath = _configuration["ParserConfig:ConfigPath"] ?? "../FiitFlow.Parser/config.json";
                
                // Преобразуем в абсолютный путь
                var absolutePath = Path.GetFullPath(configPath);
                
                if (!System.IO.File.Exists(absolutePath))
                {
                    // Попробуем альтернативные пути
                    absolutePath = TryFindConfigFile();
                    
                    if (string.IsNullOrEmpty(absolutePath))
                    {
                        return NotFound(new { 
                            error = "Конфиг не найден",
                            configPathFromSettings = configPath,
                            absolutePath = Path.GetFullPath(configPath)
                        });
                    }
                }
                
                Console.WriteLine($"Использую конфиг: {absolutePath}");
                var result = await _parserService.ParseAsync(absolutePath, studentName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private string? TryFindConfigFile()
        {
            // Список возможных местоположений
            var possiblePaths = new[]
            {
                "../FiitFlow.Parser/config.json",
                "../../FiitFlow.Parser/config.json",
                "config.json",
                Path.Combine(_env.ContentRootPath, "config.json"),
                Path.Combine(AppContext.BaseDirectory, "config.json")
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
