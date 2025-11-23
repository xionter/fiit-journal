using FiitFlow.Parser.Models;
public interface IOutputWriter
{
    void WriteStudentData(string tableName, string sheetName, Student student);
    void WriteTableHeader(string tableName);
    void WriteEmptyLine();
}
