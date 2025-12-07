namespace FiitFlow.Parser.Models
{
    public class StudentResults
    {
        public string Student { get; set; } = string.Empty;
        public Dictionary<string, SubjectResults> Subjects { get; set; } = new();
        public Dictionary<string, double> RatingScores { get; set; } = new();
    }

    public class SubjectResults
    {
        public Dictionary<string, object> Overall { get; set; } = new();
        public Dictionary<string, Dictionary<string, object>> Categories { get; set; } = new();
        public List<TableResult> Tables { get; set; } = new();
        public double CalculatedScore { get; set; }
    }
}
