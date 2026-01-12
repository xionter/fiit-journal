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
            foreach (var subjectConfig in subjectConfigs)
            {
                var baseName = subjectConfig.BaseName?.Trim() ?? string.Empty;
                var desiredName = subjectConfig.Name?.Trim() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(baseName))
                {
                    configEditor.CreateSubject(
                        desiredName,
                        "1",
                        subjectConfig.Link,
                        subjectConfig.Sheets.Select(s => (s.sheetName, s.headerRow)),
                        subjectConfig.Formula);
                    continue;
                }

                configEditor.Edit(cfg =>
                {
                    var subject = cfg.Subjects.FirstOrDefault(s =>
                        string.Equals(s.SubjectName, baseName, StringComparison.OrdinalIgnoreCase));

                    if (subject == null)
                    {
                        var newSubject = new SubjectConfig
                        {
                            SubjectName = desiredName,
                            Tables = new List<TableConfig>
                            {
                                new TableConfig
                                {
                                    Name = "1",
                                    Url = subjectConfig.Link ?? string.Empty,
                                    Sheets = subjectConfig.Sheets
                                        .Select(s => new SheetConfig
                                        {
                                            Name = s.sheetName ?? string.Empty,
                                            CategoriesRow = s.headerRow
                                        })
                                        .ToList()
                                }
                            },
                            Formula = new SubjectFormula
                            {
                                FinalFormula = subjectConfig.Formula ?? string.Empty
                            }
                        };
                        cfg.Subjects.Add(newSubject);
                        return;
                    }

                    if (!string.Equals(subject.SubjectName, desiredName, StringComparison.OrdinalIgnoreCase))
                        subject.SubjectName = desiredName;

                    subject.Formula ??= new SubjectFormula();
                    subject.Formula.FinalFormula = subjectConfig.Formula ?? string.Empty;

                    var table = subject.Tables.FirstOrDefault();
                    if (table == null)
                    {
                        table = new TableConfig { Name = "1" };
                        subject.Tables.Add(table);
                    }

                    table.Url = subjectConfig.Link ?? string.Empty;
                    table.Sheets = subjectConfig.Sheets
                        .Select(s => new SheetConfig
                        {
                            Name = s.sheetName ?? string.Empty,
                            CategoriesRow = s.headerRow
                        })
                        .ToList();
                });
            }
            return Ok();
        }

        private static bool SheetsEqual(SheetSimple[]? left, SheetSimple[]? right)
        {
            left ??= Array.Empty<SheetSimple>();
            right ??= Array.Empty<SheetSimple>();

            if (left.Length != right.Length)
                return false;

            for (var i = 0; i < left.Length; i++)
            {
                if (!string.Equals(left[i].sheetName, right[i].sheetName, StringComparison.OrdinalIgnoreCase))
                    return false;
                if (left[i].headerRow != right[i].headerRow)
                    return false;
            }

            return true;
        }
    }
}
