namespace FiitFlow.Domain;


public class Points
{
    public Guid Id { get; set; }

    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public Guid SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;

    public int Semester { get; set; }

    public int Value { get; set; }          // текущие баллы
    public DateTime UpdatedAt { get; set; } // когда последний раз парсили/обновляли
}
