using DocumentFormat.OpenXml.Wordprocessing;
using FiitFlow.Repository;
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
        private readonly IStudentRepository _studentRepository;
        private readonly IGroupRepository _groupRepository;

        public AuthController(
            ILogger<AuthController> logger,
            IConfiguration configuration,
            IStudentRepository studentRepository,
            IGroupRepository groupRepository)
        {
            _logger = logger;
            _configuration = configuration;
            _studentRepository = studentRepository;
            _groupRepository = groupRepository;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Post([FromBody] StudentLoginRequest request)
        {
            if (!CheckStudentData(request.FirstName, request.LastName, request.Group))
                return BadRequest("Wrong format");
            var hashId = await TryGetStudentGuid(request.FirstName, request.LastName, request.Group);
            if (hashId == null)
                return BadRequest("Undefind student");
            return Ok(hashId);
        }

        private bool CheckStudentData(string firstName, string lastName, string group) =>
            Regex.IsMatch(firstName, "^[А-ЯЁ][а-яё]*$") &&
            Regex.IsMatch(lastName, "^[А-ЯЁ][а-яё]*$") &&
            Regex.IsMatch(group, "^ФТ-[0-9]{3}-[0-9]$");

        private async Task<int?> TryGetStudentGuid(string firstName, string lastName, string group)
        {
            var groupEntity = await _groupRepository.GetByTitleAsync(group.Substring(0, 6), int.Parse(group.Substring(7, 1)));
            if (groupEntity == null)
                return null;
            var student = await _studentRepository.GetByNameAsync(lastName + " " + firstName, groupEntity.Id);
            if (student == null)
                return null;
            return (student.Id.ToString() + groupEntity.Id.ToString()).GetHashCode();
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
