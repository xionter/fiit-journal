using System;
using System.IO;

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
            _rootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
        }

        public string GetRootPath() => _rootPath;
    }
}
