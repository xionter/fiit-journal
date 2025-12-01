using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;

namespace FiitFlowReactApp.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;

        public AuthController(ILogger<AuthController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] string studentName, [FromBody] string studentGroup)
        {
            return Ok(new
            {
                studentName = studentName,
                studentGroup = studentGroup,
                expiresIn = 3600
            });
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
