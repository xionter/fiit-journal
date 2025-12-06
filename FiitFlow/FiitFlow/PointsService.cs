//using FiitFlow.Domain;
//using FiitFlow.Repository;
//using FiitFlow.Parser.Services;
//using System.Globalization;
//using FiitFlow.Parser.Models;
//
//namespace FiitFlow;
//
//public class PointsService
//{
//    private readonly IPointsRepository _pointsRepo;
//    private readonly IStudentRepository _studentRepo;
//    private readonly ISubjectRepository _subjectRepo;
//
//    public PointsService(
//        IPointsRepository pointsRepo,
//        IStudentRepository studentRepo,
//        ISubjectRepository subjectRepo)
//    {
//        _pointsRepo = pointsRepo;
//        _studentRepo = studentRepo;
//        _subjectRepo = subjectRepo;
//    }
//
//
//    public async Task UpdatePointsForGroupAsync(Guid groupId, int semester, string configPath)
//    {
//        var parserService = new FiitFlowParserService();
//        var collectedPoints = new List<Points>();
//        var cache = new Dictionary<string, Subject>();
//
//        var parsedResult = await parserService.ParseAsync(configPath, null);
//
//        foreach (var table in parsedResult.Tables)
//        {
//            try
//            {
//                if (string.IsNullOrWhiteSpace(table.StudentName))
//                {
//                    Console.WriteLine($"Пропускаем строку без имени студента в таблице {table.TableName}");
//                    continue;
//                }
//
//                var student = await _studentRepo.GetOrCreateAsync(table.StudentName, groupId);
//
//                var subjectCacheKey = $"{groupId}-{semester}-{table.TableName}";
//                if (!cache.TryGetValue(subjectCacheKey, out var subject))
//                {
//                    subject = await _subjectRepo.GetOrCreateAsync(groupId, table.TableName, semester, table.TableUrl);
//                    cache[subjectCacheKey] = subject;
//                }
//
//                var value = CalculatePoints(table.Data);
//
//                Console.WriteLine($"Студент {student.FullName}: {table.TableName} => {value} баллов");
//
//                collectedPoints.Add(new Points
//                {
//                    Id = Guid.NewGuid(),
//                    StudentId = student.Id,
//                    SubjectId = subject.Id,
//                    Semester = semester,
//                    Value = value,
//                    UpdatedAt = DateTime.UtcNow
//                });
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Не удалось спарсить данные из таблицы {table.TableName} для студента {table.StudentName}: {ex.Message}");
//            }
//        }
//
//        if (collectedPoints.Count > 0)
//        {
//            await _pointsRepo.UpsertRangeAsync(collectedPoints);
//        }
//    }
//
//    public Task<IReadOnlyList<Points>> GetStudentPointsAsync(Guid studentId)
//        => _pointsRepo.GetByStudentAsync(studentId);
//
//    public Task<IReadOnlyList<Points>> GetGroupPointsAsync(Guid groupId, int? semester = null)
//        => _pointsRepo.GetByGroupAsync(groupId, semester);
//
//    private static int CalculatePoints(Dictionary<string, string> data)
//    {
//        double sum = 0;
//
//        foreach (var entry in data)
//        {
//            if (double.TryParse(entry.Value.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
//            {
//                sum += value;
//            }
//        }
//
//        return (int)Math.Round(sum);
//    }
//}
