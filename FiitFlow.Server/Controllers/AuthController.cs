using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;

namespace FiitFlow.Server.Controllers
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
            return Ok(string.Join("3453", new[] { lastName, firstName, dateTime.Date.ToString(), group }).GetHashCode());
        }
    }
}
