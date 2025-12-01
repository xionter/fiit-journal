using FiitFlow.Domain;
using FiitFlow.Repository;

namespace FiitFlow;

public class PointsService
{
    private readonly IPointsRepository _pointsRepo;
    private readonly IStudentRepository _studentRepo;
    private readonly ISubjectRepository _subjectRepo;

    public PointsService(
        IPointsRepository pointsRepo,
        IStudentRepository studentRepo,
        ISubjectRepository subjectRepo)
    {
        _pointsRepo = pointsRepo;
        _studentRepo = studentRepo;
        _subjectRepo = subjectRepo;
    }


    public async Task UpdatePointsForGroupAsync(Guid groupId, int semester)
    {
       
    }

    public Task<IReadOnlyList<Points>> GetStudentPointsAsync(Guid studentId)
        => _pointsRepo.GetByStudentAsync(studentId);

    public Task<IReadOnlyList<Points>> GetGroupPointsAsync(Guid groupId, int? semester = null)
        => _pointsRepo.GetByGroupAsync(groupId, semester);
}