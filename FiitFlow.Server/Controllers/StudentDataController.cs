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
        private readonly FiitFlowParserService _parserService;

        public StudentDataController(
                ILogger<StudentDataController> logger,
                IConfiguration configuration,
                FiitFlowParserService parserService)
        {
            _logger = logger;
            _configuration = configuration;
            _parserService = parserService;
        }

        [HttpGet("{studentName}")]
        public async Task<IActionResult> GetStudentData(string studentName)
        {
            try
            {
                var configPath = FindConfigPath();
                if (configPath == null)
                    return NotFound("Config file not found");

                var result =
                    await _parserService.ParseAsync(configPath, studentName);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running parser");
                return StatusCode(500, ex.Message);
            }
        }
        private string? FindConfigPath()
        {
            var possiblePaths = new[]
            {
                "../FiitFlow.Parser/config.json",
                "../../../FiitFlow.Parser/config.json",
                "/home/xionter/proga/c-sharp/study/fiit-journal/FiitFlow.Parser/config.json"
            };

            foreach (var path in possiblePaths)
            {
                var fullPath = Path.GetFullPath(path);
                if (System.IO.File.Exists(fullPath))
                    return fullPath;
            }
            return null;
        }
    }
}
