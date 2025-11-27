using GreatVoidBattle.Application.Mappers;
using GreatVoidBattle.Core.Domains;
using Shouldly;

namespace GreatVoidBattle.UnitTests.Mappers;

public class TurnLogMapperTests
{
    private TurnLogEntry CreateTestLogEntry(
        TurnLogType type = TurnLogType.LaserHit,
        string? adminLog = null)
    {
        return new TurnLogEntry
        {
            Type = type,
            FractionId = Guid.NewGuid(),
            FractionName = "Test Fraction",
            TargetFractionId = Guid.NewGuid(),
            TargetFractionName = "Target Fraction",
            ShipId = Guid.NewGuid(),
            ShipName = "Attacker Ship",
            TargetShipId = Guid.NewGuid(),
            TargetShipName = "Target Ship",
            Message = "Test message",
            AdminLog = adminLog
        };
    }

    [Fact]
    public void ToDto_ShouldMapAllProperties()
    {
        // Arrange
        var log = CreateTestLogEntry(TurnLogType.MissileHit);

        // Act
        var dto = TurnLogMapper.ToDto(log);

        // Assert
        dto.Type.ShouldBe("MissileHit");
        dto.FractionId.ShouldBe(log.FractionId);
        dto.FractionName.ShouldBe("Test Fraction");
        dto.TargetFractionId.ShouldBe(log.TargetFractionId);
        dto.TargetFractionName.ShouldBe("Target Fraction");
        dto.ShipId.ShouldBe(log.ShipId);
        dto.ShipName.ShouldBe("Attacker Ship");
        dto.TargetShipId.ShouldBe(log.TargetShipId);
        dto.TargetShipName.ShouldBe("Target Ship");
        dto.Message.ShouldBe("Test message");
    }

    [Fact]
    public void ToDto_ShouldNotIncludeAdminLog()
    {
        // Arrange
        var log = CreateTestLogEntry(adminLog: "Secret admin information");

        // Act
        var dto = TurnLogMapper.ToDto(log);

        // Assert
        dto.AdminLog.ShouldBeNull();
    }

    [Fact]
    public void ToAdminDto_ShouldIncludeAdminLog()
    {
        // Arrange
        var log = CreateTestLogEntry(adminLog: "Detailed admin info: accuracy=85%, damage=42");

        // Act
        var dto = TurnLogMapper.ToAdminDto(log);

        // Assert
        dto.AdminLog.ShouldBe("Detailed admin info: accuracy=85%, damage=42");
    }

    [Fact]
    public void ToAdminDto_ShouldMapAllProperties()
    {
        // Arrange
        var log = CreateTestLogEntry(TurnLogType.ShipDestroyed, "Ship destroyed by critical hit");

        // Act
        var dto = TurnLogMapper.ToAdminDto(log);

        // Assert
        dto.Type.ShouldBe("ShipDestroyed");
        dto.FractionId.ShouldBe(log.FractionId);
        dto.FractionName.ShouldBe("Test Fraction");
        dto.Message.ShouldBe("Test message");
        dto.AdminLog.ShouldBe("Ship destroyed by critical hit");
    }

    [Fact]
    public void ToDtoList_ShouldMapAllLogs()
    {
        // Arrange
        var logs = new List<TurnLogEntry>
        {
            CreateTestLogEntry(TurnLogType.LaserHit),
            CreateTestLogEntry(TurnLogType.LaserMiss),
            CreateTestLogEntry(TurnLogType.MissileFired)
        };

        // Act
        var dtos = TurnLogMapper.ToDtoList(logs);

        // Assert
        dtos.Count.ShouldBe(3);
        dtos[0].Type.ShouldBe("LaserHit");
        dtos[1].Type.ShouldBe("LaserMiss");
        dtos[2].Type.ShouldBe("MissileFired");
    }

    [Fact]
    public void ToDtoList_ShouldNotIncludeAdminLogs()
    {
        // Arrange
        var logs = new List<TurnLogEntry>
        {
            CreateTestLogEntry(adminLog: "Admin info 1"),
            CreateTestLogEntry(adminLog: "Admin info 2")
        };

        // Act
        var dtos = TurnLogMapper.ToDtoList(logs);

        // Assert
        dtos.All(d => d.AdminLog == null).ShouldBeTrue();
    }

    [Fact]
    public void ToAdminDtoList_ShouldIncludeAllAdminLogs()
    {
        // Arrange
        var logs = new List<TurnLogEntry>
        {
            CreateTestLogEntry(adminLog: "Admin info 1"),
            CreateTestLogEntry(adminLog: "Admin info 2")
        };

        // Act
        var dtos = TurnLogMapper.ToAdminDtoList(logs);

        // Assert
        dtos[0].AdminLog.ShouldBe("Admin info 1");
        dtos[1].AdminLog.ShouldBe("Admin info 2");
    }

    [Fact]
    public void ToDtoList_WithEmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        var logs = new List<TurnLogEntry>();

        // Act
        var dtos = TurnLogMapper.ToDtoList(logs);

        // Assert
        dtos.ShouldNotBeNull();
        dtos.Count.ShouldBe(0);
    }

    [Theory]
    [InlineData(TurnLogType.ShipMove)]
    [InlineData(TurnLogType.LaserHit)]
    [InlineData(TurnLogType.LaserMiss)]
    [InlineData(TurnLogType.MissileFired)]
    [InlineData(TurnLogType.MissileHit)]
    [InlineData(TurnLogType.MissileMiss)]
    [InlineData(TurnLogType.MissileIntercepted)]
    [InlineData(TurnLogType.ShipDestroyed)]
    [InlineData(TurnLogType.DamageDealt)]
    [InlineData(TurnLogType.DamageReceived)]
    public void ToDto_ShouldMapAllTurnLogTypes(TurnLogType logType)
    {
        // Arrange
        var log = CreateTestLogEntry(logType);

        // Act
        var dto = TurnLogMapper.ToDto(log);

        // Assert
        dto.Type.ShouldBe(logType.ToString());
    }
}

