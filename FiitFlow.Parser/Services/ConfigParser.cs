using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using FiitFlow.Parser.Models;

namespace FiitFlow.Parser.Services;
public class ConfigParser
{
    public ParserConfig Parse(string[] lines)
    {
        var config = new ParserConfig();
        SubjectConfig currentSubject = null;
        TableConfig currentTable = null;
        SheetConfig currentSheet = null;

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            
            if (string.IsNullOrEmpty(trimmedLine))
            {
                if (currentSubject != null)
                {
                    if (currentTable != null && !currentSubject.Tables.Contains(currentTable))
                        currentSubject.Tables.Add(currentTable);

                    if (!config.Subjects.Any(s => s.SubjectName == currentSubject.SubjectName))
                        config.Subjects.Add(currentSubject);
                }

                currentTable = null;
                currentSubject = null;
                currentSheet = null;
                continue;
            }

            if (trimmedLine.StartsWith("Студент:"))
            {
                config.StudentName = trimmedLine.Substring("Студент:".Length).Trim();
            }
            else if (trimmedLine.StartsWith("CacheDirectory:", StringComparison.OrdinalIgnoreCase) ||
                     trimmedLine.StartsWith("ForceRefresh:", StringComparison.OrdinalIgnoreCase))
            {
                // Игнорируем устаревшие директивы кэша, чтобы не превращать их в названия предметов
                continue;
            }
            else if (!IsTableProperty(trimmedLine) && currentTable == null)
            {
                currentSubject = new SubjectConfig
                {
                    SubjectName = trimmedLine,
                    Tables = new List<TableConfig>()
                };

                currentTable = new TableConfig { Name = trimmedLine };
                currentSubject.Tables.Add(currentTable);
            }
            else if (trimmedLine.StartsWith("http") && currentTable != null && string.IsNullOrEmpty(currentTable.Url))
            {
                currentTable.Url = trimmedLine;
            }
            else if (trimmedLine.StartsWith("Sheet") && currentTable != null)
            {
                currentSheet = new SheetConfig { Name = trimmedLine.TrimEnd(':') };
                currentTable.Sheets.Add(currentSheet);
            }
            else if (trimmedLine.StartsWith("Строка с категориями:") && currentSheet != null)
            {
                currentSheet.CategoriesRow = ParseCategoriesRow(trimmedLine);
            }
        }

        if (currentSubject != null)
        {
            if (currentTable != null && !currentSubject.Tables.Contains(currentTable))
                currentSubject.Tables.Add(currentTable);

            if (!config.Subjects.Any(s => s.SubjectName == currentSubject.SubjectName))
                config.Subjects.Add(currentSubject);
        }

        return config;
    }

    private static bool IsTableProperty(string line) => 
        line.StartsWith("http") || line.StartsWith("Sheet") || 
        line.StartsWith("Строка с категориями:") ||
        line.StartsWith("CacheDirectory:", StringComparison.OrdinalIgnoreCase) ||
        line.StartsWith("ForceRefresh:", StringComparison.OrdinalIgnoreCase);

    private static int ParseCategoriesRow(string line)
    {
        var parts = line.Split(':');
        return parts.Length >= 2 && int.TryParse(parts[1].Trim(), out int row) ? row : 1;
    }
}
