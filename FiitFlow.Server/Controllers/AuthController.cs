using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;

namespace FiitFlowReactApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableCors("ReactClient")]
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
        public IActionResult Post(
            [FromQuery] string firstName,
            [FromQuery] string lastName,
            [FromQuery] string group,
            [FromQuery] DateTime dateTime)
        {
            return Ok((lastName + firstName + group + DateTime.Now.Date.ToString()).GetHashCode());
        }
    }
}
