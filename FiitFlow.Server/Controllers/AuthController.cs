using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;

namespace FiitFlowReactApp.Server.Controllers
{
    [ApiController]
    [EnableCors("AllowLocalhost")]
    [Route("api/[controller]")]
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
        public IActionResult Post([FromBody] StudentLoginRequest request)
        {
            return Ok(string.Join("3453", new[] {
                request.LastName,
                request.FirstName,
                request.DateTime.Date.ToString(),
                request.Group
            }).GetHashCode());
        }
    }

    public class StudentLoginRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Group { get; set; }
        public DateTime DateTime { get; set; }
    }
}
