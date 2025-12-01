using System.Text.Json;
using System.Text.Json.Serialization;
using FiitFlow.Parser.Models;

namespace FiitFlow.Parser.Services;
class JsonService
{
    public static ParserConfig LoadConfig(string filePath)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        string json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<ParserConfig>(json, options);
    }

    public static void SaveResults(StudentResults results, string filePath)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        string json = JsonSerializer.Serialize(results, options);
        File.WriteAllText(filePath, json);
    }
}
