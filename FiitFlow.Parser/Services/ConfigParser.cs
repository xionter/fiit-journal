using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class ConfigParser
{
    public AppConfig Parse(string[] lines)
    {
        var config = new AppConfig();
        TableConfig currentTable = null;
        SheetConfig currentSheet = null;

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            
            if (string.IsNullOrEmpty(trimmedLine))
            {
                currentTable = null;
                currentSheet = null;
                continue;
            }

            if (trimmedLine.StartsWith("Студент:"))
            {
                config.StudentName = trimmedLine.Substring("Студент:".Length).Trim();
            }
            else if (!IsTableProperty(trimmedLine) && currentTable == null)
            {
                currentTable = new TableConfig { Name = trimmedLine };
                config.Tables.Add(currentTable);
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

        return config;
    }

    private static bool IsTableProperty(string line) => 
        line.StartsWith("http") || line.StartsWith("Sheet") || 
        line.StartsWith("Строка с категориями:");

    private static int ParseCategoriesRow(string line)
    {
        var parts = line.Split(':');
        return parts.Length >= 2 && int.TryParse(parts[1].Trim(), out int row) ? row : 1;
    }
}
