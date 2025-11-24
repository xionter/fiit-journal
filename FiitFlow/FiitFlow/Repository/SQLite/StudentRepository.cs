using FiitFlow.Domain;
using Microsoft.EntityFrameworkCore;

namespace FiitFlow.Repository.Sqlite;

public class StudentRepository : IStudentRepository
{
    private readonly AppDbContext _db;

    public StudentRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Student?> GetByIdAsync(Guid id)
    {
        return await _db.Students
            .Include(s => s.Group)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Student?> GetByNameAsync(string fullName, Guid groupId)
    {
        return await _db.Students
            .Include(s => s.Group)
            .FirstOrDefaultAsync(s =>
                s.GroupId == groupId && s.FullName == fullName);
    }

    public async Task<IReadOnlyList<Student>> GetByGroupAsync(Guid groupId)
    {
        return await _db.Students
            .Where(s => s.GroupId == groupId)
            .OrderBy(s => s.FullName)
            .ToListAsync();
    }
}