using GreatVoidBattle.Application.Events;
using GreatVoidBattle.Application.Events.Base;
using GreatVoidBattle.Application.Events.Handler;
using GreatVoidBattle.Application.Events.Handler.Base;
using GreatVoidBattle.Application.Events.InProgress;
using GreatVoidBattle.Application.Events.InProgress.Handler;
using GreatVoidBattle.Core.Domains;

namespace GreatVoidBattle.Application.Factories;

/// <summary>
/// Dispatches battle events to their respective handlers without using reflection.
/// Uses a dictionary of typed handler functions for type-safe event dispatching.
/// </summary>
public class EventDispatcher
{
    private readonly Dictionary<Type, Func<BattleEvent, BattleState, Task>> _handlers = new();

    public EventDispatcher()
    {
        // Register all event handlers here
        RegisterHandler<AddFractionEvent>(new AddFractionEventHandler());
        RegisterHandler<UpdateFractionEvent>(new UpdateFractionEventHandler());
        RegisterHandler<AddFractionShipEvent>(new AddFractionShipEventHandler());
        RegisterHandler<UpdateFractionShipEvent>(new UpdateFractionShipEventHandler());
        RegisterHandler<StartBattleEvent>(new StartBattleEventHandler());
        RegisterHandler<SetShipPositionEvent>(new SetShipPositionEventHandler());
        RegisterHandler<AddLaserShotEvent>(new AddLaserShotEventHandler());
        RegisterHandler<AddMissileShotEvent>(new AddMissileShotEventHandler());
        RegisterHandler<AddShipMoveEvent>(new AddShipMoveEventHandler());
        RegisterHandler<EndOfTurnEvent>(new EndOfTurnEventHandler());
        // Add more handlers as needed
    }

    /// <summary>
    /// Registers a typed event handler with a wrapper function to avoid reflection
    /// </summary>
    private void RegisterHandler<TEvent>(BaseEventHandler<TEvent> handler) where TEvent : BattleEvent
    {
        _handlers[typeof(TEvent)] = (battleEvent, battleState) => 
            handler.HandleAsync((TEvent)battleEvent, battleState);
    }

    /// <summary>
    /// Dispatches an event to its registered handler
    /// </summary>
    public async Task DispatchAsync<T>(T battleEvent, BattleState battleState) where T : BattleEvent
    {
        var eventType = battleEvent.GetType();
        
        if (_handlers.TryGetValue(eventType, out var handler))
        {
            await handler(battleEvent, battleState);
        }
        else
        {
            throw new InvalidOperationException($"No handler registered for event type {eventType.Name}");
        }
    }
}
