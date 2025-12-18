namespace FiitFlow.Server.SubTools.SubToolsUnits
{
    public class SubjectConfigSimple
    {
        public string BaseName { get; set; }
        public string Name { get; set; }
        public string Link { get; set; }
        public string Formula { get; set; }
        public IEnumerable<(string sheetName, int headerRow)> Sheets { get; set; }
    }
}
