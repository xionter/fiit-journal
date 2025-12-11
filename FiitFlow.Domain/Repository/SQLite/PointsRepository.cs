using FiitFlow.Domain;
using Microsoft.EntityFrameworkCore;

namespace FiitFlow.Repository.Sqlite;

public class PointsRepository : IPointsRepository
{
    private readonly AppDbContext _db;

    public PointsRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Points>> GetByStudentAsync(Guid studentId)
    {
        return await _db.Points
            .Include(p => p.Subject)
            .Where(p => p.StudentId == studentId)
            .OrderBy(p => p.Semester)
            .ThenBy(p => p.Subject.Title)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Points>> GetByGroupAsync(Guid groupId, int? semester = null)
    {
        var query = _db.Points
            .Include(p => p.Student)
            .Include(p => p.Subject)
            .Where(p => p.Student.GroupId == groupId);

        if (semester.HasValue)
            query = query.Where(p => p.Semester == semester.Value);

        return await query
            .OrderBy(p => p.Semester)
            .ThenBy(p => p.Subject.Title)
            .ThenBy(p => p.Student.FullName)
            .ToListAsync();
    }

    public async Task UpsertRangeAsync(IEnumerable<Points> points)
    {
        foreach (var p in points)
        {
            var existing = await _db.Points
                .FirstOrDefaultAsync(x =>
                    x.StudentId == p.StudentId &&
                    x.SubjectId == p.SubjectId &&
                    x.Semester == p.Semester);

            if (existing is null)
            {
                _db.Points.Add(p);
            }
            else
            {
                existing.Value = p.Value;
                existing.UpdatedAt = p.UpdatedAt;
            }
        }

        await _db.SaveChangesAsync();
    }
}