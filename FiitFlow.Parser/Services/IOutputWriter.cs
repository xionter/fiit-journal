using System;
using FiitFlow.Parser.Models;

namespace FiitFlow.Parser.Services
{
    public interface IOutputWriter : IDisposable
    {
        void WriteLine(string line);
        void WriteStudentData(string tableName, string sheetName, Student student);
        void WriteTableHeader(string tableName);
        void WriteEmptyLine();
        string GetContent();
    }
}
