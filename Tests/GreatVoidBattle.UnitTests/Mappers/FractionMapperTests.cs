using GreatVoidBattle.Application.Mappers;
using GreatVoidBattle.Core.Domains;
using GreatVoidBattle.Core.Domains.Enums;
using Shouldly;

namespace GreatVoidBattle.UnitTests.Mappers;

public class FractionMapperTests
{
    private readonly BattleLog _battleLog = new();

    private FractionState CreateTestFraction(string name = "Test Fraction", string playerName = "Player 1", string color = "#FF0000")
    {
        return FractionState.CreateNew(name, playerName, color);
    }

    private ShipState CreateTestShip(Guid fractionId, string name = "Test Ship")
    {
        return ShipState.Create(
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
            battleLog: _battleLog
        );
    }

    [Fact]
    public void ToDto_ShouldMapAllProperties()
    {
        // Arrange
        var fraction = CreateTestFraction("Alpha Faction", "Commander Alpha", "#00FF00");

        // Act
        var dto = FractionMapper.ToDto(fraction);

        // Assert
        dto.FractionId.ShouldBe(fraction.FractionId);
        dto.FractionName.ShouldBe("Alpha Faction");
        dto.PlayerName.ShouldBe("Commander Alpha");
        dto.FractionColor.ShouldBe("#00FF00");
        dto.IsDefeated.ShouldBe(fraction.IsDefeated);
        dto.TurnFinished.ShouldBe(fraction.TurnFinished);
    }

    [Fact]
    public void ToDto_WithShips_ShouldMapShipsWithFullDetails()
    {
        // Arrange
        var fraction = CreateTestFraction();
        fraction.AddShip(CreateTestShip(fraction.FractionId, "Ship 1"));
        fraction.AddShip(CreateTestShip(fraction.FractionId, "Ship 2"));

        // Act
        var dto = FractionMapper.ToDto(fraction, includeFullShipDetails: true);

        // Assert
        dto.Ships.Count.ShouldBe(2);
        dto.Ships[0].MissileMaxRange.ShouldBe(Const.MissileMaxRage); // Full details include weapon ranges
    }

    [Fact]
    public void ToDto_WithShips_ShouldMapShipsWithBasicDetails()
    {
        // Arrange
        var fraction = CreateTestFraction();
        fraction.AddShip(CreateTestShip(fraction.FractionId, "Ship 1"));

        // Act
        var dto = FractionMapper.ToDto(fraction, includeFullShipDetails: false);

        // Assert
        dto.Ships.Count.ShouldBe(1);
        dto.Ships[0].MissileMaxRange.ShouldBe(0); // Basic details don't include weapon ranges
    }

    [Fact]
    public void ToAdminDto_ShouldIncludeAuthToken()
    {
        // Arrange
        var fraction = CreateTestFraction("Admin Faction", "Admin Player", "#0000FF");

        // Act
        var dto = FractionMapper.ToAdminDto(fraction);

        // Assert
        dto.FractionId.ShouldBe(fraction.FractionId);
        dto.FractionName.ShouldBe("Admin Faction");
        dto.AuthToken.ShouldBe(fraction.AuthToken);
        dto.AuthToken.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void ToAdminDto_ShouldMapAllPropertiesFromBaseDto()
    {
        // Arrange
        var fraction = CreateTestFraction();
        fraction.TurnFinished = true;

        // Act
        var dto = FractionMapper.ToAdminDto(fraction);

        // Assert
        dto.PlayerName.ShouldBe(fraction.PlayerName);
        dto.FractionColor.ShouldBe(fraction.FractionColor);
        dto.IsDefeated.ShouldBe(fraction.IsDefeated);
        dto.TurnFinished.ShouldBeTrue();
    }

    [Fact]
    public void ToDtoList_ShouldMapAllFractions()
    {
        // Arrange
        var fractions = new List<FractionState>
        {
            CreateTestFraction("Faction A"),
            CreateTestFraction("Faction B"),
            CreateTestFraction("Faction C")
        };

        // Act
        var dtos = FractionMapper.ToDtoList(fractions);

        // Assert
        dtos.Count.ShouldBe(3);
        dtos[0].FractionName.ShouldBe("Faction A");
        dtos[1].FractionName.ShouldBe("Faction B");
        dtos[2].FractionName.ShouldBe("Faction C");
    }

    [Fact]
    public void ToAdminDtoList_ShouldIncludeAuthTokensForAllFractions()
    {
        // Arrange
        var fractions = new List<FractionState>
        {
            CreateTestFraction("Admin Faction 1"),
            CreateTestFraction("Admin Faction 2")
        };

        // Act
        var dtos = FractionMapper.ToAdminDtoList(fractions);

        // Assert
        dtos.Count.ShouldBe(2);
        dtos.All(d => d.AuthToken != Guid.Empty).ShouldBeTrue();
    }

    [Fact]
    public void ToDtoList_WithEmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        var fractions = new List<FractionState>();

        // Act
        var dtos = FractionMapper.ToDtoList(fractions);

        // Assert
        dtos.ShouldNotBeNull();
        dtos.Count.ShouldBe(0);
    }
}

