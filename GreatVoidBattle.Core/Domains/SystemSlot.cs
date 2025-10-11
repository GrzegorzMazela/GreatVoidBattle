using GreatVoidBattle.Core.Domains.Enums;

namespace GreatVoidBattle.Core.Domains;

public class SystemSlot
{
    public Guid SystemId { get; set; }
    public WeaponType? WeaponType { get; set; } // Missile, Laser, PD

    public static SystemSlot Create(WeaponType weaponType)
    {
        return new SystemSlot
        {
            SystemId = Guid.NewGuid(),
            WeaponType = weaponType
        };
    }
}