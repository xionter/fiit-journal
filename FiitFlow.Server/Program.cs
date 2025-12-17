
using FiitFlow;
using FiitFlow.Parser.Services;
using FiitFlow.Repository;
using FiitFlow.Repository.Sqlite;
using FiitFlow.Server;
using FiitFlow.Domain.Extensions;
using FiitFlow.Server.SubTools;
using FiitFlow.Server.SubTools.SubToolsUnits;
using Microsoft.EntityFrameworkCore;

namespace FiitFlowReactApp.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var policyClient = "AllowLocalhost";

            var rootPathProvider = new RootPathProvider();
            var rootPath = rootPathProvider.GetRootPath();
            var dbPath   = Path.Combine(rootPath, "fiitflow.db");

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            builder.Services.AddScoped<IGroupRepository, GroupRepository>();
            builder.Services.AddScoped<IStudentRepository, StudentRepository>();
            builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();
            builder.Services.AddScoped<IPointsRepository, PointsRepository>();

            builder.Services.AddSingleton<IRootPathProvider>(rootPathProvider);
            builder.Services.AddScoped<IStudentConfigService>(sp =>
                new StudentConfigService(sp.GetRequiredService<IRootPathProvider>().GetRootPath()));
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
