using GreatVoidBattle.Application.Events.Handler.Base;
using GreatVoidBattle.Core.Domains;

namespace GreatVoidBattle.Application.Events.Handler;

public class UpdateFractionEventHandler : BasePreparationEventHandler<UpdateFractionEvent>
{
    public override Task HandlePreparationEventAsync(UpdateFractionEvent battleEvent, BattleState battleState)
    {
        var fraction = battleState.Fractions.FirstOrDefault(f => f.FractionId == battleEvent.FractionId);
        if (fraction == null)
        {
            throw new InvalidOperationException($"Fraction with ID {battleEvent.FractionId} not found.");
        }

        // Aktualizuj tylko podstawowe dane, nie zmieniamy AuthToken
        fraction.FractionName = battleEvent.Name;
        fraction.PlayerName = battleEvent.PlayerName;
        fraction.FractionColor = battleEvent.FractionColor;

        return Task.CompletedTask;
    }
}
