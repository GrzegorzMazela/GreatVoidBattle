using GreatVoidBattle.Application.Mappers;
using GreatVoidBattle.Core.Domains;
using GreatVoidBattle.Core.Domains.Enums;
using Shouldly;

namespace GreatVoidBattle.UnitTests.Mappers;

public class BattleStateMapperTests
{
    private BattleState CreateTestBattleState(string name = "Test Battle", int width = 1000, int height = 800)
    {
        var battleState = BattleState.CreateNew(name, width, height);
        return battleState;
    }

    private void AddTestFraction(BattleState battleState, string name = "Faction 1", string playerName = "Player 1")
    {
        var fraction = FractionState.CreateNew(name, playerName, "#FF0000");
        battleState.AddFraction(fraction);
    }

    private void AddTestShip(BattleState battleState, Guid fractionId, string name = "Test Ship")
    {
        var ship = ShipState.Create(
            fractionId: fractionId,
            name: name,
            type: ShipType.Corvette,
            positionX: 100,
            positionY: 100,
            speed: 10,
            hitPoints: 100,
            shields: 50,
            armor: 25,
            numberOfModules: 1,
            modules: new List<ModuleState>
            {
                ModuleState.Create(new List<SystemSlot> { SystemSlot.Create(WeaponType.Laser) })
            },
            battleLog: battleState.BattleLog
        );
        battleState.AddFractionShip(fractionId, ship);
    }

    [Fact]
    public void ToDto_ShouldMapBasicProperties()
    {
        // Arrange
        var battleState = CreateTestBattleState("Epic Battle", 1200, 900);

        // Act
        var dto = BattleStateMapper.ToDto(battleState);

        // Assert
        dto.BattleId.ShouldBe(battleState.BattleId);
        dto.Name.ShouldBe("Epic Battle");
        dto.Width.ShouldBe(1200);
        dto.Height.ShouldBe(900);
        dto.Status.ShouldBe("Preparation");
        dto.TurnNumber.ShouldBe(0);
    }

    [Fact]
    public void ToDto_ShouldMapFractions()
    {
        // Arrange
        var battleState = CreateTestBattleState();
        AddTestFraction(battleState, "Alpha Faction", "Commander A");
        AddTestFraction(battleState, "Beta Faction", "Commander B");

        // Act
        var dto = BattleStateMapper.ToDto(battleState);

        // Assert
        dto.Fractions.Count.ShouldBe(2);
        dto.Fractions[0].FractionName.ShouldBe("Alpha Faction");
        dto.Fractions[1].FractionName.ShouldBe("Beta Faction");
    }

    [Fact]
    public void ToDto_WithIncludeMovementPaths_ShouldIncludeEmptyPathsInitially()
    {
        // Arrange
        var battleState = CreateTestBattleState();

        // Act
        var dto = BattleStateMapper.ToDto(battleState, includeMovementPaths: true);

        // Assert
        dto.ShipMovementPaths.ShouldNotBeNull();
        dto.MissileMovementPaths.ShouldNotBeNull();
        dto.ShipMovementPaths.Count.ShouldBe(0);
        dto.MissileMovementPaths.Count.ShouldBe(0);
    }

    [Fact]
    public void ToDto_WithoutMovementPaths_ShouldNotIncludePaths()
    {
        // Arrange
        var battleState = CreateTestBattleState();

        // Act
        var dto = BattleStateMapper.ToDto(battleState, includeMovementPaths: false);

        // Assert
        dto.ShipMovementPaths.Count.ShouldBe(0);
        dto.MissileMovementPaths.Count.ShouldBe(0);
    }

    [Fact]
    public void ToAdminDto_ShouldMapBasicProperties()
    {
        // Arrange
        var battleState = CreateTestBattleState("Admin Battle", 1500, 1000);
        battleState.StartBattle();

        // Act
        var dto = BattleStateMapper.ToAdminDto(battleState);

        // Assert
        dto.BattleId.ShouldBe(battleState.BattleId);
        dto.Name.ShouldBe("Admin Battle");
        dto.Width.ShouldBe(1500);
        dto.Height.ShouldBe(1000);
        dto.Status.ShouldBe("InProgress");
        dto.TurnNumber.ShouldBe(1);
    }

    [Fact]
    public void ToAdminDto_ShouldIncludeAuthTokensInFractions()
    {
        // Arrange
        var battleState = CreateTestBattleState();
        AddTestFraction(battleState, "Admin Faction");

        // Act
        var dto = BattleStateMapper.ToAdminDto(battleState);

        // Assert
        dto.Fractions.Count.ShouldBe(1);
        dto.Fractions[0].AuthToken.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void ToDto_ShouldNotIncludeAuthTokensInFractions()
    {
        // Arrange
        var battleState = CreateTestBattleState();
        AddTestFraction(battleState, "Regular Faction");

        // Act
        var dto = BattleStateMapper.ToDto(battleState);

        // Assert
        dto.Fractions.Count.ShouldBe(1);
        // FractionDto doesn't have AuthToken property (only FractionAdminDto does)
        dto.Fractions[0].ShouldBeOfType<Application.Dto.Fractions.FractionDto>();
    }

    [Fact]
    public void ToDto_AfterStartBattle_ShouldShowCorrectStatus()
    {
        // Arrange
        var battleState = CreateTestBattleState();
        battleState.StartBattle();

        // Act
        var dto = BattleStateMapper.ToDto(battleState);

        // Assert
        dto.Status.ShouldBe("InProgress");
        dto.TurnNumber.ShouldBe(1);
    }

    [Fact]
    public void MapShipMovementPaths_ShouldMapPathsCorrectly()
    {
        // Arrange
        var battleState = CreateTestBattleState();
        AddTestFraction(battleState, "Moving Faction");
        var fractionId = battleState.Fractions.First().FractionId;
        AddTestShip(battleState, fractionId, "Moving Ship");
        
        var shipId = battleState.Fractions.First().Ships.First().ShipId;
        battleState.StartBattle();
        battleState.AddShipMove(fractionId, shipId, new Position(500, 500));

        // Act
        var paths = BattleStateMapper.MapShipMovementPaths(battleState.ShipMovementPaths);

        // Assert
        paths.Count.ShouldBe(1);
        paths[0].ShipId.ShouldBe(shipId);
        paths[0].StartPosition.ShouldNotBeNull();
        paths[0].TargetPosition.ShouldNotBeNull();
        paths[0].Path.ShouldNotBeNull();
    }

    [Fact]
    public void MapMissileMovementPaths_ShouldMapPathsCorrectly()
    {
        // Arrange
        var battleState = CreateTestBattleState();
        var fraction1 = FractionState.CreateNew("Attacker", "Player 1", "#FF0000");
        var fraction2 = FractionState.CreateNew("Target", "Player 2", "#00FF00");
        battleState.AddFraction(fraction1);
        battleState.AddFraction(fraction2);

        // Ships must be within missile range (max 55 units)
        var ship1 = ShipState.Create(
            fractionId: fraction1.FractionId,
            name: "Attacker Ship",
            type: ShipType.Corvette,
            positionX: 100, positionY: 100,
            speed: 10, hitPoints: 100, shields: 50, armor: 25,
            numberOfModules: 1,
            modules: new List<ModuleState>
            {
                ModuleState.Create(new List<SystemSlot> { SystemSlot.Create(WeaponType.Missile) })
            },
            battleLog: battleState.BattleLog
        );

        // Target is 30 units away (within range of 55)
        var ship2 = ShipState.Create(
            fractionId: fraction2.FractionId,
            name: "Target Ship",
            type: ShipType.Corvette,
            positionX: 120, positionY: 120,
            speed: 10, hitPoints: 100, shields: 50, armor: 25,
            numberOfModules: 1,
            modules: new List<ModuleState>
            {
                ModuleState.Create(new List<SystemSlot> { SystemSlot.Create(WeaponType.Laser) })
            },
            battleLog: battleState.BattleLog
        );

        battleState.AddFractionShip(fraction1.FractionId, ship1);
        battleState.AddFractionShip(fraction2.FractionId, ship2);
        battleState.StartBattle();
        battleState.AddMissileShot(fraction1.FractionId, ship1.ShipId, fraction2.FractionId, ship2.ShipId);

        // Act
        var paths = BattleStateMapper.MapMissileMovementPaths(battleState.MissileMovementPaths);

        // Assert
        paths.Count.ShouldBe(1);
        paths[0].ShipId.ShouldBe(ship1.ShipId);
        paths[0].TargetId.ShouldBe(ship2.ShipId);
        paths[0].StartPosition.ShouldNotBeNull();
        paths[0].TargetPosition.ShouldNotBeNull();
    }
}

