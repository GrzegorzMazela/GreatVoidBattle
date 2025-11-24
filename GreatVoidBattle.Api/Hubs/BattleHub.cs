using GreatVoidBattle.Application.Factories;
using GreatVoidBattle.Application.Repositories;
using Microsoft.AspNetCore.SignalR;

namespace GreatVoidBattle.Application.Hubs;

public class BattleHub : Hub
{
    private readonly IBattleStateRepository _battleStateRepository;

    public BattleHub(IBattleStateRepository battleStateRepository)
    {
        _battleStateRepository = battleStateRepository;
    }

    public Task JoinBattle(string battleId)
        => Groups.AddToGroupAsync(Context.ConnectionId, $"battle-{battleId}");

    public Task LeaveBattle(string battleId)
        => Groups.RemoveFromGroupAsync(Context.ConnectionId, $"battle-{battleId}");

    // Powiadomienie o zakończeniu tury przez gracza
    public async Task NotifyPlayerFinishedTurn(string battleId, string fractionId, string playerName)
    {
        await Clients.Group($"battle-{battleId}").SendAsync("PlayerFinishedTurn", new
        {
            FractionId = fractionId,
            PlayerName = playerName,
            Timestamp = DateTime.UtcNow
        });
    }

    // Powiadomienie o rozpoczęciu nowej tury
    public async Task NotifyNewTurnStarted(string battleId, int turnNumber)
    {
        await Clients.Group($"battle-{battleId}").SendAsync("NewTurnStarted", new
        {
            TurnNumber = turnNumber,
            Timestamp = DateTime.UtcNow
        });
    }

    // Powiadomienie o aktualizacji listy oczekujących graczy
    public async Task NotifyWaitingPlayers(string battleId, object waitingPlayers)
    {
        await Clients.Group($"battle-{battleId}").SendAsync("WaitingPlayersUpdated", waitingPlayers);
    }

    public override Task OnConnectedAsync() => base.OnConnectedAsync();

    public override Task OnDisconnectedAsync(Exception? ex) => base.OnDisconnectedAsync(ex);
}