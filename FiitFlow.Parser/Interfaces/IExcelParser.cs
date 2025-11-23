using FiitFlow.Parser.Models;
namespace FiitFlow.Parser.Interfaces
{
    public interface IExcelParser
    {
        IEnumerable<Student> FindStudents(string filePath, string studentName, TableConfig tableConfig);
    }
}
