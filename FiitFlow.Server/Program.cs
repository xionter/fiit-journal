
using FiitFlow.Repository;
using FiitFlow.Repository.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FiitFlowReactApp.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var policyClient = "AllowLocalhost";

            builder.Services.AddDbContext<AppDbContext>(
                options => options.UseSqlite(
                    $"Data Source={builder.Configuration.GetValue<string>("DbPath")}"));

            // Регистрация репозиториев с использованием интерфейсов
            builder.Services.AddScoped<IGroupRepository, GroupRepository>();
            builder.Services.AddScoped<IStudentRepository, StudentRepository>();
            builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();
            builder.Services.AddScoped<IPointsRepository, PointsRepository>();

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
