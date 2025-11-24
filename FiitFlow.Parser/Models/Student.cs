using System.Collections.Generic;
namespace FiitFlow.Parser.Models;

public class Student
{
    public string FullName { get; set; } = string.Empty;
    public Dictionary<string, string> Data { get; set; } = new();
}

