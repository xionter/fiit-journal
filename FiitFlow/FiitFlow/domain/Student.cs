namespace FiitFlow.Domain;

public class Student
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;   // ФИО как в таблице
    public Guid GroupId { get; set; }
    public GroupEntity Group { get; set; } = null!;
    
}