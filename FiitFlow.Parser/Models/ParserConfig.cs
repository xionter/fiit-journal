using System.Collections.Generic;

namespace FiitFlow.Parser.Models
{
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

    public class ParserConfig
    {
        public string StudentName { get; set; } = string.Empty;
        public CacheSettings CacheSettings { get; set; } = new CacheSettings();
        public List<SubjectConfig> Subjects { get; set; } = new();
    }
}
