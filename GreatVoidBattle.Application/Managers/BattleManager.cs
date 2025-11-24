using GreatVoidBattle.Application.Events.Base;
using GreatVoidBattle.Application.Events.Handler;
using GreatVoidBattle.Application.Factories;
using GreatVoidBattle.Core.Domains;
using System.Collections.Generic;

namespace GreatVoidBattle.Application.Managers;

public class BattleManager
{
    public Guid BattleId => _battleState.BattleId;
    public BattleState BattleState => _battleState;
    public IReadOnlyList<BattleEvent> Events => _events.AsReadOnly();
    private readonly BattleState _battleState;
    private readonly List<BattleEvent> _events = [];
    private readonly EventDispatcher _eventDispatcher = new();

    public BattleManager(BattleState battleState)
    {
        _battleState = battleState;
    }

    public BattleManager(BattleState battleState, List<BattleEvent> events) : this(battleState)
    {
        _events = events;
    }

    public async Task ApplyEventAsync<T>(T battleEvent) where T : BattleEvent
    {
        await _eventDispatcher.DispatchAsync(battleEvent, _battleState);
        _events.Add(battleEvent);
    }
}