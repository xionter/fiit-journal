using FiitFlow.Parser.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace FiitFlow.Parser.Services
{
    public sealed class ConfigEditorService
    {
        private readonly string _configPath;
        private readonly object _sync = new();
        private static readonly StringComparer NameComparer = StringComparer.OrdinalIgnoreCase;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public ConfigEditorService(string configPath)
        {
            if (string.IsNullOrWhiteSpace(configPath))
                throw new ArgumentException("Config path is empty.", nameof(configPath));

            _configPath = configPath;
        }

        public IReadOnlyList<string> ListSubjects() =>
           Load()
               .Subjects
               .Select(s => s.SubjectName)
               .Where(n => !string.IsNullOrWhiteSpace(n))
               .Distinct(NameComparer)
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

                if (File.Exists(_configPath))
                {
                    try
                    {
                        File.Replace(tmp, _configPath, null);
                        return;
                    }
                    catch
                    {
                    }

                    File.Delete(_configPath);
                }

                File.Move(tmp, _configPath);
            }
        }

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

        private static void EnsureCollections(ParserConfig config)
        {
            config.Subjects ??= new List<SubjectConfig>();
        }

        private SubjectConfig EnsureSubject(ParserConfig config, string subjectName, bool allowExisting = true)
        {
            EnsureCollections(config);

            var existing = config.Subjects.FirstOrDefault(f => NameComparer.Equals(f.SubjectName, subjectName));
            if (existing != null)
            {
                if (!allowExisting)
                    throw new InvalidOperationException($"Предмет \"{subjectName}\" уже существует.");

                return existing;
            }

            var subject = new SubjectConfig
            {
                SubjectName = subjectName,
                Tables = new List<TableConfig>(),
                Formula = new SubjectFormula()
            };

            config.Subjects.Add(subject);
            return subject;
        }

        private TableConfig EnsureTable(SubjectConfig subject, string tableName)
        {
            subject.Tables ??= new List<TableConfig>();
            var table = subject.Tables.FirstOrDefault(t => NameComparer.Equals(t.Name, tableName));
            if (table != null)
                return table;

            table = new TableConfig { Name = tableName };
            subject.Tables.Add(table);
            return table;
        }

        private static TableConfig? ResolveTable(SubjectConfig subject, string? tableName)
        {
            if (subject.Tables == null || subject.Tables.Count == 0)
                return null;

            if (string.IsNullOrWhiteSpace(tableName))
                return subject.Tables.First();

            return subject.Tables.FirstOrDefault(t => NameComparer.Equals(t.Name, tableName));
        }

        private static SheetConfig EnsureSheet(TableConfig table, string? sheetName)
        {
            table.Sheets ??= new List<SheetConfig>();

            if (!table.Sheets.Any())
            {
                var created = new SheetConfig
                {
                    Name = string.IsNullOrWhiteSpace(sheetName) ? "Sheet 1" : sheetName,
                    CategoriesRow = 1
                };

                table.Sheets.Add(created);
                return created;
            }

            if (string.IsNullOrWhiteSpace(sheetName))
                return table.Sheets[0];

            var existing = table.Sheets.FirstOrDefault(s => NameComparer.Equals(s.Name, sheetName));
            if (existing != null)
                return existing;

            var sheet = new SheetConfig { Name = sheetName };
            table.Sheets.Add(sheet);
            return sheet;
        }

        private static int NormalizeRow(int row) => row <= 0 ? 1 : row;

        private ParserConfig EditSubject(
            string subjectName,
            string? tableName,
            Action<ParserConfig, SubjectConfig, TableConfig> edit)
        {
            if (string.IsNullOrWhiteSpace(subjectName))
                throw new ArgumentException("Subject name is empty.", nameof(subjectName));
            if (edit is null) throw new ArgumentNullException(nameof(edit));

            return Edit(cfg =>
            {
                EnsureCollections(cfg);

                var subject = EnsureSubject(cfg, subjectName, allowExisting: true);
                var resolvedTableName = string.IsNullOrWhiteSpace(tableName)
                    ? subject.Tables.FirstOrDefault()?.Name ?? subjectName
                    : tableName.Trim();

                var table = EnsureTable(subject, resolvedTableName);

                edit(cfg, subject, table);
            });
        }


        public ParserConfig SetStudentName(string name) =>
            Edit(cfg => cfg.StudentName = name ?? string.Empty);

        public ParserConfig SetCache(string cacheDirectory, bool forceRefresh) =>
            Edit(cfg =>
            {
                cfg.CacheSettings.CacheDirectory = string.IsNullOrWhiteSpace(cacheDirectory) ? "./Cache" : cacheDirectory;
                cfg.CacheSettings.ForceRefresh = forceRefresh;
            });

        public ParserConfig CreateSubject(
            string subjectName,
            string tableName,
            string url,
            string sheetName,
            int headerRow,
            string finalFormula,
            Dictionary<string, string>? componentFormulas = null,
            Dictionary<string, string>? valueMappings = null,
            string? aggregateMethod = null) =>
            Edit(cfg =>
            {
                EnsureCollections(cfg);

                if (cfg.Subjects.Any(f => NameComparer.Equals(f.SubjectName, subjectName)))
                    throw new InvalidOperationException($"Предмет \"{subjectName}\" уже существует.");

                var subject = EnsureSubject(cfg, subjectName, allowExisting: false);
                var table = EnsureTable(subject, tableName);
                table.Url = url ?? string.Empty;
                table.Sheets = new List<SheetConfig>
                {
                    new SheetConfig
                    {
                        Name = string.IsNullOrWhiteSpace(sheetName) ? "Sheet 1" : sheetName,
                        CategoriesRow = NormalizeRow(headerRow)
                    }
                };

                subject.Formula = new SubjectFormula
                {
                    FinalFormula = finalFormula ?? string.Empty,
                    ComponentFormulas = componentFormulas != null ? new Dictionary<string, string>(componentFormulas) : new Dictionary<string, string>(),
                    ValueMappings = valueMappings != null ? new Dictionary<string, string>(valueMappings) : null,
                    AggregateMethod = aggregateMethod ?? string.Empty
                };
            });

        public ParserConfig SetSubjectUrl(string subjectName, string url, string? tableName = null) =>
            EditSubject(subjectName, tableName, (_, __, table) => table.Url = url ?? string.Empty);

        public ParserConfig SetSubjectFormula(
            string subjectName,
            string finalFormula,
            Dictionary<string, string>? componentFormulas = null,
            Dictionary<string, string>? valueMappings = null,
            string? aggregateMethod = null) =>
            EditSubject(subjectName, tableName: null, (_, subject, _) =>
            {
                subject.Formula ??= new SubjectFormula();

                subject.Formula.FinalFormula = finalFormula ?? string.Empty;

                if (componentFormulas != null)
                    subject.Formula.ComponentFormulas = new Dictionary<string, string>(componentFormulas);

                if (valueMappings != null)
                    subject.Formula.ValueMappings = new Dictionary<string, string>(valueMappings);

                if (aggregateMethod != null)
                    subject.Formula.AggregateMethod = aggregateMethod;
            });

        public ParserConfig SetSubjectHeaderRow(string subjectName, int headerRow, string? sheetName = null, string? tableName = null) =>
            EditSubject(subjectName, tableName, (_, __, table) =>
            {
                var sheet = EnsureSheet(table, sheetName);
                sheet.CategoriesRow = NormalizeRow(headerRow);
            });

        public ParserConfig SetSubjectSheet(string subjectName, string sheetName, string? tableName = null) =>
            EditSubject(subjectName, tableName, (_, __, table) =>
            {
                if (string.IsNullOrWhiteSpace(sheetName))
                    throw new ArgumentException("Sheet name is empty.", nameof(sheetName));

                var sheet = EnsureSheet(table, sheetName);
                sheet.Name = sheetName;
            });

        public ParserConfig RemoveSubject(string subjectName) =>
            Edit(cfg =>
            {
                EnsureCollections(cfg);

                cfg.Subjects.RemoveAll(f => NameComparer.Equals(f.SubjectName, subjectName));
            });
    }
}
