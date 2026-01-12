using System;
using System.Linq;
using System.Text.Json;
using FiitFlow.Domain;
using FiitFlow.Repository;
using FiitFlow.Repository.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ParserConfig = FiitFlow.Parser.Models.ParserConfig;
using Microsoft.EntityFrameworkCore.Update.Internal;
using FiitFlow.Parser.Services;
using FiitFlow.Parser.Interfaces;

namespace FiitFlow;

public class Program
{
    public static async Task Main2(string[] args)
    {
        var httpClient = new HttpClient();
        IExcelDownloader excelDownloader = new ExcelDownloader(httpClient);
        IExcelParser excelParser = new ExcelParser();
        IStudentSearchService studentSearchService = new StudentSearchService(excelDownloader, excelParser);
        var formulaCalculator = new FormulaCalculatorService();

        var fiitFlowParserService = new FiitFlowParserService(studentSearchService,formulaCalculator);

        var rootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
        var dbPath = Path.Combine(rootPath, "fiitflow.db");
        var cfgBasePath = Path.Combine(rootPath, "cfg");

        Console.WriteLine($"Путь к БД: {dbPath}");
        Console.WriteLine($"Путь к конфигам: {cfgBasePath}");

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={dbPath}")
            .Options;

        using var db = new AppDbContext(options);
        await db.Database.EnsureCreatedAsync();

        IGroupRepository groupRepo = new GroupRepository(db);
        IStudentRepository studentRepo = new StudentRepository(db, groupRepo);
        ISubjectRepository subjectRepo = new SubjectRepository(db);
        IPointsRepository pointsRepo = new PointsRepository(db);

        var logger = NullLoggerFactory.Instance.CreateLogger<PointsService>();

        var pointsService = new PointsService(logger, pointsRepo, studentRepo, subjectRepo, groupRepo, fiitFlowParserService);
        await studentRepo.GetOrCreateAsync("Дарья", "Макарова", "ФТ-201", 2);

        await pointsService.UpdateAll();

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

    private static async Task<ParserConfig?> LoadParserConfigAsync(string path)
    {
        if (!File.Exists(path))
            return null;

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            IgnoreUnknownProperties = true
        };

        var json = await File.ReadAllTextAsync(path);
        return JsonSerializer.Deserialize<ParserConfig>(json, options);
    }


}
