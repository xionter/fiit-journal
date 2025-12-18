using FiitFlow.Parser.Models;
using FiitFlow.Parser.Services;
using FiitFlow.Server.SubTools;
using FiitFlow.Server.SubTools.SubToolsUnits;
using FiitFlow.Domain.Extensions;
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
        private readonly IRootPathProvider _rootPathProvider;

        public ConfigEditController(
            ILogger<ConfigEditController> logger,
            IConfiguration configuration,
            IAuthentication authentication,
            IRootPathProvider rootPathProvider)
        {
            _logger = logger;
            _configuration = configuration;
            _authentication = authentication;
            _rootPathProvider = rootPathProvider;
        }

        [HttpGet("GetConfigs")]
        public async Task<ActionResult<IEnumerable<SubjectConfigSimple>>> GetSubjectConfigs(
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
            var configEditor = new ConfigEditorService(Path.Combine(
                _rootPathProvider.GetRootPath(), "cfg", group.Substring(0, 6), $"{lastName} {firstName}.json"));
            var result = configEditor.Load().Subjects.Select(subCon => new SubjectConfigSimple
            {
                BaseName = subCon.SubjectName,
                Name = subCon.SubjectName,
                Link = subCon.Tables.First().Url,
                Formula = subCon.Formula.FinalFormula ?? "",
                Sheets = subCon.Tables.First().Sheets.Select(s => new SheetSimple { sheetName = s.Name, headerRow = s.CategoriesRow ?? 1 }).ToArray()
            });
            _logger.LogCritical(result.Count().ToString());
            return Ok(result);
        }

        [HttpPost("SetConfigs")]
        public async Task<IActionResult> SetSubjectConfigs(
            [FromQuery] int id,
            [FromQuery] string firstName,
            [FromQuery] string lastName,
            [FromQuery] string group,
            [FromQuery] int term,
            [FromQuery] DateTime dateTime,
            [FromBody] IEnumerable<SubjectConfigSimple> subjectConfigs)
        {
            var authResponse = await _authentication.FindAuthIdByLoginForm(firstName, lastName, group);
            if (!authResponse.Accepted)
                return Unauthorized(new { message = "Данные пользователя неверны", errorCode = "AUTH_REJECTED" });
            var configEditor = new ConfigEditorService(Path.Combine(
                _rootPathProvider.GetRootPath(), "cfg", group.Substring(0, 6), $"{lastName} {firstName}.json"));
            var beforeSubjects = configEditor.Load().Subjects;
            foreach (var subjectConfig in subjectConfigs)
            {
                var removed = false;
                if (subjectConfig.BaseName.Length > 0 && beforeSubjects
                    .Where(bef => bef.SubjectName == subjectConfig.BaseName)
                    .Select(bef =>
                    bef.SubjectName != subjectConfig.Name ||
                    bef.Tables.First().Url != subjectConfig.Link ||
                    bef.Formula.FinalFormula != subjectConfig.Formula).FirstOrDefault(false))
                {
                    configEditor.RemoveSubject(subjectConfig.BaseName);
                    removed = true;
                }
                if (subjectConfig.BaseName.Length == 0 || removed)
                {
                    configEditor.CreateSubject(
                        subjectConfig.Name,
                        "1",
                        subjectConfig.Link,
                        subjectConfig.Sheets.Select(s => (s.sheetName, s.headerRow)),
                        subjectConfig.Formula);
                }
            }
            foreach (var befSub in beforeSubjects)
            {
                if (subjectConfigs.Where(subCon => subCon.BaseName == befSub.SubjectName).Count() == 0)
                    configEditor.RemoveSubject(befSub.SubjectName);
            }
            return Ok();
        }
    }
}
