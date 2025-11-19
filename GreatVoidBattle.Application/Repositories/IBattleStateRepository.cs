using GreatVoidBattle.Application.Dto.Battles;
using GreatVoidBattle.Core.Domains;

namespace GreatVoidBattle.Application.Repositories;

public interface IBattleStateRepository
{
    Task AddAsync(BattleState battleState);

    Task UpdateAsync(Guid id, BattleState battleState);

    Task<BattleState?> GetByIdAsync(Guid id);

    Task<IEnumerable<BattleDto>> GetBattles();
    Task<bool> SoftDeleteAsync(Guid id);
}