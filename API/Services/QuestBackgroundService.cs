using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using API_Pokemon.Data.Context;

namespace API_Pokemon.Services
{
    public class QuestBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<QuestBackgroundService> _logger;

        public QuestBackgroundService(IServiceScopeFactory scopeFactory,
                                      ILogger<QuestBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("QuestBackgroundService démarré.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();

                    var context = scope.ServiceProvider.GetRequiredService<MonsterContext>();
                    var questService = scope.ServiceProvider.GetRequiredService<QuestService>();

                    await questService.GenerateQuestsForConnectedEmployees();

                    _logger.LogInformation("Quêtes générées.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lors de la génération des quêtes.");
                }

                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
        }
    }
}
