using System;
using System.IO;
using System.Linq;
using FiitFlow.Parser.Models;

namespace FiitFlow.Parser.Services
{
    public class CacheService
    {
        private readonly string _cacheDirectory;
        private readonly bool _forceRefresh;

        public CacheService(string cacheDirectory, bool forceRefresh = false)
        {
            _cacheDirectory = cacheDirectory;
            _forceRefresh = forceRefresh;
            Directory.CreateDirectory(cacheDirectory);
        }

        public string GetCachedFilePath(TableConfig table)
        {
            var cleanTableName = SanitizeFileName(table.Name);
            return Path.Combine(_cacheDirectory, $"{cleanTableName}.xlsx");
        }

        public bool ShouldDownload(TableConfig table)
        {
            if (_forceRefresh) return true;
            
            var cachedFile = GetCachedFilePath(table);
            return !File.Exists(cachedFile);
        }

        public string? GetCachedFile(TableConfig table)
        {
            var cachedFile = GetCachedFilePath(table);
            return File.Exists(cachedFile) ? cachedFile : null;
        }

        private static string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return "unknown_table";
                
            var invalidChars = Path.GetInvalidFileNameChars();
            var cleanName = new string(fileName
                .Where(ch => !invalidChars.Contains(ch))
                .ToArray())
                .Replace(" ", "_")
                .Trim();
                
            return string.IsNullOrEmpty(cleanName) ? "unknown_table" : cleanName;
        }
    }
}
