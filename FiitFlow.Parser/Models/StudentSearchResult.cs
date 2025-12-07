namespace FiitFlow.Parser.Models;

public record TableResult(string StudentName, string TableName, 
        string TableUrl, string SheetName, Dictionary<string, string> Data);

public class StudentSearchResult
{
    public string StudentName { get; init; } = string.Empty;

    public List<TableResult> Tables { get; } = new();
}
