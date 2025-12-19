using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FiitFlow.Domain.Extensions
{
    public interface IRootPathProvider
    {
        string GetRootPath();
    }

    public class RootPathProvider : IRootPathProvider
    {
        private readonly string _rootPath;

        public RootPathProvider()
        {
            var seeds = new List<string?>
            {
                Environment.GetEnvironmentVariable("FIITFLOW_ROOT_PATH"),
                AppContext.BaseDirectory,
                Directory.GetCurrentDirectory(),
                Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../.."))
            };

            // Walk up from each seed to find a directory that actually contains cfg/
            var normalizedCandidates = seeds
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Select(Path.GetFullPath)
                .SelectMany(ExpandWithParents)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            _rootPath = normalizedCandidates
                .FirstOrDefault(c => Directory.Exists(Path.Combine(c, "cfg")))
                ?? normalizedCandidates.First();
        }

        private static IEnumerable<string> ExpandWithParents(string path)
        {
            var current = new DirectoryInfo(path);
            while (current != null)
            {
                yield return current.FullName;
                current = current.Parent;
            }
        }

        public string GetRootPath() => _rootPath;
    }
}
