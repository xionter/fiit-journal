using FiitFlow.Domain;
using FiitFlow.Repository;
using FiitFlow.Parser.Services;
using FiitFlow.Parser.Models;
using System.Globalization;
using Microsoft.Extensions.Logging;

namespace FiitFlow;

public class PointsService
{
    private readonly ILogger<PointsService> _logger;
    private readonly IPointsRepository _pointsRepo;
    private readonly IStudentRepository _studentRepo;
    private readonly ISubjectRepository _subjectRepo;
    private readonly IGroupRepository _groupRepo;

    public PointsService(
        ILogger<PointsService> logger,
        IPointsRepository pointsRepo,
        IStudentRepository studentRepo,
        ISubjectRepository subjectRepo,
        IGroupRepository groupRepo)
    {
        _pointsRepo = pointsRepo;
        _studentRepo = studentRepo;
        _subjectRepo = subjectRepo;
        _groupRepo = groupRepo;
        _logger = logger;
    }

    public async Task UpdateAll()
    {
        foreach (var group in await _groupRepo.GetAllAsync())
        {
            var rootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../.."));
            var groupConfigPath = Path.Combine(rootPath, "cfg", $"{group.GroupTitle}");
            _logger.LogInformation("begin DB Update");
            await UpdatePointsForGroupAsync(group.Id, group.GetCurrentSemester(), groupConfigPath);
            _logger.LogInformation("end DB Update");
        }
    }

    public async Task UpdatePointsForGroupAsync(Guid groupId, int semester, string configPath)
    {
        try
        {
            var students = await _studentRepo.GetByGroupAsync(groupId);
            if (students.Count == 0)
            {
                Console.WriteLine($"В группе {groupId} нет студентов");
                return;
            }

            Console.WriteLine($"Обработка {students.Count} студентов группы...");

            var collectedPoints = new List<Points>();
            var subjectCache = new Dictionary<string, Subject>();
            var parserService = new FiitFlowParserService();
            int processedCount = 0;

            foreach (var student in students)
            {
                try 
                {
                    Console.WriteLine($"\nОбработка студента: {student.FullName}");
                    var studentConfigPath = Path.Combine(configPath, $"{student.FullName}.json");
                    var confEditor = new ConfigEditorService(studentConfigPath);
                    confEditor.SetCache("./Cache", true);
                    var studentResult = await parserService.ParseWithFormulasAsync(studentConfigPath, student.FullName);
                    
                    if (studentResult?.Subjects == null || !studentResult.Subjects.Any())
                    {
                        Console.WriteLine($"  Не найдены данные для студента {student.FullName}"); 
                        continue;
                    }

                    foreach (var subjectEntry in studentResult.Subjects)
                    {
                        var subjectName = subjectEntry.Key;
                        var subjectData = subjectEntry.Value;

                        double finalScore = GetFinalScore(subjectName, subjectData, studentResult.RatingScores);
                        
                        if (finalScore <= 0)
                        {
                            Console.WriteLine($"  {subjectName}: нет баллов");
                            continue;
                        }

                        var subjectCacheKey = $"{groupId}-{semester}-{subjectName}";
                        if (!subjectCache.TryGetValue(subjectCacheKey, out var subject))
                        {
                            var tableUrl = subjectData.Tables.FirstOrDefault()?.TableUrl ?? "";
                            subject = await _subjectRepo.GetOrCreateAsync(groupId, subjectName, semester, tableUrl);
                            subjectCache[subjectCacheKey] = subject;
                        }

                        int roundedValue = (int)Math.Round(finalScore);
                        Console.WriteLine($"  {subjectName}: {finalScore:F2} → {roundedValue} баллов");

                        collectedPoints.Add(new Points
                        {
                            Id = Guid.NewGuid(),
                            StudentId = student.Id,
                            SubjectId = subject.Id,
                            Semester = semester,
                            Value = roundedValue,
                            UpdatedAt = DateTime.UtcNow
                        });
                    }

                    processedCount++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка обработки студента {student.FullName}: {ex.Message}");
                }
            }

            if (collectedPoints.Count > 0)
            {
                await _pointsRepo.UpsertRangeAsync(collectedPoints);
                Console.WriteLine($"\nУспешно обновлено {collectedPoints.Count} записей баллов для {processedCount} студентов");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при обновлении баллов группы: {ex.Message}");
        }
    }

    private double GetFinalScore(string subjectName, SubjectResults subjectData, Dictionary<string, double> ratingScores)
    {
        if (ratingScores.TryGetValue(subjectName, out double ratingScore))
        {
            return ratingScore;
        }

        if (subjectData.CalculatedScore > 0)
        {
            return subjectData.CalculatedScore;
        }

        if (subjectData.Overall.TryGetValue("ExtractedData", out var extractedDataObj) &&
            extractedDataObj is Dictionary<string, object> extractedData)
        {
            foreach (var entry in extractedData)
            {
                string key = entry.Key.ToLower();
                if (key.Contains("итог") || key.Contains("сумма") || key.Contains("итого"))
                {
                    try
                    {
                        if (entry.Value is string strValue)
                        {
                            if (double.TryParse(strValue.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
                                return value;
                        }
                        else if (entry.Value is double dValue)
                        {
                            return dValue;
                        }
                        else if (entry.Value is int iValue)
                        {
                            return iValue;
                        }
                    }
                    catch
                    {
                        // Пропускаем ошибки парсинга
                    }
                }
            }
        }

        return 0;
    }

    public Task<IReadOnlyList<Points>> GetStudentPointsAsync(int studentId)
        => _pointsRepo.GetByStudentAsync(studentId);

    public Task<IReadOnlyList<Points>> GetGroupPointsAsync(Guid groupId, int? semester = null)
        => _pointsRepo.GetByGroupAsync(groupId, semester);

    public async Task<IReadOnlyList<StudentPointsSummary>> GetGroupSummaryAsync(Guid groupId, int semester)
    {
        var points = await _pointsRepo.GetByGroupAsync(groupId, semester);
        var students = await _studentRepo.GetByGroupAsync(groupId);
        var subjects = await _subjectRepo.GetByGroupAsync(groupId, semester);

        var summary = new List<StudentPointsSummary>();

        foreach (var student in students)
        {
            var studentPoints = points.Where(p => p.StudentId == student.Id).ToList();
            var subjectScores = new Dictionary<string, int>();

            foreach (var point in studentPoints)
            {
                var subject = subjects.FirstOrDefault(s => s.Id == point.SubjectId);
                if (subject != null)
                {
                    subjectScores[subject.Title] = point.Value;
                }
            }

            summary.Add(new StudentPointsSummary
            {
                StudentId = student.Id,
                StudentName = student.FullName,
                SubjectScores = subjectScores,
                TotalScore = subjectScores.Sum(x => x.Value)
            });
        }

        return summary.OrderByDescending(s => s.TotalScore).ToList();
    }
}

public class StudentPointsSummary
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public Dictionary<string, int> SubjectScores { get; set; } = new();
    public int TotalScore { get; set; }
}
