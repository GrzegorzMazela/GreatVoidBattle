using GreatVoidBattle.Application.Mappers;
using GreatVoidBattle.Core.Domains;
using GreatVoidBattle.Core.Domains.Enums;
using Shouldly;

namespace GreatVoidBattle.UnitTests.Mappers;

public class ShipMapperTests
{
    private readonly BattleLog _battleLog = new();

    private ShipState CreateTestShip(
        string name = "Test Ship",
        ShipType type = ShipType.Corvette,
        double positionX = 100,
        double positionY = 200)
    {
        return ShipState.Create(
            fractionId: Guid.NewGuid(),
            name: name,
            type: type,
            positionX: positionX,
            positionY: positionY,
            speed: 10,
            hitPoints: 100,
            shields: 50,
            armor: 25,
            numberOfModules: 2,
            modules: new List<ModuleState>
            {
                ModuleState.Create(new List<SystemSlot> { SystemSlot.Create(WeaponType.Laser) }),
                ModuleState.Create(new List<SystemSlot> { SystemSlot.Create(WeaponType.Missile) })
            },
            battleLog: _battleLog
        );
    }

    [Fact]
    public void ToDto_ShouldMapAllProperties()
    {
        // Arrange
        var ship = CreateTestShip("Destroyer Alpha", ShipType.Destroyer, 150, 250);

        // Act
        var dto = ShipMapper.ToDto(ship);

        // Assert
        dto.ShipId.ShouldBe(ship.ShipId);
        dto.Name.ShouldBe("Destroyer Alpha");
        dto.Type.ShouldBe("Destroyer");
        dto.X.ShouldBe(150);
        dto.Y.ShouldBe(250);
        dto.Speed.ShouldBe(ship.Speed);
        dto.Armor.ShouldBe(ship.Armor);
        dto.Shields.ShouldBe(ship.Shields);
        dto.HitPoints.ShouldBe(ship.HitPoints);
        dto.NumberOfMissiles.ShouldBe(ship.NumberOfMissiles);
        dto.NumberOfLasers.ShouldBe(ship.NumberOfLasers);
        dto.NumberOfPointsDefense.ShouldBe(ship.NumberOfPointsDefense);
        dto.MissileMaxRange.ShouldBe(Const.MissileMaxRage);
        dto.MissileEffectiveRange.ShouldBe(Const.MissileEffectiveRage);
        dto.LaserMaxRange.ShouldBe(Const.LaserMaxRange);
    }

    [Fact]
    public void ToDto_ShouldMapModules()
    {
        // Arrange
        var ship = CreateTestShip();

        // Act
        var dto = ShipMapper.ToDto(ship);

        // Assert
        dto.Modules.ShouldNotBeNull();
        dto.Modules.Count.ShouldBe(2);
    }

    [Fact]
    public void ToBasicDto_ShouldMapOnlyBasicProperties()
    {
        // Arrange
        var ship = CreateTestShip("Basic Ship", ShipType.Cruiser, 300, 400);

        // Act
        var dto = ShipMapper.ToBasicDto(ship);

        // Assert
        dto.ShipId.ShouldBe(ship.ShipId);
        dto.Name.ShouldBe("Basic Ship");
        dto.X.ShouldBe(300);
        dto.Y.ShouldBe(400);
        dto.Armor.ShouldBe(ship.Armor);
        dto.Shields.ShouldBe(ship.Shields);
        dto.HitPoints.ShouldBe(ship.HitPoints);
        // Basic DTO should not have weapon ranges set (they default to 0)
        dto.MissileMaxRange.ShouldBe(0);
    }

    [Fact]
    public void ToDtoList_ShouldMapAllShips()
    {
        // Arrange
        var ships = new List<ShipState>
        {
            CreateTestShip("Ship 1"),
            CreateTestShip("Ship 2"),
            CreateTestShip("Ship 3")
        };

        // Act
        var dtos = ShipMapper.ToDtoList(ships);

        // Assert
        dtos.Count.ShouldBe(3);
        dtos[0].Name.ShouldBe("Ship 1");
        dtos[1].Name.ShouldBe("Ship 2");
        dtos[2].Name.ShouldBe("Ship 3");
    }

    [Fact]
    public void ToBasicDtoList_ShouldMapAllShipsWithBasicInfo()
    {
        // Arrange
        var ships = new List<ShipState>
        {
            CreateTestShip("Ship A"),
            CreateTestShip("Ship B")
        };

        // Act
        var dtos = ShipMapper.ToBasicDtoList(ships);

        // Assert
        dtos.Count.ShouldBe(2);
        dtos.All(d => d.MissileMaxRange == 0).ShouldBeTrue(); // Basic DTOs don't have weapon ranges
    }

    [Fact]
    public void ToDtoList_WithEmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        var ships = new List<ShipState>();

        // Act
        var dtos = ShipMapper.ToDtoList(ships);

        // Assert
        dtos.ShouldNotBeNull();
        dtos.Count.ShouldBe(0);
    }
}

