using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using FiitFlow.Parser.Models;

namespace FiitFlow.Parser.Services;
public class StudentSearchService
{
    private readonly ExcelDownloader _excelDownloader;
    private readonly ExcelParser _excelParser;

    public StudentSearchService(
        ExcelDownloader excelDownloader,
        ExcelParser excelParser)
    {
        _excelDownloader = excelDownloader;
        _excelParser = excelParser;
    }

    public async Task<StudentSearchResult> SearchStudentInAllTablesAsync(ParserConfig config, string studentName)
    {
        var result = new StudentSearchResult { StudentName = studentName };

        foreach (var table in config.Tables)
        {
            var tableResults = await ProcessTableAsync(table, studentName);
            result.Tables.AddRange(tableResults);
        }

        return result;
    }

    private async Task<IReadOnlyList<TableResult>> ProcessTableAsync(TableConfig table, string studentName)
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
}
