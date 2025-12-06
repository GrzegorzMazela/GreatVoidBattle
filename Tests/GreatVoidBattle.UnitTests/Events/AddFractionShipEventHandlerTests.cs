using GreatVoidBattle.Application.Events;
using GreatVoidBattle.Application.Managers;
using GreatVoidBattle.Core.Domains;
using GreatVoidBattle.Core.Domains.Enums;
using Shouldly;

namespace GreatVoidBattle.UnitTests.Events;

public class AddFractionShipEventHandlerTests
{
    private readonly BattleManager _battleManager;
    private readonly Guid _fractionId;

    public AddFractionShipEventHandlerTests()
    {
        var battleEvent = new CreateBattleEvent { Name = "Test Battle", Width = 500, Height = 500 };
        var battleState = BattleState.CreateNew(battleEvent.Name, battleEvent.Width, battleEvent.Height);
        var fraction = FractionState.CreateNew("Fraction 1", "Player 1", "#FF0000");
        battleState.AddFraction(fraction);
        _fractionId = fraction.FractionId;
        _battleManager = new BattleManager(battleState);
    }

    [Fact]
    public async Task AddFractionShipEventHandler_AddCorvette_Success()
    {
        var addShipEvent = new AddFractionShipEvent
        {
            BattleId = _battleManager.BattleId,
            FractionId = _fractionId,
            Name = "Test Corvette",
            Type = ShipType.Corvette,
            PositionX = 0,
            PositionY = 0,
            Modules = [
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense })
                ]
        };

        await _battleManager.ApplyEventAsync(addShipEvent);

        var ship = _battleManager.BattleState.Fractions
            .First(f => f.FractionId == _fractionId)
            .Ships.FirstOrDefault(s => s.Name == addShipEvent.Name && s.Type == ShipType.Corvette);

        ship.ShouldNotBeNull();
        ship.Name.ShouldBe(addShipEvent.Name);
        ship.Type.ShouldBe(ShipType.Corvette);
    }

    [Fact]
    public async Task AddFractionShipEventHandler_AddDestroyer_Success()
    {
        var addShipEvent = new AddFractionShipEvent
        {
            BattleId = _battleManager.BattleId,
            FractionId = _fractionId,
            Name = "Test Destroyer",
            Type = ShipType.Destroyer,
            PositionX = 0,
            PositionY = 0,
            Modules = [
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense }),
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense })
                ]
        };

        await _battleManager.ApplyEventAsync(addShipEvent);

        var ship = _battleManager.BattleState.Fractions
            .First(f => f.FractionId == _fractionId)
            .Ships.FirstOrDefault(s => s.Name == addShipEvent.Name && s.Type == ShipType.Destroyer);

        ship.ShouldNotBeNull();
        ship.Name.ShouldBe(addShipEvent.Name);
        ship.Type.ShouldBe(ShipType.Destroyer);
    }

    [Fact]
    public async Task AddFractionShipEventHandler_AddCruiser_Success()
    {
        var addShipEvent = new AddFractionShipEvent
        {
            BattleId = _battleManager.BattleId,
            FractionId = _fractionId,
            Name = "Test Cruiser",
            Type = ShipType.Cruiser,
            PositionX = 0,
            PositionY = 0,
            Modules = [
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense }),
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense }),
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense }),
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense })
                ]
        };

        await _battleManager.ApplyEventAsync(addShipEvent);

        var ship = _battleManager.BattleState.Fractions
            .First(f => f.FractionId == _fractionId)
            .Ships.FirstOrDefault(s => s.Name == addShipEvent.Name && s.Type == ShipType.Cruiser);

        ship.ShouldNotBeNull();
        ship.Name.ShouldBe(addShipEvent.Name);
        ship.Type.ShouldBe(ShipType.Cruiser);
    }

    [Fact]
    public async Task AddFractionShipEventHandler_AddBattleship_Success()
    {
        var addShipEvent = new AddFractionShipEvent
        {
            BattleId = _battleManager.BattleId,
            FractionId = _fractionId,
            Name = "Test Battleship",
            Type = ShipType.Battleship,
            PositionX = 0,
            PositionY = 0,
            Modules = [
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense }),
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense }),
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense }),
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense }),
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense }),
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense }),
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense }),
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense })
                ]
        };

        await _battleManager.ApplyEventAsync(addShipEvent);

        var ship = _battleManager.BattleState.Fractions
            .First(f => f.FractionId == _fractionId)
            .Ships.FirstOrDefault(s => s.Name == addShipEvent.Name && s.Type == ShipType.Battleship);

        ship.ShouldNotBeNull();
        ship.Name.ShouldBe(addShipEvent.Name);
        ship.Type.ShouldBe(ShipType.Battleship);
    }

    [Fact]
    public async Task AddFractionShipEventHandler_AddBattleship_NumeberOfLasersMissilesPointsDefense_Success()
    {
        var addShipEvent = new AddFractionShipEvent
        {
            BattleId = _battleManager.BattleId,
            FractionId = _fractionId,
            Name = "Test Battleship",
            Type = ShipType.Battleship,
            PositionX = 0,
            PositionY = 0,
            Modules = [
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Laser, WeaponType.Laser }),
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense }),
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense }),
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense }),
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense }),
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.Missile }),
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.Missile }),
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.Missile })
                ]
        };

        await _battleManager.ApplyEventAsync(addShipEvent);

        var ship = _battleManager.BattleState.Fractions
            .First(f => f.FractionId == _fractionId)
            .Ships.FirstOrDefault(s => s.Name == addShipEvent.Name && s.Type == ShipType.Battleship);

        ship.ShouldNotBeNull();
        ship.Name.ShouldBe(addShipEvent.Name);
        ship.Type.ShouldBe(ShipType.Battleship);
        ship.NumberOfLasers.ShouldBe(10);
        ship.NumberOfMissiles.ShouldBe(10);
        ship.NumberOfPointsDefense.ShouldBe(4);
    }

    [Fact]
    public async Task AddFractionShipEventHandler_AddSuperBattleship_Success()
    {
        var addShipEvent = new AddFractionShipEvent
        {
            BattleId = _battleManager.BattleId,
            FractionId = _fractionId,
            Name = "Test SuperBattleship",
            Type = ShipType.SuperBattleship,
            PositionX = 0,
            PositionY = 0,
            Modules = [
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense }),
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense }),
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense }),
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense }),
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense }),
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense }),
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense }),
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense }),
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense }),
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense }),
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense }),
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense })
                ]
        };

        await _battleManager.ApplyEventAsync(addShipEvent);

        var ship = _battleManager.BattleState.Fractions
            .First(f => f.FractionId == _fractionId)
            .Ships.FirstOrDefault(s => s.Name == addShipEvent.Name && s.Type == ShipType.SuperBattleship);

        ship.ShouldNotBeNull();
        ship.Name.ShouldBe(addShipEvent.Name);
        ship.Type.ShouldBe(ShipType.SuperBattleship);
    }

    [Fact]
    public async Task AddFractionShipEventHandler_AddOrbitalFort_Success()
    {
        var addShipEvent = new AddFractionShipEvent
        {
            BattleId = _battleManager.BattleId,
            FractionId = _fractionId,
            Name = "Test OrbitalFort",
            Type = ShipType.OrbitalFort,
            PositionX = 0,
            PositionY = 0,
            Modules = [
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense }),
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense })
                ]
        };

        await _battleManager.ApplyEventAsync(addShipEvent);

        var ship = _battleManager.BattleState.Fractions
            .First(f => f.FractionId == _fractionId)
            .Ships.FirstOrDefault(s => s.Name == addShipEvent.Name && s.Type == ShipType.OrbitalFort);

        ship.ShouldNotBeNull();
        ship.Name.ShouldBe(addShipEvent.Name);
        ship.Type.ShouldBe(ShipType.OrbitalFort);
    }

    [Fact]
    public async Task AddFractionShipEventHandler_AddShipsToTwoFractions_Success()
    {
        // Arrange: create battle with two fractions
        var battleEvent = new CreateBattleEvent { Name = "Test Battle", Width = 500, Height = 500 };
        var battleState = BattleState.CreateNew(battleEvent.Name, battleEvent.Width, battleEvent.Height);

        var fraction1 = FractionState.CreateNew("Fraction 1", "Player 1", "#FF0000");
        var fraction2 = FractionState.CreateNew("Fraction 2", "Player 2", "#00FF00");
        battleState.AddFraction(fraction1);
        battleState.AddFraction(fraction2);

        var battleManager = new BattleManager(battleState);

        // Act: add a Corvette to fraction1 and a Battleship to fraction2
        var addCorvetteEvent = new AddFractionShipEvent
        {
            BattleId = battleManager.BattleId,
            FractionId = fraction1.FractionId,
            Name = "F1 Corvette",
            Type = ShipType.Corvette,
            PositionX = 1,
            PositionY = 1,
            Modules = [
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense })
                ]
        };
        var addBattleshipEvent = new AddFractionShipEvent
        {
            BattleId = battleManager.BattleId,
            FractionId = fraction2.FractionId,
            Name = "F2 Battleship",
            Type = ShipType.Battleship,
            PositionX = 2,
            PositionY = 2,
            Modules = [
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense }),
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense }),
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense }),
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense }),
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense }),
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense }),
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense }),
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense })
                ]
        };

        await battleManager.ApplyEventAsync(addCorvetteEvent);
        await battleManager.ApplyEventAsync(addBattleshipEvent);

        // Assert: each fraction has the correct ship
        var f1 = battleManager.BattleState.Fractions.First(f => f.FractionId == fraction1.FractionId);
        var f2 = battleManager.BattleState.Fractions.First(f => f.FractionId == fraction2.FractionId);

        f1.Ships.Count.ShouldBe(1);
        f1.Ships[0].Name.ShouldBe("F1 Corvette");
        f1.Ships[0].Type.ShouldBe(ShipType.Corvette);

        f2.Ships.Count.ShouldBe(1);
        f2.Ships[0].Name.ShouldBe("F2 Battleship");
        f2.Ships[0].Type.ShouldBe(ShipType.Battleship);
    }

    #region Custom Stats Tests

    [Fact]
    public async Task AddFractionShipEventHandler_WithCustomSpeed_ShouldUseCustomValue()
    {
        // Arrange
        var addShipEvent = new AddFractionShipEvent
        {
            BattleId = _battleManager.BattleId,
            FractionId = _fractionId,
            Name = "Fast Corvette",
            Type = ShipType.Corvette,
            PositionX = 0,
            PositionY = 0,
            Speed = 15, // Custom speed (default is 10)
            Modules = [
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense })
            ]
        };

        // Act
        await _battleManager.ApplyEventAsync(addShipEvent);

        // Assert
        var ship = _battleManager.BattleState.Fractions
            .First(f => f.FractionId == _fractionId)
            .Ships.First(s => s.Name == "Fast Corvette");

        ship.Speed.ShouldBe(15);
    }

    [Fact]
    public async Task AddFractionShipEventHandler_WithCustomHitPoints_ShouldUseCustomValue()
    {
        // Arrange
        var addShipEvent = new AddFractionShipEvent
        {
            BattleId = _battleManager.BattleId,
            FractionId = _fractionId,
            Name = "Tough Corvette",
            Type = ShipType.Corvette,
            PositionX = 0,
            PositionY = 0,
            HitPoints = 100, // Custom HP (default is 50)
            Modules = [
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense })
            ]
        };

        // Act
        await _battleManager.ApplyEventAsync(addShipEvent);

        // Assert
        var ship = _battleManager.BattleState.Fractions
            .First(f => f.FractionId == _fractionId)
            .Ships.First(s => s.Name == "Tough Corvette");

        ship.HitPoints.ShouldBe(100);
    }

    [Fact]
    public async Task AddFractionShipEventHandler_WithCustomShields_ShouldUseCustomValue()
    {
        // Arrange
        var addShipEvent = new AddFractionShipEvent
        {
            BattleId = _battleManager.BattleId,
            FractionId = _fractionId,
            Name = "Shielded Destroyer",
            Type = ShipType.Destroyer,
            PositionX = 0,
            PositionY = 0,
            Shields = 150, // Custom shields (default is 50)
            Modules = [
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense }),
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense })
            ]
        };

        // Act
        await _battleManager.ApplyEventAsync(addShipEvent);

        // Assert
        var ship = _battleManager.BattleState.Fractions
            .First(f => f.FractionId == _fractionId)
            .Ships.First(s => s.Name == "Shielded Destroyer");

        ship.Shields.ShouldBe(150);
    }

    [Fact]
    public async Task AddFractionShipEventHandler_WithCustomArmor_ShouldUseCustomValue()
    {
        // Arrange
        var addShipEvent = new AddFractionShipEvent
        {
            BattleId = _battleManager.BattleId,
            FractionId = _fractionId,
            Name = "Armored Cruiser",
            Type = ShipType.Cruiser,
            PositionX = 0,
            PositionY = 0,
            Armor = 300, // Custom armor (default is 100)
            Modules = [
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense }),
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense }),
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense }),
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense })
            ]
        };

        // Act
        await _battleManager.ApplyEventAsync(addShipEvent);

        // Assert
        var ship = _battleManager.BattleState.Fractions
            .First(f => f.FractionId == _fractionId)
            .Ships.First(s => s.Name == "Armored Cruiser");

        ship.Armor.ShouldBe(300);
    }

    [Fact]
    public async Task AddFractionShipEventHandler_WithAllCustomStats_ShouldUseAllCustomValues()
    {
        // Arrange
        var addShipEvent = new AddFractionShipEvent
        {
            BattleId = _battleManager.BattleId,
            FractionId = _fractionId,
            Name = "Custom Battleship",
            Type = ShipType.Battleship,
            PositionX = 0,
            PositionY = 0,
            Speed = 3,
            HitPoints = 800,
            Shields = 500,
            Armor = 400,
            Modules = [
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense }),
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense }),
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense }),
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense }),
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense }),
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense }),
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense }),
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense })
            ]
        };

        // Act
        await _battleManager.ApplyEventAsync(addShipEvent);

        // Assert
        var ship = _battleManager.BattleState.Fractions
            .First(f => f.FractionId == _fractionId)
            .Ships.First(s => s.Name == "Custom Battleship");

        ship.Speed.ShouldBe(3);
        ship.HitPoints.ShouldBe(800);
        ship.Shields.ShouldBe(500);
        ship.Armor.ShouldBe(400);
    }

    [Fact]
    public async Task AddFractionShipEventHandler_WithoutCustomStats_ShouldUseDefaultValues()
    {
        // Arrange - no custom stats set
        var addShipEvent = new AddFractionShipEvent
        {
            BattleId = _battleManager.BattleId,
            FractionId = _fractionId,
            Name = "Default Corvette",
            Type = ShipType.Corvette,
            PositionX = 0,
            PositionY = 0,
            Modules = [
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense })
            ]
        };

        // Act
        await _battleManager.ApplyEventAsync(addShipEvent);

        // Assert - default Corvette stats
        var ship = _battleManager.BattleState.Fractions
            .First(f => f.FractionId == _fractionId)
            .Ships.First(s => s.Name == "Default Corvette");

        ship.Speed.ShouldBe(10);
        ship.HitPoints.ShouldBe(50);
        ship.Shields.ShouldBe(25);
        ship.Armor.ShouldBe(25);
    }

    [Fact]
    public async Task AddFractionShipEventHandler_WithPartialCustomStats_ShouldMixCustomAndDefaults()
    {
        // Arrange - only speed and shields are custom
        var addShipEvent = new AddFractionShipEvent
        {
            BattleId = _battleManager.BattleId,
            FractionId = _fractionId,
            Name = "Partial Custom Destroyer",
            Type = ShipType.Destroyer,
            PositionX = 0,
            PositionY = 0,
            Speed = 12, // Custom
            Shields = 100, // Custom
            // HitPoints and Armor are not set, should use defaults
            Modules = [
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense }),
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Missile, WeaponType.PointDefense })
            ]
        };

        // Act
        await _battleManager.ApplyEventAsync(addShipEvent);

        // Assert
        var ship = _battleManager.BattleState.Fractions
            .First(f => f.FractionId == _fractionId)
            .Ships.First(s => s.Name == "Partial Custom Destroyer");

        ship.Speed.ShouldBe(12); // Custom
        ship.Shields.ShouldBe(100); // Custom
        ship.HitPoints.ShouldBe(100); // Default for Destroyer
        ship.Armor.ShouldBe(50); // Default for Destroyer
    }

    #endregion
}
