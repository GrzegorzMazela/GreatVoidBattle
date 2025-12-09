using GreatVoidBattle.Core.Domains.Exploration;
using Shouldly;

namespace GreatVoidBattle.UnitTests.Core.Domains.Exploration;

public class SystemDiscoveryTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaults()
    {
        // Act
        var discovery = new SystemDiscovery();

        // Assert
        discovery.Id.ShouldNotBeNullOrEmpty();
        discovery.FractionId.ShouldBe(string.Empty);
        discovery.SystemId.ShouldBe(string.Empty);
        discovery.KnowledgeLevel.ShouldBe(1);
        discovery.FullyExplored.ShouldBeFalse();
        discovery.KnownName.ShouldBe(string.Empty);
        discovery.KnownType.ShouldBe(string.Empty);
        discovery.KnownDescription.ShouldBe(string.Empty);
        discovery.KnownResources.ShouldBe(string.Empty);
        discovery.KnownAnomalies.ShouldBe(string.Empty);
        discovery.KnownConnections.ShouldBeEmpty();
        discovery.FractionNotes.ShouldBe(string.Empty);
        discovery.Source.ShouldBe(DiscoverySource.Expedition);
        discovery.ExpeditionId.ShouldBeNull();
        discovery.DiscoveredAt.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow.AddSeconds(5));
    }

    [Fact]
    public void SystemDiscovery_ShouldAllowSettingAllProperties()
    {
        // Arrange & Act
        var discovery = new SystemDiscovery
        {
            FractionId = "shimura-incorporated",
            SystemId = "system-42",
            KnowledgeLevel = 3,
            FullyExplored = true,
            KnownName = "Omega Station",
            KnownType = "strategic",
            KnownDescription = "Important military outpost",
            KnownResources = "Fuel depots",
            KnownAnomalies = "Temporal distortions",
            KnownConnections = new List<string> { "system-1", "system-2" },
            FractionNotes = "High priority target",
            Source = DiscoverySource.Admin,
            ExpeditionId = "exp-123"
        };

        // Assert
        discovery.FractionId.ShouldBe("shimura-incorporated");
        discovery.SystemId.ShouldBe("system-42");
        discovery.KnowledgeLevel.ShouldBe(3);
        discovery.FullyExplored.ShouldBeTrue();
        discovery.KnownName.ShouldBe("Omega Station");
        discovery.KnownType.ShouldBe("strategic");
        discovery.KnownDescription.ShouldBe("Important military outpost");
        discovery.KnownResources.ShouldBe("Fuel depots");
        discovery.KnownAnomalies.ShouldBe("Temporal distortions");
        discovery.KnownConnections.Count.ShouldBe(2);
        discovery.FractionNotes.ShouldBe("High priority target");
        discovery.Source.ShouldBe(DiscoverySource.Admin);
        discovery.ExpeditionId.ShouldBe("exp-123");
    }

    [Theory]
    [InlineData(DiscoverySource.Expedition)]
    [InlineData(DiscoverySource.Trade)]
    [InlineData(DiscoverySource.Admin)]
    [InlineData(DiscoverySource.Initial)]
    public void Source_ShouldSupportAllDiscoverySources(DiscoverySource source)
    {
        // Arrange & Act
        var discovery = new SystemDiscovery { Source = source };

        // Assert
        discovery.Source.ShouldBe(source);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void KnowledgeLevel_ShouldAcceptValidLevels(int level)
    {
        // Arrange & Act
        var discovery = new SystemDiscovery { KnowledgeLevel = level };

        // Assert
        discovery.KnowledgeLevel.ShouldBe(level);
    }

    [Fact]
    public void KnownConnections_ShouldAllowAddingConnections()
    {
        // Arrange
        var discovery = new SystemDiscovery();

        // Act
        discovery.KnownConnections.Add("connected-system-1");
        discovery.KnownConnections.Add("connected-system-2");

        // Assert
        discovery.KnownConnections.Count.ShouldBe(2);
        discovery.KnownConnections.ShouldContain("connected-system-1");
    }

    [Fact]
    public void UpdatedAt_ShouldBeSetOnCreation()
    {
        // Act
        var discovery = new SystemDiscovery();

        // Assert
        discovery.UpdatedAt.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow.AddSeconds(5));
    }
}

