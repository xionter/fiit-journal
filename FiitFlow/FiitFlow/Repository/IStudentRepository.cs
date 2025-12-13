using FiitFlow.Domain;

namespace FiitFlow.Repository;

public interface IStudentRepository
{
    Task<Student?> GetByHashAsync(int id);
    Task<Student?> GetByNameAndGroupAsync(string name, string surName, string groupTitle, int subGroup);
    Task<IReadOnlyList<Student>> GetByGroupAsync(Guid groupId);
    Task<Student> GetOrCreateAsync(string fullName, string groupTitle, int subGroup, Guid groupId);
    Task<IReadOnlyList<Student>> GetByGroupIdAsync(Guid groupId);
}
