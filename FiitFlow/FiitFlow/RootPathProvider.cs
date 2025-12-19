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
            var candidates = new List<string?>
            {
                Environment.GetEnvironmentVariable("FIITFLOW_ROOT_PATH"),
                AppContext.BaseDirectory,
                Directory.GetCurrentDirectory(),
                Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../.."))
            };

            var normalizedCandidates = candidates
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Select(Path.GetFullPath)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            // Prefer a location that actually contains cfg/ so container mounts are resolved correctly.
            _rootPath = normalizedCandidates
                .FirstOrDefault(c => Directory.Exists(Path.Combine(c, "cfg")))
                ?? normalizedCandidates.First();
        }

        public string GetRootPath() => _rootPath;
    }
}
