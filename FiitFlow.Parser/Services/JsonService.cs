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
            PropertyNameCaseInsensitive = true,
            IgnoreUnknownProperties = true,
            WriteIndented = true
        };
        string json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<ParserConfig>(json, options) ?? new ParserConfig();
    }

    public static void SaveResults(StudentResults? results, string filePath)
    {
        if (results == null) return;
        
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
