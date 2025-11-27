using GreatVoidBattle.Application.Dto.GameState;
using GreatVoidBattle.Application.Repositories;
using GreatVoidBattle.Core.Domains.GameState;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GreatVoidBattle.Application.Services;

public class GameStateService
{
    private readonly IGameSessionRepository _sessionRepository;
    private readonly IFractionGameStateRepository _stateRepository;
    private readonly TechnologyConfigService _technologyConfig;
    private readonly ILogger<GameStateService> _logger;

    public GameStateService(
        IGameSessionRepository sessionRepository,
        IFractionGameStateRepository stateRepository,
        TechnologyConfigService technologyConfig,
        ILogger<GameStateService> logger)
    {
        _sessionRepository = sessionRepository;
        _stateRepository = stateRepository;
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

    public async Task<List<TechnologiesForTierDto>> GetAvailableTechnologiesAsync(string fractionId)
    {
        var state = await _stateRepository.GetByFractionIdAsync(fractionId);
        if (state == null) return new List<TechnologiesForTierDto>();

        var result = new List<TechnologiesForTierDto>();
        var ownedTechIds = state.Technologies.Select(t => t.TechnologyId).ToHashSet();

        // Tylko naukowcy widzą tier+1, inni tylko current tier
        var visibleTiers = new List<int> { state.CurrentTier };
        // TODO: Dodać sprawdzanie roli - jeśli naukowiec, dodaj tier+1
        // if (isScientist) visibleTiers.Add(state.CurrentTier + 1);

        foreach (var tier in visibleTiers)
        {
            var techs = _technologyConfig.GetTechnologiesByTier(tier);
            var techsWithStatus = techs.Select(t =>
            {
                var isOwned = ownedTechIds.Contains(t.Id);
                var missingReqs = t.RequiredTechnologies
                    .Where(req => !ownedTechIds.Contains(req))
                    .ToList();
                var canResearch = !isOwned && missingReqs.Count == 0;

                return new TechnologyWithStatusDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    Tier = t.Tier,
                    RequiredTechnologies = t.RequiredTechnologies,
                    IsOwned = isOwned,
                    CanResearch = canResearch,
                    MissingRequirements = missingReqs
                };
            }).ToList();

            result.Add(new TechnologiesForTierDto
            {
                Tier = tier,
                Technologies = techsWithStatus
            });
        }

        return result;
    }

    public async Task<bool> AddTechnologyToFractionAsync(AddTechnologyRequestDto request)
    {
        var state = await _stateRepository.GetByFractionIdAsync(request.FractionId);
        if (state == null)
        {
            _logger.LogWarning($"Fraction state not found for {request.FractionId}");
            return false;
        }

        var tech = _technologyConfig.GetTechnologyById(request.TechnologyId);
        if (tech == null)
        {
            _logger.LogWarning($"Technology not found: {request.TechnologyId}");
            return false;
        }

        // Sprawdź czy już posiada
        if (state.Technologies.Any(t => t.TechnologyId == request.TechnologyId))
        {
            _logger.LogWarning($"Fraction {request.FractionId} already has technology {request.TechnologyId}");
            return false;
        }

        // Sprawdź wymagania
        var ownedTechIds = state.Technologies.Select(t => t.TechnologyId).ToHashSet();
        var missingReqs = tech.RequiredTechnologies.Where(r => !ownedTechIds.Contains(r)).ToList();
        if (missingReqs.Any())
        {
            _logger.LogWarning($"Missing requirements for {request.TechnologyId}: {string.Join(", ", missingReqs)}");
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

        await _stateRepository.AddTechnologyAsync(request.FractionId, fractionTech);
        return true;
    }

    public async Task<bool> RemoveTechnologyFromFractionAsync(string fractionId, string technologyId)
    {
        await _stateRepository.RemoveTechnologyAsync(fractionId, technologyId);
        return true;
    }

    public async Task<bool> AdvanceTierAsync(string fractionId)
    {
        var state = await _stateRepository.GetByFractionIdAsync(fractionId);
        if (state == null || !state.CanAdvanceToNextTier())
        {
            return false;
        }

        await _stateRepository.AdvanceTierAsync(fractionId);
        return true;
    }

    // Research Request Management
    public async Task<bool> RequestResearchAsync(string fractionId, string technologyId)
    {
        var state = await _stateRepository.GetByFractionIdAsync(fractionId);
        if (state == null)
        {
            _logger.LogWarning($"Fraction state not found for {fractionId}");
            return false;
        }

        var tech = _technologyConfig.GetTechnologyById(technologyId);
        if (tech == null)
        {
            _logger.LogWarning($"Technology not found: {technologyId}");
            return false;
        }

        // Sprawdź czy już posiada
        if (state.Technologies.Any(t => t.TechnologyId == technologyId))
        {
            _logger.LogWarning($"Fraction {fractionId} already has technology {technologyId}");
            return false;
        }

        // Sprawdź wymagania
        var ownedTechIds = state.Technologies.Select(t => t.TechnologyId).ToHashSet();
        var missingReqs = tech.RequiredTechnologies.Where(r => !ownedTechIds.Contains(r)).ToList();
        if (missingReqs.Any())
        {
            _logger.LogWarning($"Missing requirements for {technologyId}: {string.Join(", ", missingReqs)}");
            return false;
        }

        var request = new ResearchRequest
        {
            TechnologyId = technologyId,
            RequestedAt = DateTime.UtcNow,
            Status = ResearchRequestStatus.Pending
        };

        await _stateRepository.AddResearchRequestAsync(fractionId, request);
        _logger.LogInformation($"Research request added: {fractionId} -> {technologyId}");
        return true;
    }

    public async Task<List<ResearchRequestDto>> GetPendingResearchRequestsAsync(string fractionId)
    {
        var requests = await _stateRepository.GetPendingResearchRequestsAsync(fractionId);
        return requests.Select(r =>
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
        }).ToList();
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
                .Select(r =>
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
                }).ToList()
        }).ToList();
    }

    public async Task<bool> EndTurnAsync(List<TurnResolutionDto> resolutions)
    {
        foreach (var resolution in resolutions)
        {
            if (resolution.Approved)
            {
                await _stateRepository.ApproveResearchRequestAsync(
                    resolution.FractionId,
                    resolution.TechnologyId,
                    resolution.Comment);
                _logger.LogInformation($"Approved research: {resolution.FractionId} -> {resolution.TechnologyId}");
            }
            else
            {
                await _stateRepository.RejectResearchRequestAsync(
                    resolution.FractionId,
                    resolution.TechnologyId,
                    resolution.Comment);
                _logger.LogInformation($"Rejected research: {resolution.FractionId} -> {resolution.TechnologyId}");
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

        var technologies = state.Technologies.Select(t =>
        {
            var tech = _technologyConfig.GetTechnologyById(t.TechnologyId);
            return new FractionTechnologyDto
            {
                TechnologyId = t.TechnologyId,
                TechnologyName = tech?.Name ?? "Unknown",
                Source = t.Source.ToString(),
                SourceFractionId = t.SourceFractionId,
                SourceFractionName = t.SourceFractionId, // TODO: Pobrać nazwę
                SourceDescription = t.SourceDescription,
                Comment = t.Comment,
                AcquiredDate = t.AcquiredDate
            };
        }).ToList();

        return new FractionGameStateDto
        {
            Id = state.Id,
            FractionId = state.FractionId,
            FractionName = fractionName,
            CurrentTier = state.CurrentTier,
            ResearchedTechnologiesInCurrentTier = state.ResearchedTechnologiesInCurrentTier,
            CanAdvanceToNextTier = state.CanAdvanceToNextTier(),
            Technologies = technologies,
            CreatedAt = state.CreatedAt,
            UpdatedAt = state.UpdatedAt
        };
    }
}