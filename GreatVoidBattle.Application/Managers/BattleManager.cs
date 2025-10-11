using GreatVoidBattle.Application.Events.Base;
using GreatVoidBattle.Application.Events.Handler;
using GreatVoidBattle.Application.Factories;
using GreatVoidBattle.Core.Domains;

namespace GreatVoidBattle.Application.Managers;

public class BattleManager
{
    public Guid BattleId => _battleState.BattleId;
    public BattleState BattleState => _battleState;
    public IReadOnlyList<BattleEvent> Events => _events.AsReadOnly();
    private BattleState _battleState;
    private List<BattleEvent> _events = new();
    private EventDispatcher _eventDispatcher = new();

    public BattleManager(BattleState battleState)
    {
        _battleState = battleState;
    }

    public BattleManager(BattleState battleState, List<BattleEvent> events) : this(battleState)
    {
        _events = events;
    }

    public async Task ApplyEventAsync(BattleEvent battleEvent)
    {
        await _eventDispatcher.DispatchAsync(battleEvent, _battleState);
        _events.Add(battleEvent);
    }
}