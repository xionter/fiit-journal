using System.IO;
using System.Text;
using System.Linq;

public class FileOutputWriter : IDisposable
{
    private readonly StreamWriter _writer;

    public FileOutputWriter(string filePath)
    {
        _writer = new StreamWriter(filePath, false, Encoding.UTF8);
    }

    public void WriteStudentData(string tableName, string sheetName, Student student)
    {
        _writer.WriteLine($"{sheetName}:");
        foreach (var (category, value) in student.Data.Where(x => x.Key != "SheetName"))
        {
            _writer.WriteLine($"   {category}: {value}");
        }
    }

    public void WriteTableHeader(string tableName) => _writer.WriteLine(tableName);
    public void WriteEmptyLine() => _writer.WriteLine();
    
    public void Dispose()
    {
        _writer?.Dispose();
    }
}
