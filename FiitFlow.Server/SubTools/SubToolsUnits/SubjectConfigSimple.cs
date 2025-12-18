namespace FiitFlow.Server.SubTools.SubToolsUnits
{
    public class SubjectConfigSimple
    {
        public string BaseName { get; set; }
        public string Name { get; set; }
        public string Link { get; set; }
        public string Formula { get; set; }
        public SheetSimple[] Sheets { get; set; }
    }

    public class SheetSimple
    {
        public string sheetName { get; set; }
        public int headerRow { get; set; }
    }
}
