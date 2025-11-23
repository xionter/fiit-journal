//using System.Net;
//using System.IO;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//
//using FiitFlow.Parser.Services;
//class Program
//{
//    static async Task Main(string[] args)
//    {
//        try
//        {
//            await RunApplicationAsync();
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine($"Критическая ошибка: {ex.Message}");
//        }
//    }
//
//    private static async Task RunApplicationAsync()
//    {
//        var configFile = "config.txt";
//        
//        if (!File.Exists(configFile))
//        {
//            return;
//        }
//
//        var configContent = await File.ReadAllLinesAsync(configFile);
//        var config = new ConfigParser().Parse(configContent);
//        
//        if (string.IsNullOrEmpty(config.StudentName))
//        {
//            Console.WriteLine("Ошибка: Не указано имя студента в config.txt");
//            return;
//        }
//
//        using var outputWriter = new FileOutputWriter("output.txt");
//        var searchService = new StudentSearchService(
//            new ExcelDownloader(new HttpClient()),
//            new ExcelParser(),
//            outputWriter
//        );
//
//        await searchService.SearchStudentInAllTablesAsync(config, studentName);
//        Console.WriteLine("Результаты сохранены в output.txt");
//    }
//}
