using GreatVoidBattle.Application.Events;
using GreatVoidBattle.Application.Events.Base;
using GreatVoidBattle.Application.Managers;
using GreatVoidBattle.Application.Repositories;
using GreatVoidBattle.Core.Domains;
using Microsoft.Extensions.Caching.Memory;

namespace GreatVoidBattle.Application.Factories;

public class BattleManagerFactory(IMemoryCache _memoryCache, IBattleStateRepository _battleStateRepository,
    IBattleEventRepository _battleEventRepository)
{
    private static readonly TimeSpan absoluteExpiration = TimeSpan.FromDays(1);

    private readonly MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
        .SetAbsoluteExpiration(absoluteExpiration);

    public Guid CreateNewBattle(CreateBattleEvent createBattleEvent)
    {
        var battleManager = new BattleManager(BattleState.CreateNew(createBattleEvent.Name, createBattleEvent.Width, createBattleEvent.Height), new List<BattleEvent>());
        SetBattleManager(battleManager);
        _battleStateRepository.AddAsync(battleManager.BattleState);
        return battleManager.BattleId;
    }

    public async Task ApplyEventAsync(BattleEvent battleEvent)
    {
        var battleManager = await GetBattleManager(battleEvent.BattleId);
        await battleManager.ApplyEventAsync(battleEvent);
        SetBattleManager(battleManager);
        await _battleStateRepository.UpdateAsync(battleManager.BattleState.BattleId, battleManager.BattleState);
        await _battleEventRepository.AddAsync(battleEvent);
    }

    public async Task<BattleState> GetBattleState(Guid battleId)
    {
        var battleManager = await GetBattleManager(battleId);
        return battleManager.BattleState;
    }

    private void SetBattleManager(BattleManager battleManager)
    {
        _memoryCache.Set($"BattleState_{battleManager.BattleId}", battleManager.BattleState, cacheEntryOptions);
        _memoryCache.Set($"Events_{battleManager.BattleId}", battleManager.Events, cacheEntryOptions);
    }

    private async Task<BattleManager> GetBattleManager(Guid battleId)
    {
        if (!_memoryCache.TryGetValue<BattleState>($"BattleState_{battleId}", out var battleState))
        {
            battleState = await _battleStateRepository.GetByIdAsync(battleId);
            _memoryCache.Set($"BattleState_{battleState!.BattleId}", battleState, cacheEntryOptions);
        }
        //if (!_memoryCache.TryGetValue<List<BattleEvent>>($"Events_{battleId}", out var events))
        //{
        //    events = await _battleEventRepository.GetByBattleIdAsync(battleId);
        //}
        return new BattleManager(battleState!, new List<BattleEvent>());
    }
}