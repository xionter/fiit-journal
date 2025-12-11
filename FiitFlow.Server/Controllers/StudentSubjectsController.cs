using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace FiitFlowReactApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableCors("ReactClient")]
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

        [HttpGet("All")]
        public IEnumerable<SubjectUnit> Get(
            [FromQuery] long id,
            [FromQuery] string firstName,
            [FromQuery] string lastName,
            [FromQuery] string group,
            [FromQuery] int term,
            [FromQuery] DateTime dateTime)
        {
            return Enumerable.Range(1, 10).Select(n => new SubjectUnit
            {
                Subject = $"Subject{n}-{term}",
                Teacher = $"Teacher {lastName} {firstName} - {id} - {group}",
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
