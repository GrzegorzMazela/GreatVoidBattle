using GreatVoidBattle.Application.Factories;
using Microsoft.AspNetCore.SignalR;

namespace GreatVoidBattle.Application.Hubs;

public class BattleHub : Hub<BattleManagerFactory>
{
    public Task JoinBattle(string battleId)
        => Groups.AddToGroupAsync(Context.ConnectionId, $"battle-{battleId}");

    public Task LeaveBattle(string battleId)
        => Groups.RemoveFromGroupAsync(Context.ConnectionId, $"battle-{battleId}");

    // (opcjonalnie) autoryzacja, walidacje itp.
    public override Task OnConnectedAsync() => base.OnConnectedAsync();

    public override Task OnDisconnectedAsync(Exception? ex) => base.OnDisconnectedAsync(ex);
}