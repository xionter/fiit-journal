namespace FiitFlow.Parser.Models
{
    public class StudentResults
    {
        public string Student { get; set; } = string.Empty;
        public Dictionary<string, SubjectResults> Subjects { get; set; } = new();
        public Dictionary<string, double> RatingScores { get; set; } = new(); // Итоговые баллы БРС
    }

    public class SubjectResults
    {
        public Dictionary<string, object> Overall { get; set; } = new(); // Общие результаты
        public Dictionary<string, Dictionary<string, object>> Categories { get; set; } = new(); // Категории
        public List<TableResult> Tables { get; set; } = new(); // Исходные таблицы
        public double CalculatedScore { get; set; } // Рассчитанный балл по формуле
    }
}
