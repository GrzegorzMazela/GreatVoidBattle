using GreatVoidBattle.Core.Domains.Exploration;
using Shouldly;

namespace GreatVoidBattle.UnitTests.Core.Domains.Exploration;

public class ExpeditionTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaults()
    {
        // Act
        var expedition = new Expedition();

        // Assert
        expedition.Id.ShouldNotBeNullOrEmpty();
        expedition.FractionId.ShouldBe(string.Empty);
        expedition.SystemId.ShouldBe(string.Empty);
        expedition.Status.ShouldBe(ExpeditionStatus.Pending);
        expedition.SentAtTurn.ShouldBe(0);
        expedition.FractionComment.ShouldBe(string.Empty);
        expedition.AdminComment.ShouldBe(string.Empty);
        expedition.Result.ShouldBeNull();
        expedition.ResolvedAt.ShouldBeNull();
        expedition.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow.AddSeconds(5));
    }

    [Fact]
    public void Expedition_ShouldAllowSettingAllProperties()
    {
        // Arrange & Act
        var expedition = new Expedition
        {
            FractionId = "hegemonia-titanum",
            SystemId = "system-1",
            Status = ExpeditionStatus.Approved,
            SentAtTurn = 5,
            FractionComment = "Checking for resources",
            AdminComment = "Approved - found minerals"
        };

        // Assert
        expedition.FractionId.ShouldBe("hegemonia-titanum");
        expedition.SystemId.ShouldBe("system-1");
        expedition.Status.ShouldBe(ExpeditionStatus.Approved);
        expedition.SentAtTurn.ShouldBe(5);
        expedition.FractionComment.ShouldBe("Checking for resources");
        expedition.AdminComment.ShouldBe("Approved - found minerals");
    }

    [Theory]
    [InlineData(ExpeditionStatus.Pending)]
    [InlineData(ExpeditionStatus.Approved)]
    [InlineData(ExpeditionStatus.Rejected)]
    [InlineData(ExpeditionStatus.Cancelled)]
    public void Status_ShouldSupportAllStatusValues(ExpeditionStatus status)
    {
        // Arrange & Act
        var expedition = new Expedition { Status = status };

        // Assert
        expedition.Status.ShouldBe(status);
    }

    [Fact]
    public void Result_ShouldStoreExpeditionResult()
    {
        // Arrange
        var result = new ExpeditionResult
        {
            Success = true,
            Message = "Found valuable resources",
            DiscoveredInfo = new DiscoveredSystemInfo
            {
                SystemName = "Alpha Prime",
                Type = "resources",
                Description = "Rich in minerals",
                Resources = "Tritium",
                Anomalies = "",
                ConnectedSystems = new List<string> { "system-2" }
            }
        };

        // Act
        var expedition = new Expedition { Result = result };

        // Assert
        expedition.Result.ShouldNotBeNull();
        expedition.Result.Success.ShouldBeTrue();
        expedition.Result.Message.ShouldBe("Found valuable resources");
        expedition.Result.DiscoveredInfo.ShouldNotBeNull();
        expedition.Result.DiscoveredInfo.SystemName.ShouldBe("Alpha Prime");
        expedition.Result.DiscoveredInfo.Resources.ShouldBe("Tritium");
    }

    [Fact]
    public void ResolvedAt_ShouldBeNullForPendingExpedition()
    {
        // Act
        var expedition = new Expedition();

        // Assert
        expedition.ResolvedAt.ShouldBeNull();
    }

    [Fact]
    public void ResolvedAt_ShouldBeSetWhenResolved()
    {
        // Arrange
        var resolvedTime = DateTime.UtcNow;

        // Act
        var expedition = new Expedition
        {
            Status = ExpeditionStatus.Approved,
            ResolvedAt = resolvedTime
        };

        // Assert
        expedition.ResolvedAt.ShouldBe(resolvedTime);
    }
}

public class DiscoveredSystemInfoTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaults()
    {
        // Act
        var info = new DiscoveredSystemInfo();

        // Assert
        info.SystemName.ShouldBe(string.Empty);
        info.Type.ShouldBe(string.Empty);
        info.Description.ShouldBe(string.Empty);
        info.Resources.ShouldBe(string.Empty);
        info.Anomalies.ShouldBe(string.Empty);
        info.ConnectedSystems.ShouldBeEmpty();
    }

    [Fact]
    public void DiscoveredSystemInfo_ShouldStoreAllProperties()
    {
        // Act
        var info = new DiscoveredSystemInfo
        {
            SystemName = "Beta Ceti",
            Type = "habitable",
            Description = "Earth-like planet",
            Resources = "Water, Oxygen",
            Anomalies = "Ancient ruins",
            ConnectedSystems = new List<string> { "sys-1", "sys-2", "sys-3" }
        };

        // Assert
        info.SystemName.ShouldBe("Beta Ceti");
        info.Type.ShouldBe("habitable");
        info.Description.ShouldBe("Earth-like planet");
        info.Resources.ShouldBe("Water, Oxygen");
        info.Anomalies.ShouldBe("Ancient ruins");
        info.ConnectedSystems.Count.ShouldBe(3);
    }
}

