using FiitFlow;
using FiitFlow.Domain;
using FiitFlow.Parser.Services;
using FiitFlow.Repository;
using FiitFlow.Repository.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Text.Json;
using ParserConfig = FiitFlow.Parser.Models.ParserConfig;

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
        var configPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../FiitFlow.Parser/config.json"));
        var parserConfig = await LoadParserConfigAsync(configPath);
        var sampleStudentName = parserConfig?.StudentName ?? "Иванов Иван Иванович";
        
        if (File.Exists(configPath))
        {
            var firstGroup = await db.Groups.Include(g => g.Students).FirstAsync();
            Console.WriteLine($"Пробуем подтянуть баллы из парсера для {firstGroup.GroupTitle}-{firstGroup.Subgroup}");
            await pointsService.UpdatePointsForGroupAsync(firstGroup.Id, 3, configPath);

            var points = await pointsService.GetGroupPointsAsync(firstGroup.Id, 3);
            Console.WriteLine($"Найдено {points.Count} записей Points после парсинга:");
            foreach (var p in points)
            {
                Console.WriteLine($"Студент: {p.StudentId}, Предмет: {p.SubjectId}, Баллы: {p.Value}, Обновлено: {p.UpdatedAt}");
            }
        }
        else
        {
            Console.WriteLine($"Конфиг парсера не найден: {configPath}");
        }

        var allGroups = await groupRepo.GetAllAsync();
        Console.WriteLine($"Групп в базе: {allGroups.Count}");

        foreach (var g in allGroups)
        {
            Console.WriteLine($"Группа {g.GroupTitle}-{g.Subgroup}, студентов: {g.Students.Count}");
        }

    }

    private static async Task<ParserConfig?> LoadParserConfigAsync(string path)
    {
        if (!File.Exists(path))
            return null;

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = await File.ReadAllTextAsync(path);
        return JsonSerializer.Deserialize<ParserConfig>(json, options);
    }
}
