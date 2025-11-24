using GreatVoidBattle.Core.Domains;
using GreatVoidBattle.Core.Domains.Enums;
using Shouldly;

namespace GreatVoidBattle.UnitTests.Core.Domains;

public class MissileMovementPathTests
{
    private static ShipState CreateShip(string name, double x, double y)
    {
        return ShipState.Create(
            Guid.NewGuid(), name, ShipType.Corvette, x, y,
            speed: 5, hitPoints: 100, shields: 50, armor: 20, numberOfModules: 1,
            modules: new List<ModuleState>
            {
                ModuleState.Create(new List<SystemSlot>
                {
                    SystemSlot.Create(WeaponType.Missile),
                    SystemSlot.Create(WeaponType.PointDefense),
                    SystemSlot.Create(WeaponType.Laser),
                })
            },
            new BattleLog()
            );
    }

    [Fact]
    public void Constructor_SetsPropertiesCorrectly()
    {
        // Arrange
        var ship = CreateShip("Attacker", 0, 0);
        var target = CreateShip("Defender", 2, 2);

        // Act
        var missilePath = new MissileMovementPath(ship, target, speed: 3, firedAtTurn: 1);

        // Assert
        missilePath.ShipId.ShouldBe(ship.ShipId);
        missilePath.ShipName.ShouldBe(ship.Name);
        missilePath.TargetId.ShouldBe(target.ShipId);
        missilePath.Speed.ShouldBe(3);
        missilePath.StartPosition.ShouldBe(ship.Position);
        missilePath.TargetPosition.ShouldBe(target.Position);
        missilePath.Path.ShouldNotBeNull();
        missilePath.Path.Count.ShouldBeGreaterThan(0);
        missilePath.MissileId.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void Accuracy_IsCalculatedCorrectly_WithinEffectiveRange()
    {
        // Arrange
        var ship = CreateShip("Attacker", 0, 0);
        var target = CreateShip("Defender", 0, 1); // Short distance

        // Act
        var missilePath = new MissileMovementPath(ship, target, speed: 1, firedAtTurn: 1);

        // Assert
        missilePath.Accuracy.ShouldBe(Const.MissileAccuracy + (Const.MissileEffectiveRage - missilePath.Path.Count));
    }

    [Fact]
    public void Accuracy_IsCalculatedCorrectly_BeyondEffectiveRange()
    {
        // Arrange
        var ship = CreateShip("Attacker", 0, 0);
        var target = CreateShip("Defender", 0, Const.MissileEffectiveRage + 2);

        // Act
        var missilePath = new MissileMovementPath(ship, target, speed: 1, firedAtTurn: 1);

        // Assert
        missilePath.Accuracy.ShouldBe(Const.MissileAccuracy - (missilePath.Path.Count - Const.MissileEffectiveRage));
    }

    [Fact]
    public void Constructor_ThrowsException_WhenTargetOutOfRange()
    {
        // Arrange
        var ship = CreateShip("Attacker", 0, 0);
        var target = CreateShip("Defender", 0, Const.MissileMaxRage + 5);

        // Act & Assert
        Should.Throw<InvalidOperationException>(() =>
            new MissileMovementPath(ship, target, speed: 1, firedAtTurn: 1)
        ).Message.ShouldBe("Missile target is out of range.");
    }

    [Fact]
    public void Path_IsGeneratedCorrectly()
    {
        // Arrange
        var ship = CreateShip("Attacker", 1, 1);
        var target = CreateShip("Defender", 4, 4);

        // Act
        var missilePath = new MissileMovementPath(ship, target, speed: 2, firedAtTurn: 1);

        // Assert
        missilePath.Path.Count.ShouldBeGreaterThan(0);
        missilePath.Path.Last().ShouldBe(target.Position);
    }
}