using GreatVoidBattle.Application.Events;
using GreatVoidBattle.Core.Domains;
using GreatVoidBattle.Core.Domains.Enums;
using GreatVoidBattle.Core.Factories;
using Shouldly;

namespace GreatVoidBattle.UnitTests.Factories;

public class ShipFactoryTests
{
    private readonly BattleLog _battleLog = new();
    private readonly Guid _fractionId = Guid.NewGuid();

    private AddFractionShipEvent CreateAddShipEvent(
        ShipType type,
        string name = "Test Ship",
        int? speed = null,
        int? hitPoints = null,
        int? shields = null,
        int? armor = null)
    {
        var moduleCount = type switch
        {
            ShipType.Corvette => 1,
            ShipType.Destroyer => 2,
            ShipType.Cruiser => 4,
            ShipType.Battleship => 8,
            ShipType.SuperBattleship => 12,
            ShipType.OrbitalFort => 2,
            ShipType.Transport => 0,
            _ => 1
        };

        return new AddFractionShipEvent
        {
            BattleId = Guid.NewGuid(),
            FractionId = _fractionId,
            Name = name,
            Type = type,
            PositionX = 100,
            PositionY = 200,
            Speed = speed,
            HitPoints = hitPoints,
            Shields = shields,
            Armor = armor,
            Modules = Enumerable.Range(0, moduleCount)
                .Select(_ => new Module(new List<WeaponType> 
                { 
                    WeaponType.Laser, 
                    WeaponType.Missile, 
                    WeaponType.PointDefense 
                }))
                .ToList()
        };
    }

    #region GetDefaultStats Tests

    [Theory]
    [InlineData(ShipType.Corvette, 10, 50, 25, 25, 1)]
    [InlineData(ShipType.Destroyer, 8, 100, 50, 50, 2)]
    [InlineData(ShipType.Cruiser, 6, 200, 100, 100, 4)]
    [InlineData(ShipType.Battleship, 5, 400, 200, 200, 8)]
    [InlineData(ShipType.SuperBattleship, 5, 600, 300, 300, 12)]
    [InlineData(ShipType.OrbitalFort, 0, 100, 50, 50, 2)]
    [InlineData(ShipType.Transport, 6, 200, 0, 100, 0)]
    public void GetDefaultStats_ShouldReturnCorrectValuesForEachType(
        ShipType type, 
        int expectedSpeed, 
        int expectedHp, 
        int expectedShields, 
        int expectedArmor,
        int expectedModules)
    {
        // Act
        var (speed, hitPoints, shields, armor, modules) = ShipFactory.GetDefaultStats(type);

        // Assert
        speed.ShouldBe(expectedSpeed);
        hitPoints.ShouldBe(expectedHp);
        shields.ShouldBe(expectedShields);
        armor.ShouldBe(expectedArmor);
        modules.ShouldBe(expectedModules);
    }

    #endregion

    #region CreateShip with Default Stats Tests

    [Fact]
    public void CreateShip_Corvette_WithoutCustomStats_ShouldUseDefaults()
    {
        // Arrange
        var addShipEvent = CreateAddShipEvent(ShipType.Corvette);

        // Act
        var ship = ShipFactory.CreateShip(addShipEvent, _battleLog);

        // Assert
        ship.Speed.ShouldBe(10);
        ship.HitPoints.ShouldBe(50);
        ship.Shields.ShouldBe(25);
        ship.Armor.ShouldBe(25);
    }

    [Fact]
    public void CreateShip_Destroyer_WithoutCustomStats_ShouldUseDefaults()
    {
        // Arrange
        var addShipEvent = CreateAddShipEvent(ShipType.Destroyer);

        // Act
        var ship = ShipFactory.CreateShip(addShipEvent, _battleLog);

        // Assert
        ship.Speed.ShouldBe(8);
        ship.HitPoints.ShouldBe(100);
        ship.Shields.ShouldBe(50);
        ship.Armor.ShouldBe(50);
    }

    [Fact]
    public void CreateShip_Battleship_WithoutCustomStats_ShouldUseDefaults()
    {
        // Arrange
        var addShipEvent = CreateAddShipEvent(ShipType.Battleship);

        // Act
        var ship = ShipFactory.CreateShip(addShipEvent, _battleLog);

        // Assert
        ship.Speed.ShouldBe(5);
        ship.HitPoints.ShouldBe(400);
        ship.Shields.ShouldBe(200);
        ship.Armor.ShouldBe(200);
    }

    #endregion

    #region CreateShip with Custom Stats Tests

    [Fact]
    public void CreateShip_WithCustomSpeed_ShouldUseCustomValue()
    {
        // Arrange
        var addShipEvent = CreateAddShipEvent(ShipType.Corvette, speed: 20);

        // Act
        var ship = ShipFactory.CreateShip(addShipEvent, _battleLog);

        // Assert
        ship.Speed.ShouldBe(20);
        ship.HitPoints.ShouldBe(50); // Default
        ship.Shields.ShouldBe(25); // Default
        ship.Armor.ShouldBe(25); // Default
    }

    [Fact]
    public void CreateShip_WithCustomHitPoints_ShouldUseCustomValue()
    {
        // Arrange
        var addShipEvent = CreateAddShipEvent(ShipType.Corvette, hitPoints: 150);

        // Act
        var ship = ShipFactory.CreateShip(addShipEvent, _battleLog);

        // Assert
        ship.Speed.ShouldBe(10); // Default
        ship.HitPoints.ShouldBe(150);
        ship.Shields.ShouldBe(25); // Default
        ship.Armor.ShouldBe(25); // Default
    }

    [Fact]
    public void CreateShip_WithCustomShields_ShouldUseCustomValue()
    {
        // Arrange
        var addShipEvent = CreateAddShipEvent(ShipType.Destroyer, shields: 200);

        // Act
        var ship = ShipFactory.CreateShip(addShipEvent, _battleLog);

        // Assert
        ship.Speed.ShouldBe(8); // Default
        ship.HitPoints.ShouldBe(100); // Default
        ship.Shields.ShouldBe(200);
        ship.Armor.ShouldBe(50); // Default
    }

    [Fact]
    public void CreateShip_WithCustomArmor_ShouldUseCustomValue()
    {
        // Arrange
        var addShipEvent = CreateAddShipEvent(ShipType.Cruiser, armor: 250);

        // Act
        var ship = ShipFactory.CreateShip(addShipEvent, _battleLog);

        // Assert
        ship.Speed.ShouldBe(6); // Default
        ship.HitPoints.ShouldBe(200); // Default
        ship.Shields.ShouldBe(100); // Default
        ship.Armor.ShouldBe(250);
    }

    [Fact]
    public void CreateShip_WithAllCustomStats_ShouldUseAllCustomValues()
    {
        // Arrange
        var addShipEvent = CreateAddShipEvent(
            ShipType.Battleship,
            speed: 3,
            hitPoints: 1000,
            shields: 600,
            armor: 500);

        // Act
        var ship = ShipFactory.CreateShip(addShipEvent, _battleLog);

        // Assert
        ship.Speed.ShouldBe(3);
        ship.HitPoints.ShouldBe(1000);
        ship.Shields.ShouldBe(600);
        ship.Armor.ShouldBe(500);
    }

    [Fact]
    public void CreateShip_WithZeroSpeed_ShouldAllowZeroSpeed()
    {
        // Arrange - OrbitalFort has 0 speed by default, but we test explicitly setting it
        var addShipEvent = CreateAddShipEvent(ShipType.Corvette, speed: 0);

        // Act
        var ship = ShipFactory.CreateShip(addShipEvent, _battleLog);

        // Assert
        ship.Speed.ShouldBe(0);
    }

    #endregion

    #region Basic Ship Properties Tests

    [Fact]
    public void CreateShip_ShouldSetNameCorrectly()
    {
        // Arrange
        var addShipEvent = CreateAddShipEvent(ShipType.Corvette, name: "ISS Enterprise");

        // Act
        var ship = ShipFactory.CreateShip(addShipEvent, _battleLog);

        // Assert
        ship.Name.ShouldBe("ISS Enterprise");
    }

    [Fact]
    public void CreateShip_ShouldSetTypeCorrectly()
    {
        // Arrange
        var addShipEvent = CreateAddShipEvent(ShipType.SuperBattleship);

        // Act
        var ship = ShipFactory.CreateShip(addShipEvent, _battleLog);

        // Assert
        ship.Type.ShouldBe(ShipType.SuperBattleship);
    }

    [Fact]
    public void CreateShip_ShouldSetPositionCorrectly()
    {
        // Arrange
        var addShipEvent = CreateAddShipEvent(ShipType.Corvette);

        // Act
        var ship = ShipFactory.CreateShip(addShipEvent, _battleLog);

        // Assert
        ship.Position.X.ShouldBe(100);
        ship.Position.Y.ShouldBe(200);
    }

    [Fact]
    public void CreateShip_ShouldSetFractionIdCorrectly()
    {
        // Arrange
        var addShipEvent = CreateAddShipEvent(ShipType.Corvette);

        // Act
        var ship = ShipFactory.CreateShip(addShipEvent, _battleLog);

        // Assert
        ship.FractionId.ShouldBe(_fractionId);
    }

    [Fact]
    public void CreateShip_ShouldGenerateUniqueShipId()
    {
        // Arrange
        var addShipEvent1 = CreateAddShipEvent(ShipType.Corvette, name: "Ship 1");
        var addShipEvent2 = CreateAddShipEvent(ShipType.Corvette, name: "Ship 2");

        // Act
        var ship1 = ShipFactory.CreateShip(addShipEvent1, _battleLog);
        var ship2 = ShipFactory.CreateShip(addShipEvent2, _battleLog);

        // Assert
        ship1.ShipId.ShouldNotBe(Guid.Empty);
        ship2.ShipId.ShouldNotBe(Guid.Empty);
        ship1.ShipId.ShouldNotBe(ship2.ShipId);
    }

    #endregion

    #region Modules Tests

    [Fact]
    public void CreateShip_ShouldCreateModulesFromEvent()
    {
        // Arrange
        var addShipEvent = CreateAddShipEvent(ShipType.Destroyer);

        // Act
        var ship = ShipFactory.CreateShip(addShipEvent, _battleLog);

        // Assert
        ship.Modules.Count.ShouldBe(2);
    }

    [Fact]
    public void CreateShip_ModulesShouldHaveCorrectWeapons()
    {
        // Arrange
        var addShipEvent = new AddFractionShipEvent
        {
            BattleId = Guid.NewGuid(),
            FractionId = _fractionId,
            Name = "Test Ship",
            Type = ShipType.Destroyer,
            PositionX = 0,
            PositionY = 0,
            Modules = new List<Module>
            {
                new Module(new List<WeaponType> { WeaponType.Laser, WeaponType.Laser, WeaponType.Laser }),
                new Module(new List<WeaponType> { WeaponType.Missile, WeaponType.Missile, WeaponType.PointDefense })
            }
        };

        // Act
        var ship = ShipFactory.CreateShip(addShipEvent, _battleLog);

        // Assert
        ship.NumberOfLasers.ShouldBe(3);
        ship.NumberOfMissiles.ShouldBe(2);
        ship.NumberOfPointsDefense.ShouldBe(1);
    }

    #endregion
}

