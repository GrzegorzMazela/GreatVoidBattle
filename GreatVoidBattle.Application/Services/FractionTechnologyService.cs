using GreatVoidBattle.Application.Repositories;
using GreatVoidBattle.Core.Domains.GameState;
using Microsoft.Extensions.Logging;

namespace GreatVoidBattle.Application.Services;

/// <summary>
/// Service handling business logic for fraction technologies.
/// Separates business logic from repository (data access) layer.
/// </summary>
public class FractionTechnologyService
{
    private readonly IFractionGameStateRepository _stateRepository;
    private readonly ILogger<FractionTechnologyService> _logger;

    public FractionTechnologyService(
        IFractionGameStateRepository stateRepository,
        ILogger<FractionTechnologyService> logger)
    {
        _stateRepository = stateRepository;
        _logger = logger;
    }

    /// <summary>
    /// Checks if a fraction has a specific technology
    /// </summary>
    public async Task<bool> HasTechnologyAsync(string fractionId, string technologyId)
    {
        var state = await _stateRepository.GetByFractionIdAsync(fractionId);
        return state?.Technologies.Any(t => t.TechnologyId == technologyId) ?? false;
    }

    /// <summary>
    /// Adds a technology to a fraction
    /// </summary>
    public async Task AddTechnologyAsync(string fractionId, FractionTechnology technology)
    {
        var state = await _stateRepository.GetByFractionIdAsync(fractionId);
        if (state == null)
        {
            _logger.LogWarning($"Fraction state not found for {fractionId}");
            return;
        }

        state.Technologies.Add(technology);

        // Jeśli technologia pochodzi z badań, zwiększ licznik
        if (technology.Source == TechnologySource.Research)
        {
            state.ResearchedTechnologiesInCurrentTier++;
        }

        await _stateRepository.UpdateAsync(state);
        _logger.LogInformation($"Technology {technology.TechnologyId} added to fraction {fractionId}");
    }

    /// <summary>
    /// Removes a technology from a fraction
    /// </summary>
    public async Task RemoveTechnologyAsync(string fractionId, string technologyId)
    {
        var state = await _stateRepository.GetByFractionIdAsync(fractionId);
        if (state == null)
        {
            _logger.LogWarning($"Fraction state not found for {fractionId}");
            return;
        }

        var tech = state.Technologies.FirstOrDefault(t => t.TechnologyId == technologyId);
        if (tech != null)
        {
            state.Technologies.Remove(tech);

            // Jeśli technologia pochodziła z badań, zmniejsz licznik
            if (tech.Source == TechnologySource.Research)
            {
                state.ResearchedTechnologiesInCurrentTier--;
            }

            await _stateRepository.UpdateAsync(state);
            _logger.LogInformation($"Technology {technologyId} removed from fraction {fractionId}");
        }
    }

    /// <summary>
    /// Advances a fraction to the next technology tier
    /// </summary>
    public async Task<bool> AdvanceTierAsync(string fractionId)
    {
        var state = await _stateRepository.GetByFractionIdAsync(fractionId);
        if (state == null)
        {
            _logger.LogWarning($"Fraction state not found for {fractionId}");
            return false;
        }

        if (!state.CanAdvanceToNextTier())
        {
            _logger.LogWarning($"Fraction {fractionId} cannot advance to next tier");
            return false;
        }

        state.CurrentTier++;
        state.ResearchedTechnologiesInCurrentTier = 0;
        await _stateRepository.UpdateAsync(state);
        
        _logger.LogInformation($"Fraction {fractionId} advanced to tier {state.CurrentTier}");
        return true;
    }

    /// <summary>
    /// Adds a research request for a fraction
    /// </summary>
    public async Task<bool> AddResearchRequestAsync(string fractionId, ResearchRequest request)
    {
        var state = await _stateRepository.GetByFractionIdAsync(fractionId);
        if (state == null)
        {
            _logger.LogWarning($"Fraction state not found for {fractionId}");
            return false;
        }

        // Sprawdź czy nie ma już zgłoszenia dla tej technologii
        var existingRequest = state.ResearchRequests
            .FirstOrDefault(r => r.TechnologyId == request.TechnologyId && r.Status == ResearchRequestStatus.Pending);
        
        if (existingRequest != null)
        {
            _logger.LogWarning($"Pending request for technology {request.TechnologyId} already exists for fraction {fractionId}");
            return false;
        }

        state.ResearchRequests.Add(request);
        await _stateRepository.UpdateAsync(state);
        
        _logger.LogInformation($"Research request for {request.TechnologyId} added to fraction {fractionId}");
        return true;
    }

    /// <summary>
    /// Gets pending research requests for a fraction
    /// </summary>
    public async Task<List<ResearchRequest>> GetPendingResearchRequestsAsync(string fractionId)
    {
        var state = await _stateRepository.GetByFractionIdAsync(fractionId);
        return state?.ResearchRequests
            .Where(r => r.Status == ResearchRequestStatus.Pending)
            .ToList() ?? new List<ResearchRequest>();
    }

    /// <summary>
    /// Approves a research request - adds technology to fraction
    /// </summary>
    public async Task<bool> ApproveResearchRequestAsync(string fractionId, string technologyId, string? adminComment)
    {
        var state = await _stateRepository.GetByFractionIdAsync(fractionId);
        if (state == null)
        {
            _logger.LogWarning($"Fraction state not found for {fractionId}");
            return false;
        }

        var request = state.ResearchRequests
            .FirstOrDefault(r => r.TechnologyId == technologyId && r.Status == ResearchRequestStatus.Pending);
        
        if (request == null)
        {
            _logger.LogWarning($"No pending request found for technology {technologyId} in fraction {fractionId}");
            return false;
        }

        // Update request status
        request.Status = ResearchRequestStatus.Approved;
        request.AdminComment = adminComment;

        // Add technology to fraction
        var technology = new FractionTechnology
        {
            TechnologyId = technologyId,
            Source = TechnologySource.Research,
            Comment = adminComment,
            AcquiredDate = DateTime.UtcNow
        };

        state.Technologies.Add(technology);
        state.ResearchedTechnologiesInCurrentTier++;

        await _stateRepository.UpdateAsync(state);
        
        _logger.LogInformation($"Research request approved: {fractionId} -> {technologyId}");
        return true;
    }

    /// <summary>
    /// Rejects a research request
    /// </summary>
    public async Task<bool> RejectResearchRequestAsync(string fractionId, string technologyId, string? adminComment)
    {
        var state = await _stateRepository.GetByFractionIdAsync(fractionId);
        if (state == null)
        {
            _logger.LogWarning($"Fraction state not found for {fractionId}");
            return false;
        }

        var request = state.ResearchRequests
            .FirstOrDefault(r => r.TechnologyId == technologyId && r.Status == ResearchRequestStatus.Pending);
        
        if (request == null)
        {
            _logger.LogWarning($"No pending request found for technology {technologyId} in fraction {fractionId}");
            return false;
        }

        request.Status = ResearchRequestStatus.Rejected;
        request.AdminComment = adminComment;

        await _stateRepository.UpdateAsync(state);
        
        _logger.LogInformation($"Research request rejected: {fractionId} -> {technologyId}");
        return true;
    }
}

