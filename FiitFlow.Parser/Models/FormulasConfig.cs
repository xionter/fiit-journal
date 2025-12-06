namespace FiitFlow.Parser.Models
{
    public class SubjectFormula
    {
        public string SubjectName { get; set; } = string.Empty;
        public List<string> TableNames { get; set; } = new();
        public string? FinalFormula { get; set; } // Формула итога
        public Dictionary<string, string>? ComponentFormulas { get; set; } // Формулы компонентов
        public Dictionary<string, string>? ValueMappings { get; set; } // Маппинг значений из таблиц
    }

    public class FormulasConfig
    {
        public List<SubjectFormula> SubjectFormulas { get; set; } = new();
    }
}
