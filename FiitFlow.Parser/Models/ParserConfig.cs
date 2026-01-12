using System.Collections.Generic;

namespace FiitFlow.Parser.Models
{
    public class SheetConfig
    {
        public string Name { get; set; } = string.Empty;
        public int? CategoriesRow { get; set; }
    }

    public class TableConfig
    {
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public List<SheetConfig> Sheets { get; set; } = new();
    }

    public class ParserConfig
    {
        public string StudentName { get; set; } = string.Empty;
        public List<SubjectConfig> Subjects { get; set; } = new();
    }
}
