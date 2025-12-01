using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace FiitFlow.Parser.Services;

public class ExcelDownloader
{
    private readonly HttpClient _httpClient;

    public ExcelDownloader(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Add(
            "User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
    }

    public async Task<string> DownloadAsync(string url, string outputPath)
    {
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync();
        await using var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
        await stream.CopyToAsync(fileStream);

        return outputPath;
    }
}
