using FiitFlow.Domain;
using Microsoft.EntityFrameworkCore;

namespace FiitFlow.Repository.Sqlite;

public class GroupRepository : IGroupRepository
{
    private readonly AppDbContext _db;

    public GroupRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<GroupEntity?> GetByIdAsync(Guid id)
    {
        return await _db.Groups
            .Include(g => g.Students)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<GroupEntity> GetOrCreateByTitleAsync(string groupTitle, int subgroup)
    {
        var existing = await _db.Groups
            .Include(g => g.Students)
            .FirstOrDefaultAsync(g =>
                g.GroupTitle == groupTitle && g.Subgroup == subgroup);

        if (existing is not null)
            return existing;

        var group = new GroupEntity
        {
            Id = Guid.NewGuid(),
            GroupTitle = groupTitle,
            Subgroup = subgroup
        };

        _db.Groups.Add(group);
        await _db.SaveChangesAsync();

        return group;
    }

    public async Task<IReadOnlyList<GroupEntity>> GetAllAsync()
    {
        return await _db.Groups
            .OrderBy(g => g.GroupTitle)
            .ThenBy(g => g.Subgroup)
            .ToListAsync();
    }
}