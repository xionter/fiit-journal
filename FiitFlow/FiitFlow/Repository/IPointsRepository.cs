using FiitFlow.Domain;

namespace FiitFlow.Repository;

public interface IPointsRepository
{
    Task<IReadOnlyList<Points>> GetByStudentAsync(Guid studentId);
    Task<IReadOnlyList<Points>> GetByGroupAsync(Guid groupId, int? semester = null);

    // Сохранить результаты парсинга (обновить существующие записи или создать новые).
    Task UpsertRangeAsync(IEnumerable<Points> points);
}