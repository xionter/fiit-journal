namespace FiitFlow.Server.SubTools.SubToolsUnits
{
    public class StudentLoginRequest
    {
        public required string FirstName { get; set; }
        public string LastName { get; set; }
        public string Group { get; set; }
        public DateTime DateTime { get; set; }
    }
}
