using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using FiitFlow.Parser.Models;

namespace FiitFlow.Parser.Services;
public class StudentSearchService
{
    private readonly ExcelDownloader _excelDownloader;
    private readonly ExcelParser _excelParser;
    private readonly IOutputWriter _outputWriter;

    public StudentSearchService(
        ExcelDownloader excelDownloader,
        ExcelParser excelParser,
        IOutputWriter outputWriter)
    {
        _excelDownloader = excelDownloader;
        _excelParser = excelParser;
        _outputWriter = outputWriter;
    }

    public async Task SearchStudentInAllTablesAsync(ParserConfig config, string studentName)
    {
        if (!string.IsNullOrEmpty(config.StudentName))
        {
            _outputWriter.WriteTableHeader($"Студент: {config.StudentName}");
            _outputWriter.WriteEmptyLine();
        }

        foreach (var table in config.Tables)
        {
            await ProcessTableAsync(table, studentName);
        }
    }

    private async Task ProcessTableAsync(TableConfig table, string studentName)
    {
        _outputWriter.WriteTableHeader(table.Name);
        
        try
        {
            var filePath = await _excelDownloader.DownloadAsync(table);
            
            var students = _excelParser.FindStudents(filePath, studentName, table);
            var hasData = false;
            
            foreach (var student in students)
            {
                _outputWriter.WriteStudentData(table.Name, student.Data["SheetName"], student);
                _outputWriter.WriteEmptyLine();
                hasData = true;
            }
            
            if (!hasData)
            {
                _outputWriter.WriteEmptyLine();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при обработке таблицы {table.Name}: {ex.Message}");
            _outputWriter.WriteEmptyLine();
        }
    }
}
