using FiitFlow.Domain;
using Microsoft.EntityFrameworkCore;

namespace FiitFlow.Repository.Sqlite;

public class StudentRepository : IStudentRepository
{
    private readonly AppDbContext _db;
    private readonly IGroupRepository _groupRepository;

    public StudentRepository(AppDbContext db, IGroupRepository groupRepository)
    {
        _db = db;
        _groupRepository = groupRepository;
    }

    public async Task<Student?> GetByHashAsync(int id)
    {
        return await _db.Students
            .Include(s => s.Group)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Student?> GetByNameAndGroupAsync(string name, string surName, string groupTitle, int subGroup)
    {
        return await _db.Students
            .Include(s => s.Group)
            .FirstOrDefaultAsync(s =>
                    s.Group.GroupTitle == groupTitle && s.Group.Subgroup == subGroup && s.FullName == surName + name);
    }

    public async Task<IReadOnlyList<Student>> GetByGroupAsync(Guid groupId)
    {
        return await _db.Students
            .Where(s => s.GroupId == groupId)
            .OrderBy(s => s.FullName)
            .ToListAsync();
    }

    public async Task<Student> GetOrCreateAsync(string firstName, string lastName, string groupTitle, int subGroup)
    {
        var group = await _groupRepository.GetOrCreateByTitleAsync(groupTitle, subGroup);
        var fullName = lastName + " " + firstName;
        var existing = await _db.Students
            .FirstOrDefaultAsync(s => s.GroupId == group.Id && s.FullName == fullName);

        if (existing is not null)
            return existing;

        var student = new Student
        {
            Id = HashCode.Combine(fullName, groupTitle, subGroup),
            FullName = fullName,
            GroupId = group.Id
        };

        _db.Students.Add(student);
        await _db.SaveChangesAsync();

        return student;
    }

    public async Task<IReadOnlyList<Student>> GetByGroupIdAsync(Guid groupId)
    {
        return await _db.Students
            .Where(s => s.GroupId == groupId)
            .ToListAsync();
    }
}
