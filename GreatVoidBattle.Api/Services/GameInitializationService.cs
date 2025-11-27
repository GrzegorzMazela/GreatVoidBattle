using GreatVoidBattle.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GreatVoidBattle.Api.Services;

public class GameInitializationService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<GameInitializationService> _logger;

    public GameInitializationService(
        IServiceProvider serviceProvider,
        ILogger<GameInitializationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting game initialization check...");

        using var scope = _serviceProvider.CreateScope();
        var gameStateService = scope.ServiceProvider.GetRequiredService<GameStateService>();

        try
        {
            // Sprawdź czy istnieje aktywna gra
            var activeSession = await gameStateService.GetActiveSessionAsync();
            
            if (activeSession != null)
            {
                _logger.LogInformation($"Active game session already exists: {activeSession.Name} (ID: {activeSession.Id})");
                return;
            }

            // Jeśli nie ma aktywnej gry, zainicjalizuj nową
            _logger.LogInformation("No active game session found. Initializing new game...");
            
            var result = await gameStateService.InitializeGameAsync("Great Void Battle - Main Game");
            
            _logger.LogInformation($"Game session initialized successfully!");
            _logger.LogInformation($"Session ID: {result.SessionId}");
            _logger.LogInformation($"Fractions created: {string.Join(", ", result.Fractions.Select(f => f.FractionName))}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during game initialization");
            // Nie rzucamy wyjątku, aby nie zatrzymać aplikacji
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Game initialization service stopped");
        return Task.CompletedTask;
    }
}
