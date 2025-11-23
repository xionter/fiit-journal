using System;
using System.Text;
using FiitFlow.Parser.Models;

namespace FiitFlow.Parser.Services
{
    public class StringOutputWriter : IOutputWriter
    {
        private readonly StringBuilder _sb = new();

        public void WriteLine(string line) => _sb.AppendLine(line);

        public void WriteStudentData(string tableName, string sheetName, Student student)
        {
            _sb.AppendLine($"{sheetName}:");
            foreach (var (category, value) in student.Data)
            {
                if (category != "SheetName")
                {
                    _sb.AppendLine($"   {category}: {value}");
                }
            }
        }

        public void WriteTableHeader(string tableName) => _sb.AppendLine(tableName);

        public void WriteEmptyLine() => _sb.AppendLine();

        public string GetContent() => _sb.ToString();

        public void Dispose()
        {
        }
    }
}
