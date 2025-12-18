using FiitFlow.Parser.Models;
public interface IExcelDownloader
{
    Task<string> DownloadAsync(TableConfig table);
}
