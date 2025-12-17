using FiitFlow.Parser.Services;
using FiitFlow.Repository;
using System.Text.RegularExpressions;

namespace FiitFlow.Server.SubTools.SubToolsUnits
{
    public class AuthenticationTool : IAuthentication
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IStudentConfigService _studentConfigService;

        public AuthenticationTool(
            IStudentRepository studentRepository,
            IStudentConfigService studentConfigService)
        {
            this._studentRepository = studentRepository;
            _studentConfigService = studentConfigService;
        }

        public async Task<AuthResponse<int>> FindAuthIdByLoginForm(string firstName, string lastName, string groupFull)
        {
            if (!CheckLoginFormCorrect(firstName, lastName, groupFull))
                return new AuthResponse<int>(false, 0, "Incorrect login data form");
            var groupTitle = groupFull.Substring(0, 6);
            var subgroup = int.Parse(groupFull.Substring(7));
            var student = await _studentRepository.GetOrCreateAsync(firstName, lastName, groupTitle, subgroup);
            if (student == null)
                return new AuthResponse<int>(false, 0, "User was not found");

            try
            {
                _studentConfigService.EnsureStudentConfig(student.FullName, groupTitle, subgroup);
            }
            catch (Exception ex)
            {
                return new AuthResponse<int>(false, student.Id, $"Config preparation failed: {ex.Message}");
            }
            return new AuthResponse<int>(true, student.Id, string.Empty);
        }

        private bool CheckLoginFormCorrect(string firstName, string lastName, string groupFull) =>
            Regex.IsMatch(firstName, "^[А-ЯЁ][а-яё]*$") &&
            Regex.IsMatch(lastName, "^[А-ЯЁ][а-яё]*$") &&
            Regex.IsMatch(groupFull, "^ФТ-[0-9]{3}-[0-9]$");
    }
}
