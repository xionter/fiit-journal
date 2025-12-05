namespace FiitFlow.Parser.Models;

public record TableResult(string StudentName, string TableName, string TableUrl, string SheetName, Dictionary<string, string> Data);

public class StudentSearchResult
{
    // If a specific student was requested, this is set; otherwise empty.
    public string StudentName { get; init; } = string.Empty;

    // Flat list of all tables parsed with the associated student name.
    public List<TableResult> Tables { get; } = new();
}
