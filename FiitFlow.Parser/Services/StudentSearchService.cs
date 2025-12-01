using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
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

        var allTableData = new Dictionary<string, Dictionary<string, string>>();

        foreach (var table in config.Tables)
        {
            var tableData = await ProcessTableAsync(table, studentName);
            if (tableData != null && tableData.Any())
            {
                allTableData[table.Name] = tableData;
            }
        }

        if (config.Subjects != null && config.Subjects.Any())
        {
            await CalculateAndDisplayFormulasAsync(config, allTableData);
        }
    }

    private async Task<Dictionary<string, string>?> ProcessTableAsync(
            TableConfig table, 
            string studentName)
    {
        _outputWriter.WriteTableHeader(table.Name);

        try
        {
            var filePath = await _excelDownloader.DownloadAsync(table);

            var students = _excelParser.FindStudents(filePath, studentName, table);
            var hasData = false;
            var tableData = new Dictionary<string, string>();

            foreach (var student in students)
            {
                _outputWriter.WriteStudentData(table.Name, student.Data["SheetName"], student);
                _outputWriter.WriteEmptyLine();
                hasData = true;

                foreach (var kvp in student.Data)
                {
                    tableData[kvp.Key] = kvp.Value;
                }
            }

            if (!hasData)
            {
                _outputWriter.WriteEmptyLine();
            }

            return tableData;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при обработке таблицы {table.Name}: {ex.Message}");
            _outputWriter.WriteEmptyLine();
            return null;
        }
    }

    private async Task CalculateAndDisplayFormulasAsync(
            ParserConfig config, 
            Dictionary<string, Dictionary<string, string>> allTableData)
    {
        _outputWriter.WriteTableHeader("РАСЧЕТЫ ПО ФОРМУЛАМ");
        _outputWriter.WriteEmptyLine();

        var formulaCalculator = new FormulaCalculator();

        foreach (var subject in config.Subjects)
        {
            _outputWriter.WriteTableHeader($"Предмет: {subject.Name}");

            var subjectData = new Dictionary<string, Dictionary<string, string>>();
            foreach (var tableName in subject.TableNames)
            {
                if (allTableData.ContainsKey(tableName))
                {
                    subjectData[tableName] = allTableData[tableName];
                }
                else
                {
                    _outputWriter.WriteLine($"   Таблица '{tableName}' не найдена для предмета '{subject.Name}'");
                }
            }

            try
            {
                var results = formulaCalculator.CalculateFormulasForSubject(subject, subjectData, allTableData);

                foreach (var result in results)
                {
                    if (double.IsNaN(result.Value))
                    {
                        _outputWriter.WriteLine($"   {result.Key}: ОШИБКА ВЫЧИСЛЕНИЯ");
                    }
                    else
                    {
                        _outputWriter.WriteLine($"   {result.Key}: {result.Value:F2}");
                    }
                }
            }
            catch (Exception ex)
            {
                _outputWriter.WriteLine($"   ОШИБКА: {ex.Message}");
            }

            _outputWriter.WriteEmptyLine();
        }
    }
}
