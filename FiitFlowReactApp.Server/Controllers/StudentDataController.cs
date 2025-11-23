using Microsoft.AspNetCore.Mvc;
using FiitFlow.Parser.Services;

namespace FiitFlowReactApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentDataController : ControllerBase
    {
        private readonly ILogger<StudentDataController> _logger;

        public StudentDataController(ILogger<StudentDataController> logger)
        {
            _logger = logger;
        }

        [HttpGet("{studentName}")]
        public async Task<IActionResult> GetStudentData(string studentName)
        {
            try
            {
                var parserService = new FiitFlowParserService();
                var configPath = Path.Combine("..", "..", "..", "FiitFlow.Parser", "config.txt");
                var result = await parserService.ParseAsync(configPath);
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

