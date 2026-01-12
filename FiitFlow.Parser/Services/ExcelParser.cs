using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Globalization;
using System;
using FiitFlow.Parser.Models;
using FiitFlow.Parser.Interfaces;

namespace FiitFlow.Parser.Services;

public class ExcelParser : IExcelParser
{
    public IEnumerable<Student> FindStudents(string filePath, string? studentName, TableConfig tableConfig)
    {
        using var spreadsheetDocument = SpreadsheetDocument.Open(filePath, false);
        var workbookPart = spreadsheetDocument.WorkbookPart;

        if (workbookPart?.Workbook == null)
            yield break;

        var sheets = workbookPart.Workbook.Descendants<Sheet>().ToList();

        for (var i = 0; i < sheets.Count; ++i)
        {
            var sheet = sheets[i];
            var sheetConfig = GetSheetConfig(tableConfig, sheet, i);

            if (sheetConfig == null) continue;

            var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id?.Value ?? "");
            if (worksheetPart?.Worksheet == null) continue;

            var sheetData = worksheetPart.Worksheet.Elements<SheetData>().FirstOrDefault();

            if (sheetData == null) continue;

            var rows = sheetData.Elements<Row>().ToList();

            int? categoriesRow = sheetConfig.CategoriesRow;
            if (!categoriesRow.HasValue || categoriesRow.Value <= 0)
            {
                categoriesRow = AutoDetectCategoriesRow(rows, workbookPart);
            }

            if (!categoriesRow.HasValue)
                throw new Exception($"Не удалось найти строку категорий в листе {sheet.Name}");

            var students = FindStudentInSheet(
                    rows,
                    studentName,
                    categoriesRow.Value,
                    workbookPart
                    );

            foreach (var student in students)
            {
                student.Data["SheetName"] = sheet.Name ?? string.Empty;
                yield return student;
            }
        }
    }

    private static SheetConfig? GetSheetConfig(TableConfig tableConfig, Sheet sheet, int index)
    {
        if (tableConfig.Sheets == null || tableConfig.Sheets.Count == 0)
        {
            return new SheetConfig
            {
                Name = sheet.Name ?? $"Sheet {index + 1}"
            };
        }

        return tableConfig.Sheets.FirstOrDefault(sc => 
                sc.Name.Equals($"Sheet {index + 1}", StringComparison.OrdinalIgnoreCase) || 
                sc.Name.Equals(sheet.Name ?? "", StringComparison.OrdinalIgnoreCase));
    }

    private static IEnumerable<Student> FindStudentInSheet(
            List<Row> rows,
            string? studentName,
            int categoriesRow,
            WorkbookPart workbookPart)

    {
        if (string.IsNullOrWhiteSpace(studentName))
        {
            for (var r = categoriesRow; r < rows.Count; ++r)
            {
                var name = GetFirstNonEmptyCellValue(rows[r], workbookPart);
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                var student = ExtractStudentData(rows, r + 1, categoriesRow, workbookPart, name);
                yield return student;
            }
        }
        else
        {
            for (var r = 0; r < rows.Count; ++r)
            {
                var row = rows[r];

                foreach (var cell in row.Elements<Cell>())
                {
                    var value = GetCellValue(cell, workbookPart);
                    if (value.ToLower().Contains(studentName.ToLower()))
                    {
                        var student = ExtractStudentData(rows, r + 1, categoriesRow, workbookPart, studentName);
                        yield return student;
                        yield break;
                    }
                }
            }
        }
    }

    private static Student ExtractStudentData(List<Row> rows, int studentRow, int categoriesRow, WorkbookPart workbookPart, string? fullName = null)
    {
        var student = new Student { FullName = fullName ?? string.Empty };
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

            if (!string.IsNullOrEmpty(cellValue) && TryParseDoubleInvariant(cellValue, out _))
            {
                var dateValue = TryParseExcelDate(cellValue);
                if (dateValue != null)
                    cellValue = dateValue;
            }

            student.Data[categoryName] = cellValue;
        }

        if (string.IsNullOrWhiteSpace(student.FullName))
        {
            student.FullName = GetFirstNonEmptyCellValue(studentRowData, workbookPart);
        }

        return student;
    }

    private static string GetFirstNonEmptyCellValue(Row row, WorkbookPart workbookPart)
    {
        foreach (var cell in row.Elements<Cell>())
        {
            var value = GetCellValue(cell, workbookPart);
            if (!string.IsNullOrWhiteSpace(value))
                return value.Trim();
        }
        return string.Empty;
    }

    private static string? TryParseExcelDate(string value)
    {
        if (string.IsNullOrEmpty(value)) 
            return null;

        var cleanValue = value.Trim().EndsWith(".0") ? value.Trim().Substring(0, value.Trim().Length - 2) : value.Trim();

        if (!TryParseDoubleInvariant(cleanValue, out double excelDate))
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

    private static bool TryParseDoubleInvariant(string value, out double number)
    {
        number = 0;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var normalized = value.Trim()
            .Replace("\u00A0", string.Empty)
            .Replace(" ", string.Empty)
            .Replace(",", ".");

        return double.TryParse(
                normalized,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out number);
    }

    private static int? AutoDetectCategoriesRow(
            List<Row> rows,
            WorkbookPart workbookPart)
    {
        for (int i = 0; i < Math.Min(rows.Count, 15); i++)
        {
            var values = rows[i]
                .Elements<Cell>()
                .Select(c => GetCellValue(c, workbookPart))
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .ToList();

            if (values.Count < 3)
                continue;

            if (values.Count(IsCategoryLike) >= 2)
                return i + 1;
        }

        for (int i = 0; i < Math.Min(rows.Count, 15); i++)
        {
            var values = rows[i]
                .Elements<Cell>()
                .Select(c => GetCellValue(c, workbookPart))
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .ToList();

            if (values.Count < 3)
                continue;

            if (!TryParseDoubleInvariant(values[0], out _))
                return i + 1;
        }

        return null;
    }

    private static bool IsCategoryLike(string value)
    {
        value = value.ToLower();

        return value.Contains("дз")
            || value.Contains("кр")
            || value.Contains("пров")
            || value.Contains("акт")
            || value.Contains("итог")
            || value.Contains("сумм")
            || value.Contains("total")
            || value.Contains("балл");
    }

}
