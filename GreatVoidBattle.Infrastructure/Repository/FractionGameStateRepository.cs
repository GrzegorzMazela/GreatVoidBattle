using GreatVoidBattle.Application.Repositories;
using GreatVoidBattle.Core.Domains.GameState;
using MongoDB.Driver;

namespace GreatVoidBattle.Infrastructure.Repository;

public class FractionGameStateRepository : IFractionGameStateRepository
{
    private readonly IMongoCollection<FractionGameState> _collection;

    public FractionGameStateRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<FractionGameState>("FractionGameStates");
    }

    public async Task<FractionGameState?> GetByFractionIdAsync(string fractionId)
    {
        return await _collection.Find(s => s.FractionId == fractionId).FirstOrDefaultAsync();
    }

    public async Task<FractionGameState?> GetByIdAsync(string id)
    {
        return await _collection.Find(s => s.Id == id).FirstOrDefaultAsync();
    }

    public async Task<List<FractionGameState>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<FractionGameState> CreateAsync(FractionGameState state)
    {
        await _collection.InsertOneAsync(state);
        return state;
    }

    public async Task UpdateAsync(FractionGameState state)
    {
        state.UpdatedAt = DateTime.UtcNow;
        await _collection.ReplaceOneAsync(s => s.Id == state.Id, state);
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneAsync(s => s.Id == id);
    }

    public async Task<bool> HasTechnologyAsync(string fractionId, string technologyId)
    {
        var state = await GetByFractionIdAsync(fractionId);
        return state?.Technologies.Any(t => t.TechnologyId == technologyId) ?? false;
    }

    public async Task AddTechnologyAsync(string fractionId, FractionTechnology technology)
    {
        var state = await GetByFractionIdAsync(fractionId);
        if (state == null) return;

        state.Technologies.Add(technology);

        // Jeśli technologia pochodzi z badań, zwiększ licznik
        if (technology.Source == TechnologySource.Research)
        {
            state.ResearchedTechnologiesInCurrentTier++;
        }

        await UpdateAsync(state);
    }

    public async Task RemoveTechnologyAsync(string fractionId, string technologyId)
    {
        var state = await GetByFractionIdAsync(fractionId);
        if (state == null) return;

        var tech = state.Technologies.FirstOrDefault(t => t.TechnologyId == technologyId);
        if (tech != null)
        {
            state.Technologies.Remove(tech);

            // Jeśli technologia pochodziła z badań, zmniejsz licznik
            if (tech.Source == TechnologySource.Research)
            {
                state.ResearchedTechnologiesInCurrentTier--;
            }

            await UpdateAsync(state);
        }
    }

    public async Task AdvanceTierAsync(string fractionId)
    {
        var state = await GetByFractionIdAsync(fractionId);
        if (state == null || !state.CanAdvanceToNextTier()) return;

        state.CurrentTier++;
        state.ResearchedTechnologiesInCurrentTier = 0;
        await UpdateAsync(state);
    }

    public async Task AddResearchRequestAsync(string fractionId, ResearchRequest request)
    {
        var state = await GetByFractionIdAsync(fractionId);
        if (state == null) return;

        // Sprawdź czy nie ma już zgłoszenia dla tej technologii
        var existingRequest = state.ResearchRequests.FirstOrDefault(r => r.TechnologyId == request.TechnologyId && r.Status == ResearchRequestStatus.Pending);
        if (existingRequest != null) return;

        state.ResearchRequests.Add(request);
        await UpdateAsync(state);
    }

    public async Task<List<ResearchRequest>> GetPendingResearchRequestsAsync(string fractionId)
    {
        var state = await GetByFractionIdAsync(fractionId);
        return state?.ResearchRequests.Where(r => r.Status == ResearchRequestStatus.Pending).ToList() ?? new List<ResearchRequest>();
    }

    public async Task<List<FractionGameState>> GetAllWithPendingRequestsAsync()
    {
        var allStates = await GetAllAsync();
        return allStates.Where(s => s.ResearchRequests.Any(r => r.Status == ResearchRequestStatus.Pending)).ToList();
    }

    public async Task ApproveResearchRequestAsync(string fractionId, string technologyId, string? adminComment)
    {
        var state = await GetByFractionIdAsync(fractionId);
        if (state == null) return;

        var request = state.ResearchRequests.FirstOrDefault(r => r.TechnologyId == technologyId && r.Status == ResearchRequestStatus.Pending);
        if (request == null) return;

        request.Status = ResearchRequestStatus.Approved;
        request.AdminComment = adminComment;

        // Dodaj technologię do posiadanych
        var technology = new FractionTechnology
        {
            TechnologyId = technologyId,
            Source = TechnologySource.Research,
            Comment = adminComment,
            AcquiredDate = DateTime.UtcNow
        };

        state.Technologies.Add(technology);
        state.ResearchedTechnologiesInCurrentTier++;

        await UpdateAsync(state);
    }

    public async Task RejectResearchRequestAsync(string fractionId, string technologyId, string? adminComment)
    {
        var state = await GetByFractionIdAsync(fractionId);
        if (state == null) return;

        var request = state.ResearchRequests.FirstOrDefault(r => r.TechnologyId == technologyId && r.Status == ResearchRequestStatus.Pending);
        if (request == null) return;

        request.Status = ResearchRequestStatus.Rejected;
        request.AdminComment = adminComment;

        await UpdateAsync(state);
    }
}