namespace FiitFlow.Domain;

public class Subject
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public string Title { get; set; } = null!;   // название предмета
    public int Semester { get; set; }            // номер семестра
    public string? TableUrl { get; set; }        // ссылка на исходную таблицу с баллами

    public ICollection<Points> Points { get; set; } = new List<Points>();
}