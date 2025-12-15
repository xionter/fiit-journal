using FiitFlow.Server.SubTools;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace FiitFlow.Server.Controllers
{
    [ApiController]
    [EnableCors("AllowLocalhost")]
    [Route("api/[controller]")]
    public class ConfigEditController : Controller
    {
        private readonly ILogger<ConfigEditController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IAuthentication _authentication;

        public ConfigEditController(
            ILogger<ConfigEditController> logger,
            IConfiguration configuration,
            IAuthentication authentication)
        {
            _logger = logger;
            _configuration = configuration;
            _authentication = authentication;
        }

        [HttpGet]
        public 
    }
}
