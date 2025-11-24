using GreatVoidBattle.Core.Domains;
using GreatVoidBattle.Core.Domains.Enums;
using Shouldly;
using System;
using System.Collections.Generic;
using Xunit;

namespace GreatVoidBattle.UnitTests.Core.Domains;

public class ShipStateTests
{
    private static ShipState CreateTestShip(
        int numberOfModules = 1,
        int lasers = 1,
        int missiles = 0,
        int pointDefense = 0)
    {
        var slots = new List<SystemSlot>();
        for (int i = 0; i < lasers; i++)
            slots.Add(SystemSlot.Create(WeaponType.Laser));
        for (int i = 0; i < missiles; i++)
            slots.Add(SystemSlot.Create(WeaponType.Missile));
        for (int i = 0; i < pointDefense; i++)
            slots.Add(SystemSlot.Create(WeaponType.PointDefense));

        var modules = new List<ModuleState>
        {
            ModuleState.Create(slots)
        };

        return ShipState.Create(
            fractionId: Guid.NewGuid(),
            name: "Test Ship",
            type: ShipType.Corvette,
            positionX: 0,
            positionY: 0,
            speed: 1,
            hitPoints: 100,
            shields: 25,
            armor: 25,
            numberOfModules: numberOfModules,
            modules: modules,
            battleLog: new BattleLog()
        );
    }

    [Fact]
    public void Create_ShouldInitializeShipStateCorrectly()
    {
        var modules = new List<ModuleState>
        {
            ModuleState.Create(new List<SystemSlot> { SystemSlot.Create(WeaponType.Laser) })
        };

        var ship = ShipState.Create(
            fractionId: Guid.NewGuid(),
            name: "Test Ship",
            type: ShipType.Corvette,
            positionX: 1.5,
            positionY: 2.5,
            speed: 5,
            hitPoints: 100,
            shields: 50,
            armor: 10,
            numberOfModules: 1,
            modules: modules,
            battleLog: new BattleLog()
        );

        ship.Name.ShouldBe("Test Ship");
        ship.Position.X.ShouldBe(1.5);
        ship.Position.Y.ShouldBe(2.5);
        ship.Speed.ShouldBe(5);
        ship.HitPoints.ShouldBe(100);
        ship.Shields.ShouldBe(50);
        ship.Armor.ShouldBe(10);
        ship.Modules.Count.ShouldBe(1);
        ship.Status.ShouldBe(ShipStatus.Active);
    }

    [Fact]
    public void Create_ShouldThrow_WhenNumberOfModulesDoesNotMatch()
    {
        var modules = new List<ModuleState>
        {
            ModuleState.Create(new List<SystemSlot> { SystemSlot.Create(WeaponType.Laser) })
        };

        Should.Throw<ArgumentException>(() =>
            ShipState.Create(
                fractionId: Guid.NewGuid(),
                name: "Test Ship",
                type: ShipType.Corvette,
                positionX: 0,
                positionY: 0,
                speed: 1,
                hitPoints: 100,
                shields: 50,
                armor: 10,
                numberOfModules: 2, // mismatch
                modules: modules,
                battleLog: new BattleLog()
            )
        );
    }

    [Fact]
    public void UpdatePosition_ShouldChangePosition()
    {
        var ship = CreateTestShip();
        ship.UpdatePosition(10, 20);

        ship.Position.X.ShouldBe(10);
        ship.Position.Y.ShouldBe(20);
    }

    [Fact]
    public void FireLaser_ShouldIncreaseLasersFiredPerTurn()
    {
        var ship = CreateTestShip(lasers: 2);
        ship.FireLaser();
        ship.NumberOfLasersFiredPerTurn.ShouldBe(1);
    }

    [Fact]
    public void FireMissile_ShouldIncreaseMissilesFiredPerTurn()
    {
        var ship = CreateTestShip(missiles: 2);
        ship.FireMissile();
        ship.NumberOfMissilesFiredPerTurn.ShouldBe(1);
    }

    [Fact]
    public void FinishTurn_ShouldResetFiredCounters()
    {
        var ship = CreateTestShip(lasers: 2, missiles: 2);
        ship.FireLaser();
        ship.FireMissile();
        ship.FinishTurn();

        ship.NumberOfLasersFiredPerTurn.ShouldBe(0);
        ship.NumberOfMissilesFiredPerTurn.ShouldBe(0);
    }

    [Fact]
    public void TakeDamage_OnlyShilds_ShouldReduceHitPoints()
    {
        var ship = CreateTestShip();
        var battleLog = new BattleLog();
        var initialHp = ship.HitPoints + ship.Shields + ship.Armor;
        var initialShilds = ship.Shields;
        var (hit, rolledValue) = ship.TakeDamage(battleLog, 10);
        var allHP = ship.HitPoints + ship.Shields + ship.Armor;

        allHP.ShouldBe(initialHp - 10);
        ship.Shields.ShouldBe(initialShilds - 10);
        hit.ShouldBeTrue();
    }

    [Fact]
    public void TakeDamage_ShildsAndArmors_ShouldReduceHitPoints()
    {
        var ship = CreateTestShip();
        var battleLog = new BattleLog();
        var initialHp = ship.HitPoints + ship.Shields + ship.Armor;
        var (hit, rolledValue) = ship.TakeDamage(battleLog, 40);
        var allHP = ship.HitPoints + ship.Shields + ship.Armor;

        allHP.ShouldBe(initialHp - 40);
        ship.Shields.ShouldBe(0);
        ship.Armor.ShouldBe(10);
        hit.ShouldBeTrue();
    }

    [Fact]
    public void TakeDamage_ShildsAndArmorsAndHp_ShouldReduceHitPoints()
    {
        var ship = CreateTestShip();
        var battleLog = new BattleLog();
        var initialHp = ship.HitPoints + ship.Shields + ship.Armor;
        var (hit, rolledValue) = ship.TakeDamage(battleLog, 100);
        var allHP = ship.HitPoints + ship.Shields + ship.Armor;

        allHP.ShouldBe(initialHp - 100);
        ship.Shields.ShouldBe(0);
        ship.Armor.ShouldBe(0);
        ship.HitPoints.ShouldBe(50);
        hit.ShouldBeTrue();
    }

    [Fact]
    public void GetPointDefenseAccuracy_ShouldReturnInt()
    {
        var ship = CreateTestShip(pointDefense: 2);
        var accuracy = ship.GetPointDefenseAccuracy();

        accuracy.ShouldBeOfType<int>();
    }
}