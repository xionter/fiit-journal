using System.IO;
using System.Linq;

namespace FiitFlow.Parser.Services
{
    public interface IStudentConfigService
    {
        void EnsureStudentConfig(string studentFullName, string groupTitle, int subgroup);
    }

    public class StudentConfigService : IStudentConfigService
    {
        private readonly string _rootPath;

        public StudentConfigService(string rootPath)
        {
            _rootPath = rootPath;
        }

        public void EnsureStudentConfig(string studentFullName, string groupTitle, int subgroup)
        {
            var groupConfigDirectory = Path.Combine(_rootPath, "cfg", groupTitle);
            Directory.CreateDirectory(groupConfigDirectory);

            var studentConfigPath = Path.Combine(groupConfigDirectory, $"{studentFullName}.json");
            if (File.Exists(studentConfigPath))
                return;

            var defaultConfigPath = ResolveDefaultConfigPath(groupConfigDirectory, subgroup);
            File.Copy(defaultConfigPath, studentConfigPath);
            var ConfEditor = new ConfigEditorService(studentConfigPath).SetStudentName(studentFullName);
        }

        private string ResolveDefaultConfigPath(string groupConfigDirectory, int subgroup)
        {
            var expectedFileName = $"default{subgroup}.json";
            var expectedPath = Path.Combine(groupConfigDirectory, expectedFileName);

            if (File.Exists(expectedPath))
                return expectedPath;

            var fallbackPath = Directory
                .EnumerateFiles(groupConfigDirectory, $"default*{subgroup}*.json", SearchOption.TopDirectoryOnly)
                .Concat(Directory.EnumerateFiles(groupConfigDirectory, "default*.json", SearchOption.TopDirectoryOnly))
                .FirstOrDefault();

            if (fallbackPath is null)
                throw new FileNotFoundException($"Default config template not found for subgroup {subgroup} in {groupConfigDirectory}");

            return fallbackPath;
        }
    }
}
