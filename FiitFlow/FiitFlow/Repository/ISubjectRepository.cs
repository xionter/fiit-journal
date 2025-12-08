using FiitFlow.Domain;

namespace FiitFlow.Repository;

public interface ISubjectRepository
{
    Task<Subject?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<Subject>> GetByGroupAndSemesterAsync(Guid groupId, int semester);
    Task<Subject> GetOrCreateAsync(Guid groupId, string title, int semester, string? tableUrl = null);
    Task<IReadOnlyList<Subject>> GetByGroupAsync(Guid groupId, int semester);
}
