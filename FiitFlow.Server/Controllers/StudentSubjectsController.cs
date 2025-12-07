using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace FiitFlowReactApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentSubjectsController : Controller
    {
        private readonly ILogger<StudentSubjectsController> _logger;
        private readonly IConfiguration _configuration;

        public StudentSubjectsController(
            ILogger<StudentSubjectsController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet]
        public IEnumerable<SubjectUnit> Get(
            [FromQuery] long id,
            [FromQuery] string firstName,
            [FromQuery] string secondName,
            [FromQuery] string group,
            [FromQuery] DateTime dateTime)
        {
            return Enumerable.Range(1, 10).Select(n => new SubjectUnit
            {
                Subject = $"Subject{n}",
                Teacher = $"Teacher {secondName} {firstName} - {id} - {group}",
                Score = 8.34245f * n,
                LastUpdate = DateTime.Now.ToString()
            }).ToArray();
        }
    }
}

public class SubjectUnit
{
    public string Subject { get; set; }
    public string Teacher { get; set; }
    public float Score { get; set; }
    public string LastUpdate { get; set; }
}
