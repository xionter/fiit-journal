namespace FiitFlow.Parser.Models
{
    public class SubjectFormula
    {
        public string SubjectName { get; set; } = string.Empty;
        public List<string> TableNames { get; set; } = new();
        public string? FinalFormula { get; set; }
        public Dictionary<string, string>? ComponentFormulas { get; set; }
        public Dictionary<string, string>? ValueMappings { get; set; }
        public string AggregateMethod { get; set; } = string.Empty;
    }

    public class FormulasConfig
    {
        public List<SubjectFormula> SubjectFormulas { get; set; } = new();
    }
}
