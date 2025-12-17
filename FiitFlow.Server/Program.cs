using FiitFlow;
using FiitFlow.Repository;
using FiitFlow.Repository.Sqlite;
using FiitFlow.Server;
using FiitFlow.Server.SubTools;
using FiitFlow.Server.SubTools.SubToolsUnits;
using Microsoft.EntityFrameworkCore;
using FiitFlow.Parser.Interfaces;
using FiitFlow.Parser.Services;
namespace FiitFlowReactApp.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var policyClient = "AllowLocalhost";

            var rootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../"));
            var dbPath   = Path.Combine(rootPath, "fiitflow.db");

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            builder.Services.AddScoped<IGroupRepository, GroupRepository>();
            builder.Services.AddScoped<IStudentRepository, StudentRepository>();
            builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();
            builder.Services.AddScoped<IPointsRepository, PointsRepository>();

            builder.Services.AddScoped<PointsService, PointsService>();

            builder.Services.AddScoped<IAuthentication, AuthenticationTool>();

            builder.Services.AddHostedService<DbUpdateWorker>();

            builder.Services.AddControllers();

            // Add services to the container.
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(policyClient,
                    builder =>
                    {
                        builder.WithOrigins(
                               "https://localhost:7242",
                               "https://localhost:49757",
                               "http://localhost:5173",
                               "http://localhost:5273"
                               )
                            //.AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials();
                    });
            });
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            
            builder.Services.AddSingleton<HttpClient>();

            builder.Services.AddSingleton<CacheService>(sp =>
                    new CacheService("./Cache", false));

            builder.Services.AddSingleton<IExcelDownloader, ExcelDownloader>();
            builder.Services.AddSingleton<IExcelParser, ExcelParser>();

            builder.Services.AddSingleton<IStudentSearchService, StudentSearchService>();

            builder.Services.AddSingleton<FormulaCalculatorService>();

            builder.Services.AddSingleton<FiitFlowParserService>();
            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();
            }

            app.UseDefaultFiles();
            app.MapStaticAssets();
            app.UseStaticFiles();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                //app.UseSwagger();
                //app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors(policyClient);
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapFallbackToFile("/index.html");

            app.Run();
        }
    }
}
