using FiitFlow.Domain;

namespace FiitFlow.Repository;

public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(Guid id);
    Task<Student?> GetByNameAsync(string fullName, Guid groupId);
    Task<IReadOnlyList<Student>> GetByGroupAsync(Guid groupId);
    Task<Student> GetOrCreateAsync(string fullName, Guid groupId);
}
