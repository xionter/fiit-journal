using FiitFlow.Parser.Models;

namespace FiitFlow.Parser.Interfaces
{
    public interface IConfigParser
    {
        AppConfig Parse(string[] lines);
    }
}
