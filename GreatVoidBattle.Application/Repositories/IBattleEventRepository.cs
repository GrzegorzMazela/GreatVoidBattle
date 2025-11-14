using GreatVoidBattle.Application.Events.Base;

namespace GreatVoidBattle.Application.Repositories;

public interface IBattleEventRepository
{
    Task AddAsync(BattleEvent battleEvent);

    Task<List<BattleEvent>> GetByBattleIdAsync(Guid battleId);
}