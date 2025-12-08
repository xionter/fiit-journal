using FiitFlow;
using FiitFlow.Domain;
using FiitFlow.Repository;
using FiitFlow.Repository.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace FiitFlow;

public class Program
{
    public static async Task Main(string[] args)
    {
        var rootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
        var dbPath = Path.Combine(rootPath, "fiitflow.db");
        var configPath = Path.Combine(rootPath, "FiitFlow.Parser", "config.json");
        
        Console.WriteLine($"Путь к БД: {dbPath}");
        Console.WriteLine($"Путь к конфигу: {configPath}");

        if (!System.IO.File.Exists(configPath))
        {
            Console.WriteLine($"Конфиг парсера не найден: {configPath}");
            return;
        }

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={dbPath}")
            .Options;

        using var db = new AppDbContext(options);
        await db.Database.EnsureCreatedAsync();
        
        IGroupRepository groupRepo = new GroupRepository(db);
        IStudentRepository studentRepo = new StudentRepository(db);
        ISubjectRepository subjectRepo = new SubjectRepository(db);
        IPointsRepository pointsRepo = new PointsRepository(db);
        
        var pointsService = new PointsService(pointsRepo, studentRepo, subjectRepo);
        
        var allGroups = await groupRepo.GetAllAsync();
        Console.WriteLine($"\nНайдено групп: {allGroups.Count}");

        if (allGroups.Count == 0)
        {
            Console.WriteLine("Группы не найдены в базе данных");
            return;
        }

        foreach (var group in allGroups)
        {
            try
            {
                Console.WriteLine($"\n=== Обработка группы: {group.GroupTitle}-{group.Subgroup} ===");
                Console.WriteLine($"Студентов в группе: {group.Students.Count}");
                
                await pointsService.UpdatePointsForGroupAsync(group.Id, 3, configPath);

                var summary = await pointsService.GetGroupSummaryAsync(group.Id, 3);
                
                if (summary.Count == 0)
                {
                    Console.WriteLine($"Нет данных для группы {group.GroupTitle}");
                    continue;
                }
                
                Console.WriteLine($"\nИтоговая сводка для группы {group.GroupTitle}:");
                Console.WriteLine($"{"№",3} | {"Студент",-30} | {"Предметов",10} | {"Всего баллов",12}");
                Console.WriteLine(new string('-', 70));
                
                int position = 1;
                foreach (var studentSummary in summary)
                {
                    var subjectsCount = studentSummary.SubjectScores.Count;
                    Console.WriteLine($"{position,3} | {studentSummary.StudentName,-30} | {subjectsCount,10} | {studentSummary.TotalScore,12}");
                    position++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обработки группы {group.GroupTitle}: {ex.Message}");
            }
        }

        Console.WriteLine($"\n=== Общая статистика базы данных ===");
        Console.WriteLine($"Всего групп: {db.Groups.Count()}");
        Console.WriteLine($"Всего студентов: {db.Students.Count()}");
        Console.WriteLine($"Всего предметов: {db.Subjects.Count()}");
        Console.WriteLine($"Всего записей баллов: {db.Points.Count()}");
        
        Console.WriteLine($"\n=== Примеры записей баллов в базе ===");
        var samplePoints = db.Points
            .Include(p => p.Student)
            .Include(p => p.Subject)
            .Take(5)
            .ToList();
            
        foreach (var point in samplePoints)
        {
            Console.WriteLine($"Студент: {point.Student?.FullName ?? "N/A"}, " +
                             $"Предмет: {point.Subject?.Title ?? "N/A"}, " +
                             $"Баллы: {point.Value}, Семестр: {point.Semester}");
        }
    }
}
