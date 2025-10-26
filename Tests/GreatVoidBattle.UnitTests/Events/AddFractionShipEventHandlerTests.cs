using GreatVoidBattle.Application.Events;
using GreatVoidBattle.Application.Managers;
using GreatVoidBattle.Core.Domains;
using GreatVoidBattle.Core.Domains.Enums;
using GreatVoidBattle.Events;
using Shouldly;

namespace GreatVoidBattle.UnitTests.Events;

public class AddFractionShipEventHandlerTests
{
    private readonly BattleManager _battleManager;
    private readonly Guid _fractionId;

    public AddFractionShipEventHandlerTests()
    {
        var battleEvent = new CreateBattleEvent { Name = "Test Battle" };
        var battleState = BattleState.CreateNew(battleEvent.Name);
        var fraction = FractionState.CreateNew("Fraction 1", battleState.BattleLog);
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
        var battleEvent = new CreateBattleEvent { Name = "Test Battle" };
        var battleState = BattleState.CreateNew(battleEvent.Name);

        var fraction1 = FractionState.CreateNew("Fraction 1", battleState.BattleLog);
        var fraction2 = FractionState.CreateNew("Fraction 2", battleState.BattleLog);
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
}