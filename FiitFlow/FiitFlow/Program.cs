using FiitFlow;
using FiitFlow.Domain;
using FiitFlow.Repository;
using FiitFlow.Repository.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FiitFlow;

public class Program
{
    public static async Task Main(string[] args)
    {

        
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=fiitflow.db")
            .Options;

        using var db = new AppDbContext(options);
        await db.Database.EnsureCreatedAsync();
        
        IGroupRepository groupRepo = new GroupRepository(db);
        IStudentRepository studentRepo = new StudentRepository(db);
        ISubjectRepository subjectRepo = new SubjectRepository(db);
        IPointsRepository pointsRepo = new PointsRepository(db);


        var pointsService = new PointsService(pointsRepo, studentRepo, subjectRepo);
        
        
        // тест
        if (!await db.Groups.AnyAsync())
        {
            var group = new GroupEntity
            {
                Id = Guid.NewGuid(),
                GroupTitle = "FIIT-101",
                Subgroup = 1
            };

            var student = new Student
            {
                Id = Guid.NewGuid(),
                FullName = "Иванов Иван Иванович",
                Group = group
            };
            
            var subject = new Subject
            {
                Id = Guid.NewGuid(),
                GroupId = group.Id,
                Title = "Математика",
                TableUrl = "hihiha"
            };
            
            var points = new Points
            {
                Id = Guid.NewGuid(),
                Student = student,
                Subject = subject,
                Value = 85,
                UpdatedAt = DateTime.UtcNow
            };
            
            db.Groups.Add(group);
            db.Students.Add(student);
            db.Subjects.Add(subject);
            db.Points.Add(points);
            await db.SaveChangesAsync();
            //pointsService.GetStudentPointsAsync(student.Id).Wait();
            Console.WriteLine("Добавили тестовую группу и студента.");
        }

        var allGroups = await groupRepo.GetAllAsync();
        Console.WriteLine($"Групп в базе: {allGroups.Count}");

        foreach (var g in allGroups)
        {
            Console.WriteLine($"Группа {g.GroupTitle}-{g.Subgroup}, студентов: {g.Students.Count}");
        }

    }
}