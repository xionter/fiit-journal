using Microsoft.AspNetCore.Mvc;

namespace FiitFlowReactApp.Server.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
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
        public IEnumerable<SubjectUnit> Get()
        {
            return Enumerable.Range(1, 10).Select(n => new SubjectUnit
            {
                Subject = $"Subject{n}",
                Teacher = $"TEacher {n}",
                Score = 8.34245f * n,
                LastUpdate = DateTime.Now
            }).ToArray();
        }
    }
}

public class SubjectUnit
{
    public string Subject { get; set; }
    public string Teacher { get; set; }
    public float Score { get; set; }
    public DateTime LastUpdate { get; set; }
}
