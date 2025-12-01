namespace FiitFlow.Domain;

public class Student
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;   // ФИО как в таблице
    public Guid GroupId { get; set; }
    public GroupEntity Group { get; set; } = null!;

    public ICollection<Points> Points { get; set; } = new List<Points>();
}