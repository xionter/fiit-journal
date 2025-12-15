using DocumentFormat.OpenXml.Bibliography;
using FiitFlow.Repository;
using FiitFlow.Server.SubTools;
using FiitFlow.Server.SubTools.SubToolsUnits;
using Google.Apis.Sheets.v4.Data;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace FiitFlowReactApp.Server.Controllers
{
    [ApiController]
    [EnableCors("AllowLocalhost")]
    [Route("api/[controller]")]
    public class StudentSubjectsController : Controller
    {
        private readonly ILogger<StudentSubjectsController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IAuthentication _authentication;
        private readonly IPointsRepository _pointsRepository;

        public StudentSubjectsController(
            ILogger<StudentSubjectsController> logger,
            IConfiguration configuration,
            IAuthentication authentication,
            IPointsRepository pointsRepository)
        {
            _logger = logger;
            _configuration = configuration;
            _authentication = authentication;
            _pointsRepository = pointsRepository;
        }

        [HttpGet("All")]
        public async Task<ActionResult<IEnumerable<PointsSimple>>> GetAll(
            [FromQuery] int id,
            [FromQuery] string firstName,
            [FromQuery] string lastName,
            [FromQuery] string group,
            [FromQuery] int term,
            [FromQuery] DateTime dateTime)
        {
            var authResponse = await _authentication.FindAuthIdByLoginForm(firstName, lastName, group);
            if (!authResponse.Accepted)
                return Unauthorized(new { message = "Данные пользователя неверны", errorCode = "AUTH_REJECTED" });
            var studentPoints = await _pointsRepository.GetByStudentAsync(id);
            var result = studentPoints
                .Where(points => points.Semester == term)
                .Select(points => new PointsSimple
                {
                    Subject = points.Subject.Title,
                    Teacher = "-",
                    Score = points.Value,
                    LastUpdate = points.UpdatedAt
                });
            return Ok(result);
        }
    }
}
