using FiitFlow.Parser.Models;

namespace FiitFlow.Parser.Interfaces
{
    public interface IStudentSearchService
    {
        Task<StudentSearchResult> SearchStudentInAllTablesAsync(
            ParserConfig config,
            string? studentName = null);
    }
}

