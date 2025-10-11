using GreatVoidBattle.Application.Events.Base;
using GreatVoidBattle.Application.Events.Handler;
using GreatVoidBattle.Application.Events.Handler.Base;
using GreatVoidBattle.Core.Domains;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GreatVoidBattle.Application.Factories;

public class EventDispatcher
{
    private readonly Dictionary<Type, object> _handlers = new();

    public EventDispatcher()
    {
        // Register all event handlers here
        RegisterHandler(new AddFractionEventHandler());
        RegisterHandler(new AddFractionShipEventHandler());
        // Add more handlers as needed
    }

    private void RegisterHandler<TEvent>(BaseEventHandler<TEvent> handler) where TEvent : BattleEvent
    {
        _handlers[typeof(TEvent)] = handler;
    }

    public async Task DispatchAsync<T>(T battleEvent, BattleState battleState) where T : BattleEvent
    {
        var targetEvent = battleEvent.GetType();
        if (_handlers.TryGetValue(targetEvent, out var handlerObj))
        {
            await ((BaseEventHandler<T>)handlerObj).HandleAsync(battleEvent, battleState);
        }
        else
        {
            throw new InvalidOperationException($"No handler registered for event type {targetEvent.Name}");
        }
    }
}