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

        [HttpGet("login")]
        public IActionResult Login(
            [FromQuery] string firstName,
            [FromQuery] string lastName,
            [FromQuery] string group,
            [FromQuery] DateTime dateTime)
        {
            return Ok(new
            {
                id = (lastName + firstName + group + DateTime.Now.Date.ToString()).GetHashCode(),
                expiresIn = 3600
            });
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
