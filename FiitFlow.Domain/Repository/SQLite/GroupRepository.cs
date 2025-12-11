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

    public async Task<GroupEntity?> GetByTitleAsync(string groupTitle, int subgroup)
    {
        return await _db.Groups
            .Include(g => g.Students)
            .FirstOrDefaultAsync(g =>
                g.GroupTitle == groupTitle && g.Subgroup == subgroup);
    }

    public async Task<IReadOnlyList<GroupEntity>> GetAllAsync()
    {
        return await _db.Groups
            .OrderBy(g => g.GroupTitle)
            .ThenBy(g => g.Subgroup)
            .ToListAsync();
    }
}