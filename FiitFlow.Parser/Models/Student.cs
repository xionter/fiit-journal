using System.Collections.Generic;

public class Student
{
    public string FullName { get; set; } = string.Empty;
    public Dictionary<string, string> Data { get; set; } = new();
}

public class SheetConfig
{
    public string Name { get; set; } = string.Empty;
    public int CategoriesRow { get; set; } = 1;
}

public class TableConfig
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public List<SheetConfig> Sheets { get; set; } = new();
}

public class AppConfig
{
    public string StudentName { get; set; } = string.Empty;
    public List<TableConfig> Tables { get; set; } = new();
}
