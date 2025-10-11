using GreatVoidBattle.Application.Events;
using GreatVoidBattle.Application.Events.Base;
using GreatVoidBattle.Application.Managers;
using GreatVoidBattle.Core.Domains;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreatVoidBattle.UnitTests.Events;

public class AddFractionEventHandlerTests
{
    private BattleManager _battleManager;

    public AddFractionEventHandlerTests()
    {
        var battleEvent = new CreateBattleEvent { Name = "Test Battle" };
        var battleState = BattleState.CreateNew(battleEvent.Name);
        _battleManager = new BattleManager(battleState);
    }

    [Fact]
    public async Task AddFractionEventHandler_AddOne_Success()
    {
        var battleEvent = new AddFractionEvent
        {
            BattleId = _battleManager.BattleId,
            Name = "Test Fraction"
        };

        await _battleManager.ApplyEventAsync(battleEvent);

        _battleManager.BattleState.Fractions.Count.ShouldBe(1);
        var fraction = _battleManager.BattleState.Fractions.FirstOrDefault(f => f.FractionName == battleEvent.Name);
        fraction.ShouldNotBeNull();
        fraction.FractionId.ShouldNotBe(Guid.Empty);
        fraction.FractionName.ShouldBe(battleEvent.Name);
        fraction.IsDefeated.ShouldBe(true);
        fraction.Ships.Count.ShouldBe(0);
    }

    [Fact]
    public async Task AddFractionEventHandler_AddTwo_Success()
    {
        var battleEvent1 = new AddFractionEvent
        {
            BattleId = _battleManager.BattleId,
            Name = "Test Fraction 1"
        };

        var battleEvent2 = new AddFractionEvent
        {
            BattleId = _battleManager.BattleId,
            Name = "Test Fraction 2"
        };

        await _battleManager.ApplyEventAsync(battleEvent1);
        await _battleManager.ApplyEventAsync(battleEvent2);

        _battleManager.BattleState.Fractions.Count.ShouldBe(2);
        var fraction1 = _battleManager.BattleState.Fractions.FirstOrDefault(f => f.FractionName == battleEvent1.Name);
        fraction1.ShouldNotBeNull();
        fraction1.FractionId.ShouldNotBe(Guid.Empty);
        fraction1.FractionName.ShouldBe(battleEvent1.Name);
        fraction1.IsDefeated.ShouldBe(true);
        fraction1.Ships.Count.ShouldBe(0);

        var fraction2 = _battleManager.BattleState.Fractions.FirstOrDefault(f => f.FractionName == battleEvent2.Name);
        fraction2.ShouldNotBeNull();
        fraction2.FractionId.ShouldNotBe(Guid.Empty);
        fraction2.FractionName.ShouldBe(battleEvent2.Name);
        fraction2.IsDefeated.ShouldBe(true);
        fraction2.Ships.Count.ShouldBe(0);
    }
}