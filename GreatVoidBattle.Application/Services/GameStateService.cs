using GreatVoidBattle.Application.Dto.GameState;
using GreatVoidBattle.Application.Repositories;
using GreatVoidBattle.Core.Domains.GameState;
using Microsoft.Extensions.Logging;

namespace GreatVoidBattle.Application.Services;

public class GameStateService
{
    private readonly IGameSessionRepository _sessionRepository;
    private readonly IFractionGameStateRepository _stateRepository;
    private readonly FractionTechnologyService _technologyService;
    private readonly TechnologyConfigService _technologyConfig;
    private readonly ILogger<GameStateService> _logger;

    public GameStateService(
        IGameSessionRepository sessionRepository,
        IFractionGameStateRepository stateRepository,
        FractionTechnologyService technologyService,
        TechnologyConfigService technologyConfig,
        ILogger<GameStateService> logger)
    {
        _sessionRepository = sessionRepository;
        _stateRepository = stateRepository;
        _technologyService = technologyService;
        _technologyConfig = technologyConfig;
        _logger = logger;
    }

    public async Task<GameSessionDto?> GetActiveSessionAsync()
    {
        var session = await _sessionRepository.GetActiveSessionAsync();
        return session == null ? null : MapToDto(session);
    }

    public async Task<GameSessionDto> CreateSessionAsync(string name, List<string> fractionIds)
    {
        var session = new GameSession
        {
            Name = name,
            FractionIds = fractionIds,
            CurrentTurn = 1,
            Status = GameSessionStatus.Active
        };

        await _sessionRepository.CreateAsync(session);

        // Utwórz stany dla każdej frakcji
        foreach (var fractionId in fractionIds)
        {
            var state = new FractionGameState
            {
                FractionId = fractionId,
                CurrentTier = 1,
                ResearchedTechnologiesInCurrentTier = 0
            };
            await _stateRepository.CreateAsync(state);
        }

        return MapToDto(session);
    }

    public async Task<FractionGameStateDto?> GetFractionStateAsync(string fractionId)
    {
        var state = await _stateRepository.GetByFractionIdAsync(fractionId);
        return state == null ? null : await MapToDto(state);
    }
    
    public async Task<List<FractionGameStateDto>> GetAllFractionStatesAsync()
    {
        var states = await _stateRepository.GetAllAsync();
        var result = new List<FractionGameStateDto>();
        foreach (var state in states)
        {
            result.Add(await MapToDto(state));
        }
        return result;
    }

    public async Task<List<TechnologiesForTierDto>> GetAvailableTechnologiesAsync(string fractionId)
    {
        var state = await _stateRepository.GetByFractionIdAsync(fractionId);
        if (state == null) return new List<TechnologiesForTierDto>();

        var result = new List<TechnologiesForTierDto>();
        var ownedTechIds = state.Technologies.Select(t => t.TechnologyId).ToHashSet();
        var pendingTechIds = state.ResearchRequests
            .Where(r => r.Status == ResearchRequestStatus.Pending)
            .Select(r => r.TechnologyId)
            .ToHashSet();

        // Helper do pobierania tieru technologii
        int GetTechTier(string techId) => _technologyConfig.GetTechnologyById(techId)?.Tier ?? 1;
        
        // Znajdujemy widoczne tiery: od 1 do najwyższego dostępnego + 1 (preview)
        var maxVisibleTier = 1;
        while (state.CanResearchTier(GetTechTier, maxVisibleTier + 1))
        {
            maxVisibleTier++;
        }
        maxVisibleTier++; // +1 do preview następnego tieru

        for (int tier = 1; tier <= maxVisibleTier; tier++)
        {
            var techs = _technologyConfig.GetTechnologiesByTier(tier);
            if (!techs.Any()) continue; // Pomiń puste tiery
            
            var canResearchThisTier = state.CanResearchTier(GetTechTier, tier);
            var canViewThisTier = state.CanViewTier(GetTechTier, tier);
            var researchedInTier = state.GetResearchedCountForTier(GetTechTier, tier);
            
            var techsWithStatus = techs.Select(t =>
            {
                var isOwned = ownedTechIds.Contains(t.Id);
                var isPending = pendingTechIds.Contains(t.Id);
                var missingReqs = t.RequiredTechnologies
                    .Where(req => !ownedTechIds.Contains(req))
                    .ToList();
                // Można badać jeśli: nie ma technologii, nie jest w kolejce, ma wymagania, i tier jest dostępny
                var canResearch = !isOwned && !isPending && missingReqs.Count == 0 && canResearchThisTier;

                return new TechnologyWithStatusDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    Tier = t.Tier,
                    RequiredTechnologies = t.RequiredTechnologies,
                    IsOwned = isOwned,
                    CanResearch = canResearch,
                    IsPendingResearch = isPending,
                    SlotsCost = t.Tier, // Koszt slotów = tier
                    MissingRequirements = missingReqs
                };
            }).ToList();

            result.Add(new TechnologiesForTierDto
            {
                Tier = tier,
                CanResearch = canResearchThisTier,
                CanView = canViewThisTier,
                ResearchedCount = researchedInTier,
                RequiredForNextTier = 15,
                Technologies = techsWithStatus
            });
        }

        return result;
    }

    public async Task<bool> AddTechnologyToFractionAsync(AddTechnologyRequestDto request)
    {
        var validation = await ValidateTechnologyRequest(request.FractionId, request.TechnologyId);
        if (!validation.IsValid)
        {
            _logger.LogWarning(validation.ErrorMessage);
            return false;
        }

        var source = Enum.Parse<TechnologySource>(request.Source);
        var fractionTech = new FractionTechnology
        {
            TechnologyId = request.TechnologyId,
            Source = source,
            SourceFractionId = request.SourceFractionId,
            SourceDescription = request.SourceDescription,
            Comment = request.Comment,
            AcquiredDate = DateTime.UtcNow
        };

        await _technologyService.AddTechnologyAsync(request.FractionId, fractionTech);
        return true;
    }

    public async Task<bool> RemoveTechnologyFromFractionAsync(string fractionId, string technologyId)
    {
        await _technologyService.RemoveTechnologyAsync(fractionId, technologyId);
        return true;
    }

    public async Task<bool> AdvanceTierAsync(string fractionId)
    {
        return await _technologyService.AdvanceTierAsync(fractionId);
    }

    // Research Request Management
    public async Task<bool> RequestResearchAsync(string fractionId, string technologyId)
    {
        var validation = await ValidateTechnologyRequest(fractionId, technologyId);
        if (!validation.IsValid)
        {
            _logger.LogWarning(validation.ErrorMessage);
            return false;
        }

        var state = validation.State!;
        var tech = validation.Technology!;
        
        // Helper do pobierania tieru technologii
        int GetTechTier(string techId) => _technologyConfig.GetTechnologyById(techId)?.Tier ?? 1;
        
        // Sprawdź czy tier jest dostępny do badania
        if (!state.CanResearchTier(GetTechTier, tech.Tier))
        {
            _logger.LogWarning($"Cannot research tier {tech.Tier}. Need 15 researched technologies from tier {tech.Tier - 1}");
            return false;
        }
        
        // Sprawdź czy są dostępne sloty
        var usedSlots = state.GetUsedResearchSlots(GetTechTier);
        var requiredSlots = tech.Tier; // Tier = koszt slotów
        
        if (usedSlots + requiredSlots > state.ResearchSlots)
        {
            _logger.LogWarning($"Not enough research slots. Used: {usedSlots}, Required: {requiredSlots}, Available: {state.ResearchSlots}");
            return false;
        }

        var request = new ResearchRequest
        {
            TechnologyId = technologyId,
            RequestedAt = DateTime.UtcNow,
            Status = ResearchRequestStatus.Pending
        };

        var result = await _technologyService.AddResearchRequestAsync(fractionId, request);
        if (result)
        {
            _logger.LogInformation($"Research request added: {fractionId} -> {technologyId}");
        }
        return result;
    }
    
    // Admin: Set research slots for fraction
    public async Task<bool> SetResearchSlotsAsync(string fractionId, int slots)
    {
        if (slots < 0)
        {
            _logger.LogWarning($"Invalid slots count: {slots}");
            return false;
        }
        
        var state = await _stateRepository.GetByFractionIdAsync(fractionId);
        if (state == null)
        {
            _logger.LogWarning($"Fraction state not found for {fractionId}");
            return false;
        }
        
        state.ResearchSlots = slots;
        state.UpdatedAt = DateTime.UtcNow;
        await _stateRepository.UpdateAsync(state);
        
        _logger.LogInformation($"Research slots for {fractionId} set to {slots}");
        return true;
    }

    public async Task<List<ResearchRequestDto>> GetPendingResearchRequestsAsync(string fractionId)
    {
        var requests = await _technologyService.GetPendingResearchRequestsAsync(fractionId);
        return requests.Select(MapToResearchRequestDto).ToList();
    }

    public async Task<List<FractionResearchRequestsDto>> GetAllPendingRequestsAsync()
    {
        var states = await _stateRepository.GetAllWithPendingRequestsAsync();
        return states.Select(s => new FractionResearchRequestsDto
        {
            FractionId = s.FractionId,
            FractionName = s.FractionId, // TODO: Pobrać nazwę
            PendingRequests = s.ResearchRequests
                .Where(r => r.Status == ResearchRequestStatus.Pending)
                .Select(MapToResearchRequestDto)
                .ToList()
        }).ToList();
    }

    public async Task<bool> EndTurnAsync(List<TurnResolutionDto> resolutions)
    {
        foreach (var resolution in resolutions)
        {
            if (resolution.Approved)
            {
                await _technologyService.ApproveResearchRequestAsync(
                    resolution.FractionId,
                    resolution.TechnologyId,
                    resolution.Comment);
            }
            else
            {
                await _technologyService.RejectResearchRequestAsync(
                    resolution.FractionId,
                    resolution.TechnologyId,
                    resolution.Comment);
            }
        }

        // Zwiększ numer tury
        var session = await _sessionRepository.GetActiveSessionAsync();
        if (session != null)
        {
            await _sessionRepository.IncrementTurnAsync(session.Id);
            _logger.LogInformation($"Turn incremented to {session.CurrentTurn + 1}");
        }

        return true;
    }

    public async Task<InitializeGameResponse> InitializeGameAsync(string gameName)
    {
        // Sprawdź czy gra już istnieje
        var existing = await _sessionRepository.GetActiveSessionAsync();
        if (existing != null)
        {
            _logger.LogWarning("Active game session already exists");
            throw new InvalidOperationException("Active game session already exists");
        }

        // Zdefiniuj 3 podstawowe frakcje z mapowaniem do ról Discord
        var fractions = new List<FractionInitInfo>
        {
            new FractionInitInfo 
            { 
                FractionId = "hegemonia_titanum",
                FractionName = "Hegemonia Titanum",
                DiscordRoleName = "Hegemonia Titanum"
            },
            new FractionInitInfo 
            { 
                FractionId = "shimura_incorporated",
                FractionName = "Shimura Incorporated",
                DiscordRoleName = "Shimura Incorporated"
            },
            new FractionInitInfo 
            { 
                FractionId = "protektorat_pogranicza",
                FractionName = "Protektorat Pogranicza",
                DiscordRoleName = "Protektorat Pogranicza"
            }
        };

        // Utwórz sesję gry
        var session = new GameSession
        {
            Name = gameName,
            FractionIds = fractions.Select(f => f.FractionId).ToList(),
            CurrentTurn = 1,
            Status = GameSessionStatus.Active
        };

        await _sessionRepository.CreateAsync(session);
        _logger.LogInformation($"Created game session: {session.Id}");

        // Utwórz stany dla każdej frakcji
        foreach (var fraction in fractions)
        {
            var state = new FractionGameState
            {
                FractionId = fraction.FractionId,
                CurrentTier = 1,
                ResearchedTechnologiesInCurrentTier = 0
            };
            await _stateRepository.CreateAsync(state);
            _logger.LogInformation($"Created state for fraction: {fraction.FractionName}");
        }

        return new InitializeGameResponse
        {
            SessionId = session.Id,
            Fractions = fractions
        };
    }

    #region Private Validation Methods

    /// <summary>
    /// Validates technology request - checks if fraction and technology exist,
    /// if technology is not already owned, and if all requirements are met
    /// </summary>
    private async Task<TechnologyValidationResult> ValidateTechnologyRequest(string fractionId, string technologyId)
    {
        var state = await _stateRepository.GetByFractionIdAsync(fractionId);
        if (state == null)
        {
            return TechnologyValidationResult.Failure($"Fraction state not found for {fractionId}");
        }

        var tech = _technologyConfig.GetTechnologyById(technologyId);
        if (tech == null)
        {
            return TechnologyValidationResult.Failure($"Technology not found: {technologyId}");
        }

        // Sprawdź czy już posiada
        if (state.Technologies.Any(t => t.TechnologyId == technologyId))
        {
            return TechnologyValidationResult.Failure($"Fraction {fractionId} already has technology {technologyId}");
        }

        // Sprawdź wymagania
        var ownedTechIds = state.Technologies.Select(t => t.TechnologyId).ToHashSet();
        var missingReqs = tech.RequiredTechnologies.Where(r => !ownedTechIds.Contains(r)).ToList();
        if (missingReqs.Any())
        {
            return TechnologyValidationResult.MissingRequirementsFailure(
                $"Missing requirements for {technologyId}: {string.Join(", ", missingReqs)}",
                missingReqs);
        }

        return TechnologyValidationResult.Success(state, tech, ownedTechIds);
    }

    #endregion

    #region Private Mapping Methods

    private GameSessionDto MapToDto(GameSession session)
    {
        return new GameSessionDto
        {
            Id = session.Id,
            Name = session.Name,
            CurrentTurn = session.CurrentTurn,
            FractionIds = session.FractionIds,
            Status = session.Status.ToString(),
            CreatedAt = session.CreatedAt,
            UpdatedAt = session.UpdatedAt
        };
    }

    private async Task<FractionGameStateDto> MapToDto(FractionGameState state)
    {
        // TODO: Pobrać nazwę frakcji z FractionRepository
        var fractionName = state.FractionId; // Tymczasowo
        
        // Helper do pobierania tieru technologii
        int GetTechTier(string techId) => _technologyConfig.GetTechnologyById(techId)?.Tier ?? 1;

        var technologies = state.Technologies.Select(t =>
        {
            var tech = _technologyConfig.GetTechnologyById(t.TechnologyId);
            return new FractionTechnologyDto
            {
                TechnologyId = t.TechnologyId,
                TechnologyName = tech?.Name ?? "Unknown",
                Tier = tech?.Tier ?? 1,
                Source = t.Source.ToString(),
                SourceFractionId = t.SourceFractionId,
                SourceFractionName = t.SourceFractionId, // TODO: Pobrać nazwę
                SourceDescription = t.SourceDescription,
                Comment = t.Comment,
                AcquiredDate = t.AcquiredDate
            };
        }).ToList();
        
        // Oblicz postęp dla każdego tieru
        var tierProgress = new List<TierProgressDto>();
        for (int tier = 1; tier <= 5; tier++) // Max 5 tierów
        {
            var techs = _technologyConfig.GetTechnologiesByTier(tier);
            if (!techs.Any()) continue;
            
            tierProgress.Add(new TierProgressDto
            {
                Tier = tier,
                ResearchedCount = state.GetResearchedCountForTier(GetTechTier, tier),
                RequiredForNextTier = 15,
                CanResearch = state.CanResearchTier(GetTechTier, tier),
                CanView = state.CanViewTier(GetTechTier, tier)
            });
        }

        return new FractionGameStateDto
        {
            Id = state.Id,
            FractionId = state.FractionId,
            FractionName = fractionName,
            CurrentTier = state.CurrentTier,
            ResearchSlots = state.ResearchSlots,
            UsedResearchSlots = state.GetUsedResearchSlots(GetTechTier),
            ResearchedTechnologiesInCurrentTier = state.ResearchedTechnologiesInCurrentTier,
            CanAdvanceToNextTier = state.CanAdvanceToNextTier(),
            Technologies = technologies,
            TierProgress = tierProgress,
            CreatedAt = state.CreatedAt,
            UpdatedAt = state.UpdatedAt
        };
    }

    private ResearchRequestDto MapToResearchRequestDto(ResearchRequest r)
    {
        var tech = _technologyConfig.GetTechnologyById(r.TechnologyId);
        return new ResearchRequestDto
        {
            TechnologyId = r.TechnologyId,
            TechnologyName = tech?.Name ?? "Unknown",
            RequestedAt = r.RequestedAt,
            Status = r.Status.ToString(),
            AdminComment = r.AdminComment
        };
    }

    #endregion
}
