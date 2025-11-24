using GreatVoidBattle.Application.Events.Base;
using GreatVoidBattle.Application.Events.Handler;
using GreatVoidBattle.Application.Events.Handler.Base;
using GreatVoidBattle.Application.Events.InProgress.Handler;
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
        RegisterHandler(new UpdateFractionEventHandler());
        RegisterHandler(new AddFractionShipEventHandler());
        RegisterHandler(new UpdateFractionShipEventHandler());
        RegisterHandler(new StartBattleEventHandler());
        RegisterHandler(new SetShipPositionEventHandler());
        RegisterHandler(new AddLaserShotEventHandler());
        RegisterHandler(new AddMissileShotEventHandler());
        RegisterHandler(new AddShipMoveEventHandler());
        RegisterHandler(new EndOfTurnEventHandler());
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
            // Znajdź metodę HandleAsync za pomocą refleksji
            var method = handlerObj.GetType().GetMethod("HandleAsync");
            if (method == null)
                throw new InvalidOperationException($"Handler for event type {targetEvent.Name} does not implement HandleAsync.");

            // Wywołaj metodę asynchronicznie
            var task = (Task?)method.Invoke(handlerObj, new object[] { battleEvent, battleState });
            if (task != null)
                await task;
        }
        else
        {
            throw new InvalidOperationException($"No handler registered for event type {targetEvent.Name}");
        }
    }
}