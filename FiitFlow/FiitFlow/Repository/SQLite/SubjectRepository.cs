using FiitFlow.Domain;
using Microsoft.EntityFrameworkCore;

namespace FiitFlow.Repository.Sqlite;

public class SubjectRepository : ISubjectRepository
{
    private readonly AppDbContext _db;

    public SubjectRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Subject?> GetByIdAsync(Guid id)
    {
        return await _db.Subjects
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IReadOnlyList<Subject>> GetByGroupAndSemesterAsync(Guid groupId, int semester)
    {
        // пока без groupId
        return await _db.Subjects
            .Where(s => s.Semester == semester)
            .OrderBy(s => s.Title)
            .ToListAsync();
    }
}