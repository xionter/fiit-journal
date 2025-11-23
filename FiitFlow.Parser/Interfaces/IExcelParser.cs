public interface IExcelParser
{
    IEnumerable<Student> FindStudents(string filePath, string studentName, TableConfig tableConfig);
}
