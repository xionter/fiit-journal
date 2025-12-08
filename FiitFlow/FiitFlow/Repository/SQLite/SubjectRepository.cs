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
        return await _db.Subjects
            .Where(s => s.Semester == semester && s.GroupId == groupId)
            .OrderBy(s => s.Title)
            .ToListAsync();
    }

    public async Task<Subject> GetOrCreateAsync(Guid groupId, string title, int semester, string? tableUrl = null)
    {
        var existing = await _db.Subjects
            .FirstOrDefaultAsync(s =>
                    s.GroupId == groupId &&
                    s.Semester == semester &&
                    s.Title == title);

        if (existing is not null)
            return existing;

        var subject = new Subject
        {
            Id = Guid.NewGuid(),
            GroupId = groupId,
            Title = title,
            Semester = semester,
            TableUrl = tableUrl
        };

        _db.Subjects.Add(subject);
        await _db.SaveChangesAsync();

        return subject;
    }

    public async Task<IReadOnlyList<Subject>> GetByGroupAsync(Guid groupId, int semester)
    {
        return await _db.Subjects
            .Where(s => s.GroupId == groupId && s.Semester == semester)
            .ToListAsync();
    }
}
