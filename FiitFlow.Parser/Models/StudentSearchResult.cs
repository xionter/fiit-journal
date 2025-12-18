namespace FiitFlow.Parser.Models;

//public record TableResult(string StudentName, string TableName,
//        string TableUrl, string SheetName, Dictionary<string, string> Data);

public class TableResult
{
    public string StudentName { get; set; }
    public string TableName { get; set; }
    public string TableUrl { get; set; }
    public string SheetName { get; set; }
    public Dictionary<string, string> Data { get; set; }

    public TableResult(string studentName, string tableName,
        string tableUrl, string sheetName, Dictionary<string, string> data)
    {
        StudentName = studentName;
        TableName = tableName;
        TableUrl = tableUrl;
        SheetName = sheetName;
        Data = data;
    }
}

public class StudentSearchResult
{
    public string StudentName { get; set; } = string.Empty;

    public List<TableResult> Tables { get; set; } = new();
}
