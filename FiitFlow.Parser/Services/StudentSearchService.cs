using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using FiitFlow.Parser.Models;
using FiitFlow.Parser.Interfaces;

namespace FiitFlow.Parser.Services;
public class StudentSearchService : IStudentSearchService
{
    private readonly IExcelDownloader _excelDownloader;
    private readonly IExcelParser _excelParser;

    public StudentSearchService(
        IExcelDownloader excelDownloader,
        IExcelParser excelParser)
    {
        _excelDownloader = excelDownloader;
        _excelParser = excelParser;
    }

    public async Task<StudentSearchResult> SearchStudentInAllTablesAsync(ParserConfig config, string? studentName = null)
    {
        var result = new StudentSearchResult { StudentName = studentName ?? string.Empty };

        var tables = config.Subjects?
            .SelectMany(s => s.Tables ?? Enumerable.Empty<TableConfig>())
            .Where(t => !string.IsNullOrWhiteSpace(t.Name) || !string.IsNullOrWhiteSpace(t.Url))
            .GroupBy(t => GetTableDedupKey(t), StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList() ?? new List<TableConfig>();

        foreach (var table in tables)
        {
            var tableResults = await ProcessTableAsync(table, studentName);
            result.Tables.AddRange(tableResults);
        }

        return result;
    }

    public async Task<StudentSearchResult> SearchStudentInTableAsync(ParserConfig config, string subjectName)
    {
        var result = new StudentSearchResult { StudentName = config.StudentName };
        var tables = config.Subjects?.Where(s => s.SubjectName == subjectName)
            .SelectMany(s => s.Tables).ToList() ?? new List<TableConfig>(); ;

        foreach (var table in tables)
        {
            var tableResults = await ProcessTableAsync(table, config.StudentName);
            result.Tables.AddRange(tableResults);
        }

        return result;
    }

    private async Task<IReadOnlyList<TableResult>> ProcessTableAsync(TableConfig table, string? studentName)
    {
        var tableResults = new List<TableResult>();
        
        try
        {
            var filePath = await _excelDownloader.DownloadAsync(table);
            
            var students = _excelParser.FindStudents(filePath, studentName, table);
            
            foreach (var student in students)
            {
                student.Data.TryGetValue("SheetName", out var sheetName);

                var data = new Dictionary<string, string>(student.Data);
                data.Remove("SheetName");

                tableResults.Add(new TableResult(
                    student.FullName,
                    table.Name,
                    table.Url,
                    sheetName ?? string.Empty,
                    data));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при обработке таблицы {table.Name}: {ex.Message}");
        }

        return tableResults;
    }

    private static string GetTableDedupKey(TableConfig table)
    {
        var url = table.Url?.Trim() ?? string.Empty;
        var name = table.Name?.Trim() ?? string.Empty;
        return $"{url}|{name}";
    }
}
