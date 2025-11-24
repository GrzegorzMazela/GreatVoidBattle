using GreatVoidBattle.Application.Events;
using GreatVoidBattle.Application.Managers;
using GreatVoidBattle.Core.Domains;
using Shouldly;

namespace GreatVoidBattle.UnitTests.Events;

public class CreateBattleEventTests
{
    [Fact]
    public void CreateBattleEvent_Success()
    {
        var battleEvent = new CreateBattleEvent { Name = "Test Battle" };

        var battleState = BattleState.CreateNew(battleEvent.Name, 500, 500);
        var battleManager = new BattleManager(battleState);

        battleManager.BattleId.ShouldNotBe(Guid.Empty);
        battleManager.BattleState.BattleId.ShouldBe(battleManager.BattleId);
        battleManager.BattleState.BattleName.ShouldBe(battleEvent.Name);
        battleManager.BattleState.TurnNumber.ShouldBe(0);
        battleManager.BattleState.Fractions.Count.ShouldBe(0);
    }
}