using System.Text;

namespace FiitFlow.Parser.Services
{
    public class StringOutputWriter : IOutputWriter
    {
        private readonly StringBuilder _sb = new();

        public void WriteLine(string line)
        {
            _sb.AppendLine(line);
        }

        public string GetContent() => _sb.ToString();
    }
    public interface IOutputWriter
    {
        void WriteLine(string line);
    }
}

