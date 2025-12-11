
namespace FiitFlow.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var policyClient = "ReactClient";

            // Add services to the container.
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(policyClient,
                    builder =>
                    {
                        builder//.WithOrigins(
                               //"https://localhost:7242",
                               //"https://localhost:49575",
                               //"http://localhost:5173",
                               //"http://localhost:5273"
                               //)
                            .AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                    });
            });

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            app.UseCors(policyClient);

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

            app.UseAuthorization();


            app.MapControllers();

            app.MapFallbackToFile("/index.html");

            app.Run();
        }
    }
}
