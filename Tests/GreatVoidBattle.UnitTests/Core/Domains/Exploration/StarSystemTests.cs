using GreatVoidBattle.Core.Domains.Exploration;
using Shouldly;

namespace GreatVoidBattle.UnitTests.Core.Domains.Exploration;

public class StarSystemTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaults()
    {
        // Act
        var system = new StarSystem();

        // Assert
        system.Id.ShouldNotBeNullOrEmpty();
        system.Name.ShouldBe(string.Empty);
        system.Type.ShouldBe("empty");
        system.X.ShouldBe(0);
        system.Y.ShouldBe(0);
        system.Description.ShouldBe(string.Empty);
        system.Resources.ShouldBe(string.Empty);
        system.Anomalies.ShouldBe(string.Empty);
        system.ControllingFractionId.ShouldBeNull();
        system.ConnectedSystems.ShouldBeEmpty();
        system.AdminNotes.ShouldBe(string.Empty);
        system.SecretInfo.ShouldBe(string.Empty);
        system.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow.AddSeconds(5));
    }

    [Fact]
    public void StarSystem_ShouldAllowSettingAllProperties()
    {
        // Arrange & Act
        var system = new StarSystem
        {
            Name = "Alpha Centauri",
            Type = "resources",
            X = 100.5,
            Y = 200.3,
            Description = "A rich star system",
            Resources = "Tritium, Crystals",
            Anomalies = "Strange readings",
            ControllingFractionId = "fraction-1",
            ConnectedSystems = new List<string> { "system-2", "system-3" },
            AdminNotes = "Important location",
            SecretInfo = "Hidden base"
        };

        // Assert
        system.Name.ShouldBe("Alpha Centauri");
        system.Type.ShouldBe("resources");
        system.X.ShouldBe(100.5);
        system.Y.ShouldBe(200.3);
        system.Description.ShouldBe("A rich star system");
        system.Resources.ShouldBe("Tritium, Crystals");
        system.Anomalies.ShouldBe("Strange readings");
        system.ControllingFractionId.ShouldBe("fraction-1");
        system.ConnectedSystems.Count.ShouldBe(2);
        system.AdminNotes.ShouldBe("Important location");
        system.SecretInfo.ShouldBe("Hidden base");
    }

    [Fact]
    public void ConnectedSystems_ShouldAllowAddingConnections()
    {
        // Arrange
        var system = new StarSystem();

        // Act
        system.ConnectedSystems.Add("system-1");
        system.ConnectedSystems.Add("system-2");

        // Assert
        system.ConnectedSystems.Count.ShouldBe(2);
        system.ConnectedSystems.ShouldContain("system-1");
        system.ConnectedSystems.ShouldContain("system-2");
    }

    [Fact]
    public void UpdatedAt_ShouldBeSetOnCreation()
    {
        // Act
        var system = new StarSystem();

        // Assert
        system.UpdatedAt.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow.AddSeconds(5));
    }
}

