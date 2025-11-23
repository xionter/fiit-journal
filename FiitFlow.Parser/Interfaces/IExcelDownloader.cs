public interface IExcelDownloader
{
    Task<string> DownloadAsync(string url, string outputPath);
}
