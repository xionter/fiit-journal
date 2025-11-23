using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

public class StudentSearchService
{
    private readonly ExcelDownloader _excelDownloader;
    private readonly ExcelParser _excelParser;
    private readonly FileOutputWriter _outputWriter;

    public StudentSearchService(
        ExcelDownloader excelDownloader,
        ExcelParser excelParser,
        FileOutputWriter outputWriter)
    {
        _excelDownloader = excelDownloader;
        _excelParser = excelParser;
        _outputWriter = outputWriter;
    }

    public async Task SearchStudentInAllTablesAsync(AppConfig config)
    {
        if (!string.IsNullOrEmpty(config.StudentName))
        {
            _outputWriter.WriteTableHeader($"Студент: {config.StudentName}");
            _outputWriter.WriteEmptyLine();
        }

        foreach (var table in config.Tables)
        {
            await ProcessTableAsync(table, config.StudentName);
        }
    }

    private async Task ProcessTableAsync(TableConfig table, string studentName)
    {
        _outputWriter.WriteTableHeader(table.Name);
        
        try
        {
            var tempFile = Path.GetTempFileName();
            var downloadUrl = BuildDownloadUrl(table.Url);
            
            await _excelDownloader.DownloadAsync(downloadUrl, tempFile);
            
            var students = _excelParser.FindStudents(tempFile, studentName, table);
            var hasData = false;
            
            foreach (var student in students)
            {
                _outputWriter.WriteStudentData(table.Name, student.Data["SheetName"], student);
                _outputWriter.WriteEmptyLine();
                hasData = true;
            }
            
            if (!hasData)
            {
                _outputWriter.WriteEmptyLine(); // Keep consistent spacing even for empty tables
            }
            
            File.Delete(tempFile);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при обработке таблицы {table.Name}: {ex.Message}");
            _outputWriter.WriteEmptyLine();
        }
    }

    private static string BuildDownloadUrl(string sheetUrl)
    {
        var fileId = ExtractFileId(sheetUrl);
        return $"https://docs.google.com/spreadsheets/d/{fileId}/export?format=xlsx";
    }

    private static string ExtractFileId(string url)
    {
        var uri = new Uri(url);
        var path = uri.AbsolutePath;
        var startIndex = path.IndexOf("/d/") + 3;
        
        if (startIndex < 3) throw new ArgumentException("Invalid Google Sheets URL");
        
        var endIndex = path.IndexOf("/", startIndex);
        if (endIndex == -1) endIndex = path.Length;
        
        return path.Substring(startIndex, endIndex - startIndex);
    }
}
