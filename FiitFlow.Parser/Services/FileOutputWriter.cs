using System;
using System.IO;
using System.Text;
using FiitFlow.Parser.Models;
using FiitFlow.Parser.Interfaces;

namespace FiitFlow.Parser.Services
{
    public class FileOutputWriter : IOutputWriter
    {
        private readonly StreamWriter _writer;
        private readonly string _filePath;

        public FileOutputWriter(string filePath)
        {
            _filePath = filePath;
            _writer = new StreamWriter(filePath, false, Encoding.UTF8);
        }

        public void WriteLine(string line) => _writer.WriteLine(line);

        public void WriteStudentData(string tableName, string sheetName, Student student)
        {
            _writer.WriteLine($"{sheetName}:");
            foreach (var (category, value) in student.Data)
            {
                if (category != "SheetName")
                {
                    _writer.WriteLine($"   {category}: {value}");
                }
            }
        }

        public void WriteTableHeader(string tableName) => _writer.WriteLine(tableName);

        public void WriteEmptyLine() => _writer.WriteLine();

        public string GetContent()
        {
            _writer.Flush();
            return File.ReadAllText(_filePath);
        }

        public void Dispose()
        {
            _writer?.Dispose();
        }
    }
}
