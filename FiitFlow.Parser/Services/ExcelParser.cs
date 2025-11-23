using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

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

            var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id!.Value!);
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

    private static SheetConfig GetSheetConfig(TableConfig tableConfig, Sheet sheet, int index) =>
        tableConfig.Sheets.FirstOrDefault(sc => 
            sc.Name.Equals($"Sheet {index + 1}", StringComparison.OrdinalIgnoreCase) || 
            sc.Name.Equals(sheet.Name ?? "", StringComparison.OrdinalIgnoreCase));

    private static Student FindStudentInSheet(List<Row> rows, string studentName, SheetConfig sheetConfig, WorkbookPart workbookPart)
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
            
            student.Data[categoryName] = studentCell != null ? GetCellValue(studentCell, workbookPart) : "";
        }

        return student;
    }

    private static Cell GetCellByReference(Row row, string cellReference) =>
        row.Elements<Cell>().FirstOrDefault(c => c.CellReference?.Value == cellReference);

    private static string GetColumnLetter(string cellReference) =>
        Regex.Replace(cellReference, @"[\d-]", "");

    private static string GetCellValue(Cell cell, WorkbookPart workbookPart)
    {
        if (cell.CellValue == null) return string.Empty;

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
