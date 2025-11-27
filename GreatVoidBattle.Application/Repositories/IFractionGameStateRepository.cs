using GreatVoidBattle.Core.Domains.GameState;

namespace GreatVoidBattle.Application.Repositories;

public interface IFractionGameStateRepository
{
    Task<FractionGameState?> GetByFractionIdAsync(string fractionId);

    Task<FractionGameState?> GetByIdAsync(string id);

    Task<List<FractionGameState>> GetAllAsync();

    Task<FractionGameState> CreateAsync(FractionGameState state);

    Task UpdateAsync(FractionGameState state);

    Task DeleteAsync(string id);

    Task<bool> HasTechnologyAsync(string fractionId, string technologyId);

    Task AddTechnologyAsync(string fractionId, FractionTechnology technology);

    Task RemoveTechnologyAsync(string fractionId, string technologyId);

    Task AdvanceTierAsync(string fractionId);

    // Research Request management
    Task AddResearchRequestAsync(string fractionId, ResearchRequest request);

    Task<List<ResearchRequest>> GetPendingResearchRequestsAsync(string fractionId);

    Task<List<FractionGameState>> GetAllWithPendingRequestsAsync();

    Task ApproveResearchRequestAsync(string fractionId, string technologyId, string? adminComment);

    Task RejectResearchRequestAsync(string fractionId, string technologyId, string? adminComment);
}