using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System;
using FiitFlow.Parser.Models;

namespace FiitFlow.Parser.Services;

public class ExcelParser
{
    public IEnumerable<Student> FindStudents(string filePath, string studentName, TableConfig tableConfig)
    {
        using var spreadsheetDocument = SpreadsheetDocument.Open(filePath, false);
        var workbookPart = spreadsheetDocument.WorkbookPart;

        if (workbookPart?.Workbook == null)
            yield break;

        var sheets = workbookPart.Workbook.Descendants<Sheet>().ToList();

        for (int i = 0; i < sheets.Count; i++)
        {
            var sheet = sheets[i];
            var sheetConfig = GetSheetConfig(tableConfig, sheet, i);

            if (sheetConfig == null) continue;

            var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id?.Value ?? "");
            if (worksheetPart?.Worksheet == null) continue;

            var sheetData = worksheetPart.Worksheet.Elements<SheetData>().FirstOrDefault();

            if (sheetData == null) continue;

            var rows = sheetData.Elements<Row>().ToList();
            var student = FindStudentInSheet(rows, studentName, sheetConfig, workbookPart);

            if (student != null)
            {
                student.Data["SheetName"] = sheet.Name ?? string.Empty;
                yield return student;
            }
        }
    }

    private static SheetConfig? GetSheetConfig(TableConfig tableConfig, Sheet sheet, int index)
    {
        return tableConfig.Sheets.FirstOrDefault(sc => 
                sc.Name.Equals($"Sheet {index + 1}", StringComparison.OrdinalIgnoreCase) || 
                sc.Name.Equals(sheet.Name ?? "", StringComparison.OrdinalIgnoreCase));
    }

    private static Student? FindStudentInSheet(List<Row> rows, string studentName, SheetConfig sheetConfig, WorkbookPart workbookPart)
    {
        for (int r = 0; r < rows.Count; r++)
        {
            var row = rows[r];

            foreach (var cell in row.Elements<Cell>())
            {
                var value = GetCellValue(cell, workbookPart);
                if (value.ToLower().Contains(studentName.ToLower()))
                {
                    return ExtractStudentData(rows, r + 1, sheetConfig.CategoriesRow, workbookPart);
                }
            }
        }
        return null;
    }

    private static Student ExtractStudentData(List<Row> rows, int studentRow, int categoriesRow, WorkbookPart workbookPart)
    {
        var student = new Student();
        var categoriesRowData = rows[categoriesRow - 1];
        var studentRowData = rows[studentRow - 1];

        foreach (var categoryCell in categoriesRowData.Elements<Cell>())
        {
            if (categoryCell.CellReference?.Value == null) continue;

            var categoryName = GetCellValue(categoryCell, workbookPart);
            if (string.IsNullOrWhiteSpace(categoryName)) continue;

            var column = GetColumnLetter(categoryCell.CellReference.Value);
            var studentCell = GetCellByReference(studentRowData, $"{column}{studentRow}");

            var cellValue = studentCell != null ? GetCellValue(studentCell, workbookPart) : "";

            var categoryDate = TryParseExcelDate(categoryName);
            if (categoryDate != null)
                categoryName = categoryDate;

            if (!string.IsNullOrEmpty(cellValue) && double.TryParse(cellValue, out _))
            {
                var dateValue = TryParseExcelDate(cellValue);
                if (dateValue != null)
                    cellValue = dateValue;
            }

            student.Data[categoryName] = cellValue;
        }

        return student;
    }

    private static string? TryParseExcelDate(string value)
    {
        if (string.IsNullOrEmpty(value)) 
            return null;

        var cleanValue = value.Trim().EndsWith(".0") ? value.Trim().Substring(0, value.Trim().Length - 2) : value.Trim();

        if (!double.TryParse(cleanValue, out double excelDate))
            return null;

        try
        {
            DateTime baseDate = new DateTime(1899, 12, 30);
            DateTime date = baseDate.AddDays(excelDate);

            if (date.Year < 2023 || date.Year > 2026)
                return null;

            return date.ToString("dd.MM.yyyy");
        }
        catch
        {
            return value;
        }
    }

    private static Cell? GetCellByReference(Row row, string cellReference)
    {
        return row.Elements<Cell>().FirstOrDefault(c => c.CellReference?.Value == cellReference);
    }

    private static string GetColumnLetter(string cellReference)
    {
        return Regex.Replace(cellReference, @"[\d-]", "");
    }

    private static string GetCellValue(Cell? cell, WorkbookPart workbookPart)
    {
        if (cell?.CellValue == null) return string.Empty;

        var value = cell.CellValue.Text;

        if (cell.DataType?.Value == CellValues.SharedString)
        {
            var stringTable = workbookPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();
            if (stringTable != null && int.TryParse(value, out int index) && 
                    index >= 0 && index < stringTable.SharedStringTable.Count())
            {
                value = stringTable.SharedStringTable.ElementAt(index).InnerText;
            }
        }

        return value.Trim();
    }
}
