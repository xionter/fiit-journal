using FiitFlow.Domain;

namespace FiitFlow.Repository;

public interface IGroupRepository
{
    Task<GroupEntity?> GetByIdAsync(Guid id);
    Task<GroupEntity> GetOrCreateByTitleAsync(string groupTitle, int subgroup);
    Task<IReadOnlyList<GroupEntity>> GetAllAsync();
}