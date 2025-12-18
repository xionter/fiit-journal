using FiitFlow.Parser.Models;

namespace FiitFlow.Parser.Interfaces
{
    public interface IStudentSearchService
    {
        Task<StudentSearchResult> SearchStudentInAllTablesAsync(
            ParserConfig config,
            string? studentName = null);

        Task<StudentSearchResult> SearchStudentInTableAsync(
            ParserConfig config, string table);
    }
}

