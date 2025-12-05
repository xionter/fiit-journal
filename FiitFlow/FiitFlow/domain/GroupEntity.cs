namespace FiitFlow.Domain;

public class GroupEntity
{
    public Guid Id { get; set; }              // внутренний id
    public string GroupTitle { get; set; } = null!; // фт-101 и т.п.
    public int Subgroup { get; set; }         // подгруппа, если есть

    public ICollection<Student> Students { get; set; } = new List<Student>();
}