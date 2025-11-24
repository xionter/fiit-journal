using FiitFlow.Parser.Models;
namespace FiitFlow.Parser.Interfaces
{
    public interface IStudentSearchService
    {
        Task SearchStudentInAllTablesAsync(ParserConfig config);
    }
}
