using Microsoft.AspNetCore.Mvc;
using FiitFlow.Parser.Services;
using Microsoft.Extensions.Configuration;

namespace FiitFlowReactApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentDataController : ControllerBase
    {
        private readonly ILogger<StudentDataController> _logger;
        private readonly IConfiguration _configuration;

        public StudentDataController(ILogger<StudentDataController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet("{studentName}")]
        public async Task<IActionResult> GetStudentData(string studentName)
        {
            try
            {
                _logger.LogInformation($"Current directory: {Directory.GetCurrentDirectory()}");

                using var httpClient = new HttpClient();
                var parserService = new FiitFlowParserService(httpClient);

                // Try multiple possible paths
                var possiblePaths = new[]
                {
                    "../FiitFlow.Parser/config.txt",
                    "../../../FiitFlow.Parser/config.txt",
                    "/home/xionter/proga/c-sharp/study/fiit-journal/FiitFlow.Parser/config.txt"
                };

                string configPath = null;
                foreach (var path in possiblePaths)
                {
                    var fullPath = Path.GetFullPath(path);
                    _logger.LogInformation($"Checking: {fullPath}");
                    if (System.IO.File.Exists(fullPath))
                    {
                        configPath = fullPath;
                        break;
                    }
                }

                if (configPath == null)
                {
                    return NotFound("Config file not found in any expected location");
                }

                _logger.LogInformation($"Using config: {configPath}");
                var result = await parserService.ParseAsync(configPath, studentName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running parser");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
