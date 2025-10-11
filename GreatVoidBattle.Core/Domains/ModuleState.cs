using GreatVoidBattle.Core.Domains.Enums;

namespace GreatVoidBattle.Core.Domains;

public class ModuleState
{
    public Guid ModuleId { get; set; }
    public List<SystemSlot> Slots { get; set; } = new(); // np. wyrzutnie, lasery, obrona

    public static ModuleState Create(List<SystemSlot> slots)
    {
        return new ModuleState
        {
            ModuleId = Guid.NewGuid(),
            Slots = slots
        };
    }
}