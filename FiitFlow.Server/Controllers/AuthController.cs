using DocumentFormat.OpenXml.Wordprocessing;
using FiitFlow.Repository;
using FiitFlow.Server.SubTools;
using FiitFlow.Server.SubTools.SubToolsUnits;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FiitFlowReactApp.Server.Controllers
{
    [ApiController]
    [EnableCors("AllowLocalhost")]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IAuthentication _authentication;

        public AuthController(
            ILogger<AuthController> logger,
            IConfiguration configuration,
            IAuthentication authentication)
        {
            _logger = logger;
            _configuration = configuration;
            _authentication = authentication;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Post([FromBody] StudentLoginRequest request)
        {
            var authResponse = await _authentication.FindAuthIdByLoginForm(request.FirstName, request.LastName, request.Group);
            if (authResponse.Accepted)
                return Ok(authResponse.Data);
            return BadRequest(authResponse.exceptionMessage);
        }
    }
}
