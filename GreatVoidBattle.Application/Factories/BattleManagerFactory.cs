using GreatVoidBattle.Application.Events;
using GreatVoidBattle.Application.Events.Base;
using GreatVoidBattle.Application.Managers;
using GreatVoidBattle.Core.Domains;
using Microsoft.Extensions.Caching.Memory;

namespace GreatVoidBattle.Application.Factories;

public class BattleManagerFactory(IMemoryCache _memoryCache)
{
    public void CreateNewBattle(CreateBattleEvent createBattleEvent)
    {
        var battleManager = new BattleManager(BattleState.CreateNew(createBattleEvent.Name), new List<BattleEvent>());
        SetBattleManager(battleManager);
    }

    public async Task ApplyEventAsync(BattleEvent battleEvent)
    {
        var battleManager = GetBattleManager(battleEvent.BattleId);
        await battleManager.ApplyEventAsync(battleEvent);
        SetBattleManager(battleManager);
    }

    public BattleState GetBattleState(Guid battleId)
    {
        var battleManager = GetBattleManager(battleId);
        return battleManager.BattleState;
    }

    private void SetBattleManager(BattleManager battleManager)
    {
        var absoluteExpiration = TimeSpan.FromDays(1);
        var cacheEntryOptions = new MemoryCacheEntryOptions();
        cacheEntryOptions.SetAbsoluteExpiration(absoluteExpiration);

        _memoryCache.Set($"BattleState_{battleManager.BattleId}", battleManager.BattleState, cacheEntryOptions);
        _memoryCache.Set($"Events_{battleManager.BattleId}", battleManager.Events, cacheEntryOptions);
    }

    private BattleManager GetBattleManager(Guid battleId)
    {
        if (!_memoryCache.TryGetValue<BattleState>($"BattleState_{battleId}", out var battleState))
        {
            //TODO: Handle not found
        }
        if (!_memoryCache.TryGetValue<List<BattleEvent>>($"Events_{battleId}", out var events))
        {
            //TODO: Handle not found
        }
        return new BattleManager(battleState, events);
    }
}