using System;
using System.IO;
using System.Text;
using System.Text.Json;
using FiitFlow.Parser.Models;

namespace FiitFlow.Parser.Services
{
    public sealed class ConfigEditorService
    {
        private readonly string _configPath;
        private readonly object _sync = new();

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            WriteIndented = true
        };

        public ConfigEditorService(string configPath)
        {
            if (string.IsNullOrWhiteSpace(configPath))
                throw new ArgumentException("Config path is empty.", nameof(configPath));

            _configPath = configPath;
        }
        public IReadOnlyList<string> ListSubjects() =>
           Load()
               .Formulas
               .SubjectFormulas
               .Select(s => s.SubjectName)
               .Where(n => !string.IsNullOrWhiteSpace(n))
               .Distinct(System.StringComparer.OrdinalIgnoreCase)
               .OrderBy(n => n)
               .ToList();

        public ParserConfig Load()
        {
            lock (_sync)
            {
                if (!File.Exists(_configPath))
                    return new ParserConfig();

                var json = File.ReadAllText(_configPath, Encoding.UTF8);
                if (string.IsNullOrWhiteSpace(json))
                    return new ParserConfig();

                var cfg = JsonSerializer.Deserialize<ParserConfig>(json, JsonOptions);
                return cfg ?? new ParserConfig();
            }
        }

        public void Save(ParserConfig config)
        {
            if (config is null) throw new ArgumentNullException(nameof(config));

            lock (_sync)
            {
                var dir = Path.GetDirectoryName(_configPath);
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);

                var json = JsonSerializer.Serialize(config, JsonOptions);

                var tmp = _configPath + ".tmp";
                File.WriteAllText(tmp, json, Encoding.UTF8);

                // максимально атомарно
                if (File.Exists(_configPath))
                {
                    try
                    {
                        File.Replace(tmp, _configPath, null);
                        return;
                    }
                    catch
                    {
                        // если Replace недоступен (например, на некоторых FS), упадём на Move
                    }

                    File.Delete(_configPath);
                }

                File.Move(tmp, _configPath);
            }
        }

        /// <summary>
        /// Удобное редактирование: Load -> apply -> Save.
        /// </summary>
        public ParserConfig Edit(Action<ParserConfig> edit)
        {
            if (edit is null) throw new ArgumentNullException(nameof(edit));

            lock (_sync)
            {
                var cfg = Load();
                edit(cfg);
                Save(cfg);
                return cfg;
            }
        }

        // Примеры готовых высокоуровневых операций:

        public ParserConfig SetStudentName(string name) =>
            Edit(cfg => cfg.StudentName = name ?? string.Empty);

        public ParserConfig SetCache(string cacheDirectory, bool forceRefresh) =>
            Edit(cfg =>
            {
                cfg.CacheSettings.CacheDirectory = string.IsNullOrWhiteSpace(cacheDirectory) ? "./Cache" : cacheDirectory;
                cfg.CacheSettings.ForceRefresh = forceRefresh;
            });

        public ParserConfig UpsertTable(TableConfig table) =>
            Edit(cfg =>
            {
                if (table == null) throw new ArgumentNullException(nameof(table));
                if (string.IsNullOrWhiteSpace(table.Name)) throw new ArgumentException("Table.Name is empty.", nameof(table));

                var existing = cfg.Tables.Find(t => string.Equals(t.Name, table.Name, StringComparison.OrdinalIgnoreCase));
                if (existing == null)
                {
                    cfg.Tables.Add(table);
                    return;
                }

                existing.Url = table.Url ?? string.Empty;
                existing.Sheets = table.Sheets ?? new();
            });

        public ParserConfig RemoveTable(string tableName) =>
            Edit(cfg =>
            {
                cfg.Tables.RemoveAll(t => string.Equals(t.Name, tableName, StringComparison.OrdinalIgnoreCase));
            });
    }
}
