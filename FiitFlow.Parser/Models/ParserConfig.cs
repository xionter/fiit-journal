using System.Collections.Generic;

namespace FiitFlow.Parser.Models;

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

public class CacheSettings
{
    public string CacheDirectory { get; set; } = "./Cache";
    public bool ForceRefresh { get; set; } = false;
}

public class FormulaConfig
{
    public string Name { get; set; } = string.Empty;
    public string Expression { get; set; } = string.Empty;
    public Dictionary<string, string> Variables { get; set; } = new();
    public Dictionary<string, string> FormulaReferences { get; set; } = new();
}

public class SubjectConfig
{
    public string Name { get; set; } = string.Empty;
    public List<string> TableNames { get; set; } = new();
    public List<FormulaConfig> Formulas { get; set; } = new();
}

public class ParserConfig
{
    public string StudentName { get; set; } = string.Empty;
    public CacheSettings CacheSettings { get; set; } = new CacheSettings();
    public List<TableConfig> Tables { get; set; } = new();
    public List<SubjectConfig> Subjects { get; set; } = new();
}
