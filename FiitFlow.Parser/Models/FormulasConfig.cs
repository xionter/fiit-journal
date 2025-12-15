namespace FiitFlow.Parser.Models
{
    public class SubjectFormula
    {
        public string? FinalFormula { get; set; }
        public Dictionary<string, string>? ComponentFormulas { get; set; }
        public Dictionary<string, string>? ValueMappings { get; set; }
        public string AggregateMethod { get; set; } = string.Empty;
    }

    public class SubjectConfig
    {
        public string SubjectName { get; set; } = string.Empty;
        public List<TableConfig> Tables { get; set; } = new();
        public SubjectFormula Formula { get; set; } = new SubjectFormula();
    }
}
