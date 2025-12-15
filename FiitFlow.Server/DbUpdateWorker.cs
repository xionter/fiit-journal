namespace FiitFlow.Server
{
    using Microsoft.Extensions.Hosting;

    public class DbUpdateWorker : BackgroundService
    {
        private readonly ILogger<DbUpdateWorker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(15);

        public DbUpdateWorker(
            ILogger<DbUpdateWorker> logger,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("DbUpdateWorker started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var updater = scope.ServiceProvider.GetRequiredService<PointsService>();

                    await updater.UpdateAll();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при обновлении БД");
                }

                try
                {
                    await Task.Delay(_interval, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // приложение останавливается
                }
            }

            _logger.LogInformation("DbUpdateWorker stopping");
        }
    }
}
