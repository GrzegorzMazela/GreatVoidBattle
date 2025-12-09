using GreatVoidBattle.Application.Dto.Exploration;
using GreatVoidBattle.Application.Repositories;
using GreatVoidBattle.Core.Domains.Exploration;
using Microsoft.Extensions.Logging;

namespace GreatVoidBattle.Application.Services;

public class ExplorationService
{
    private readonly IStarSystemRepository _systemRepository;
    private readonly IExpeditionRepository _expeditionRepository;
    private readonly ISystemDiscoveryRepository _discoveryRepository;
    private readonly IExplorationStateRepository _stateRepository;
    private readonly IGameSessionRepository _sessionRepository;
    private readonly ILogger<ExplorationService> _logger;

    public ExplorationService(
        IStarSystemRepository systemRepository,
        IExpeditionRepository expeditionRepository,
        ISystemDiscoveryRepository discoveryRepository,
        IExplorationStateRepository stateRepository,
        IGameSessionRepository sessionRepository,
        ILogger<ExplorationService> logger)
    {
        _systemRepository = systemRepository;
        _expeditionRepository = expeditionRepository;
        _discoveryRepository = discoveryRepository;
        _stateRepository = stateRepository;
        _sessionRepository = sessionRepository;
        _logger = logger;
    }

    #region Star Systems

    public async Task<List<StarSystemAdminDto>> GetAllSystemsAsync()
    {
        var systems = await _systemRepository.GetAllAsync();
        return systems.Select(MapToAdminDto).ToList();
    }

    public async Task<StarSystemAdminDto?> GetSystemAsync(string systemId)
    {
        var system = await _systemRepository.GetByIdAsync(systemId);
        return system == null ? null : MapToAdminDto(system);
    }

    public async Task<StarSystemAdminDto> CreateSystemAsync(CreateStarSystemDto dto)
    {
        var system = new StarSystem
        {
            Name = dto.Name,
            X = dto.X,
            Y = dto.Y,
            Type = dto.Type,
            Description = dto.Description,
            Resources = dto.Resources,
            Anomalies = dto.Anomalies,
            ControllingFractionId = dto.ControllingFractionId,
            ConnectedSystems = dto.ConnectedSystems
        };

        await _systemRepository.CreateAsync(system);
        _logger.LogInformation($"Created star system: {system.Name} ({system.Id})");
        return MapToAdminDto(system);
    }

    public async Task<StarSystemAdminDto?> UpdateSystemAsync(string systemId, UpdateStarSystemDto dto)
    {
        var system = await _systemRepository.GetByIdAsync(systemId);
        if (system == null)
            return null;

        if (dto.Name != null) system.Name = dto.Name;
        if (dto.X.HasValue) system.X = dto.X.Value;
        if (dto.Y.HasValue) system.Y = dto.Y.Value;
        if (dto.Type != null) system.Type = dto.Type;
        if (dto.Description != null) system.Description = dto.Description;
        if (dto.Resources != null) system.Resources = dto.Resources;
        if (dto.Anomalies != null) system.Anomalies = dto.Anomalies;
        if (dto.ControllingFractionId != null) system.ControllingFractionId = dto.ControllingFractionId;
        if (dto.ConnectedSystems != null) system.ConnectedSystems = dto.ConnectedSystems;
        if (dto.AdminNotes != null) system.AdminNotes = dto.AdminNotes;
        if (dto.SecretInfo != null) system.SecretInfo = dto.SecretInfo;

        await _systemRepository.UpdateAsync(system);
        _logger.LogInformation($"Updated star system: {system.Name} ({system.Id})");
        return MapToAdminDto(system);
    }

    #endregion

    #region Fraction Exploration

    public async Task<List<SystemDiscoveryDto>> GetKnownSystemsAsync(string fractionId)
    {
        var discoveries = await _discoveryRepository.GetByFractionIdAsync(fractionId);
        return discoveries.Select(MapToDto).ToList();
    }

    public async Task<ExplorationStatusDto> GetExplorationStatusAsync(string fractionId)
    {
        var state = await _stateRepository.GetOrCreateByFractionIdAsync(fractionId);
        var pendingExpeditions = await _expeditionRepository.GetPendingByFractionIdAsync(fractionId);
        var discoveries = await _discoveryRepository.GetByFractionIdAsync(fractionId);

        return new ExplorationStatusDto
        {
            FractionId = fractionId,
            ResearchSlots = state.ResearchSlots,
            UsedSlotsThisTurn = state.UsedSlotsThisTurn,
            DiscoveredSystemsCount = discoveries.Count,
            TotalExpeditions = state.TotalExpeditions,
            SuccessfulExpeditions = state.SuccessfulExpeditions,
            PendingExpeditionsCount = pendingExpeditions.Count
        };
    }

    public async Task<ExpeditionDto?> SendExpeditionAsync(string fractionId, SendExpeditionDto dto)
    {
        // Sprawdź czy układ istnieje
        var system = await _systemRepository.GetByIdAsync(dto.SystemId);
        if (system == null)
        {
            _logger.LogWarning($"System not found: {dto.SystemId}");
            return null;
        }

        // Sprawdź sloty
        var state = await _stateRepository.GetOrCreateByFractionIdAsync(fractionId);
        if (state.UsedSlotsThisTurn >= state.ResearchSlots)
        {
            _logger.LogWarning($"No available research slots for fraction {fractionId}");
            return null;
        }

        // Pobierz aktualną turę
        var session = await _sessionRepository.GetActiveSessionAsync();
        var currentTurn = session?.CurrentTurn ?? 1;

        var expedition = new Expedition
        {
            FractionId = fractionId,
            SystemId = dto.SystemId,
            Status = ExpeditionStatus.Pending,
            SentAtTurn = currentTurn,
            FractionComment = dto.Comment
        };

        await _expeditionRepository.CreateAsync(expedition);

        // Aktualizuj wykorzystane sloty
        state.UsedSlotsThisTurn++;
        state.TotalExpeditions++;
        await _stateRepository.UpdateAsync(state);

        _logger.LogInformation($"Expedition sent: {fractionId} -> {system.Name} ({dto.SystemId})");

        return new ExpeditionDto
        {
            Id = expedition.Id,
            FractionId = fractionId,
            SystemId = dto.SystemId,
            SystemName = system.Name,
            Status = expedition.Status.ToString(),
            SentAtTurn = currentTurn,
            FractionComment = dto.Comment,
            CreatedAt = expedition.CreatedAt
        };
    }

    public async Task<bool> CancelExpeditionAsync(string fractionId, string expeditionId)
    {
        var expedition = await _expeditionRepository.GetByIdAsync(expeditionId);
        if (expedition == null || expedition.FractionId != fractionId)
            return false;

        if (expedition.Status != ExpeditionStatus.Pending)
            return false;

        expedition.Status = ExpeditionStatus.Cancelled;
        await _expeditionRepository.UpdateAsync(expedition);

        // Zwróć slot
        var state = await _stateRepository.GetByFractionIdAsync(fractionId);
        if (state != null && state.UsedSlotsThisTurn > 0)
        {
            state.UsedSlotsThisTurn--;
            await _stateRepository.UpdateAsync(state);
        }

        _logger.LogInformation($"Expedition cancelled: {expeditionId}");
        return true;
    }

    public async Task<List<ExpeditionDto>> GetPendingExpeditionsAsync(string fractionId)
    {
        var expeditions = await _expeditionRepository.GetPendingByFractionIdAsync(fractionId);
        var result = new List<ExpeditionDto>();

        foreach (var exp in expeditions)
        {
            var system = await _systemRepository.GetByIdAsync(exp.SystemId);
            result.Add(new ExpeditionDto
            {
                Id = exp.Id,
                FractionId = exp.FractionId,
                SystemId = exp.SystemId,
                SystemName = system?.Name ?? "Unknown",
                Status = exp.Status.ToString(),
                SentAtTurn = exp.SentAtTurn,
                FractionComment = exp.FractionComment,
                CreatedAt = exp.CreatedAt
            });
        }

        return result;
    }

    public async Task<ExplorationHistoryDto> GetExplorationHistoryAsync(string fractionId)
    {
        var history = await _expeditionRepository.GetHistoryByFractionIdAsync(fractionId);
        var entries = new List<ExplorationHistoryEntryDto>();

        foreach (var exp in history.OrderByDescending(e => e.CreatedAt))
        {
            var system = await _systemRepository.GetByIdAsync(exp.SystemId);
            entries.Add(new ExplorationHistoryEntryDto
            {
                Id = exp.Id,
                Type = "expedition",
                SystemId = exp.SystemId,
                SystemName = system?.Name ?? "Unknown",
                Description = exp.Result?.Message ?? "",
                Status = exp.Status.ToString(),
                Turn = exp.SentAtTurn,
                Date = exp.CreatedAt
            });
        }

        return new ExplorationHistoryDto
        {
            FractionId = fractionId,
            Entries = entries
        };
    }

    public async Task<FractionResearchSlotsDto> GetResearchSlotsAsync(string fractionId)
    {
        var state = await _stateRepository.GetOrCreateByFractionIdAsync(fractionId);
        var pendingCount = (await _expeditionRepository.GetPendingByFractionIdAsync(fractionId)).Count;

        return new FractionResearchSlotsDto
        {
            FractionId = fractionId,
            FractionName = fractionId, // TODO: Pobierz nazwę frakcji
            ResearchSlots = state.ResearchSlots,
            UsedSlotsThisTurn = state.UsedSlotsThisTurn,
            PendingExpeditions = pendingCount
        };
    }

    #endregion

    #region Admin Operations

    public async Task<bool> SetResearchSlotsAsync(string fractionId, int slots)
    {
        if (slots < 0) return false;

        var state = await _stateRepository.GetOrCreateByFractionIdAsync(fractionId);
        state.ResearchSlots = slots;
        await _stateRepository.UpdateAsync(state);

        _logger.LogInformation($"Research slots for {fractionId} set to {slots}");
        return true;
    }

    public async Task<List<ExpeditionAdminDto>> GetAllPendingExpeditionsAsync()
    {
        var expeditions = await _expeditionRepository.GetAllPendingAsync();
        var result = new List<ExpeditionAdminDto>();

        foreach (var exp in expeditions)
        {
            var system = await _systemRepository.GetByIdAsync(exp.SystemId);
            result.Add(new ExpeditionAdminDto
            {
                Id = exp.Id,
                FractionId = exp.FractionId,
                FractionName = exp.FractionId, // TODO: Pobierz nazwę frakcji
                SystemId = exp.SystemId,
                SystemName = system?.Name ?? "Unknown",
                Status = exp.Status.ToString(),
                SentAtTurn = exp.SentAtTurn,
                FractionComment = exp.FractionComment,
                AdminComment = exp.AdminComment,
                CreatedAt = exp.CreatedAt
            });
        }

        return result;
    }

    public async Task<ExpeditionAdminDto?> ResolveExpeditionAsync(string expeditionId, ResolveExpeditionDto dto)
    {
        var expedition = await _expeditionRepository.GetByIdAsync(expeditionId);
        if (expedition == null || expedition.Status != ExpeditionStatus.Pending)
            return null;

        expedition.Status = dto.Success ? ExpeditionStatus.Approved : ExpeditionStatus.Rejected;
        expedition.AdminComment = dto.AdminComment;
        expedition.ResolvedAt = DateTime.UtcNow;
        expedition.Result = new ExpeditionResult
        {
            Success = dto.Success,
            Message = dto.Message,
            DiscoveredInfo = dto.DiscoveredInfo != null ? new DiscoveredSystemInfo
            {
                SystemName = dto.DiscoveredInfo.SystemName,
                Type = dto.DiscoveredInfo.Type,
                Description = dto.DiscoveredInfo.Description,
                Resources = dto.DiscoveredInfo.Resources,
                Anomalies = dto.DiscoveredInfo.Anomalies,
                ConnectedSystems = dto.DiscoveredInfo.ConnectedSystems
            } : null
        };

        await _expeditionRepository.UpdateAsync(expedition);

        // Jeśli sukces, utwórz/aktualizuj odkrycie
        if (dto.Success && dto.DiscoveredInfo != null)
        {
            await CreateOrUpdateDiscoveryAsync(
                expedition.FractionId,
                expedition.SystemId,
                dto.DiscoveredInfo,
                expedition.Id);

            // Aktualizuj statystyki
            var state = await _stateRepository.GetByFractionIdAsync(expedition.FractionId);
            if (state != null)
            {
                state.SuccessfulExpeditions++;
                await _stateRepository.UpdateAsync(state);
            }
        }

        _logger.LogInformation($"Expedition {expeditionId} resolved: {(dto.Success ? "approved" : "rejected")}");

        var system = await _systemRepository.GetByIdAsync(expedition.SystemId);
        return new ExpeditionAdminDto
        {
            Id = expedition.Id,
            FractionId = expedition.FractionId,
            FractionName = expedition.FractionId,
            SystemId = expedition.SystemId,
            SystemName = system?.Name ?? "Unknown",
            Status = expedition.Status.ToString(),
            SentAtTurn = expedition.SentAtTurn,
            FractionComment = expedition.FractionComment,
            AdminComment = expedition.AdminComment,
            CreatedAt = expedition.CreatedAt,
            ResolvedAt = expedition.ResolvedAt,
            Result = dto.Success ? new ExpeditionResultDto
            {
                Success = true,
                Message = dto.Message,
                DiscoveredInfo = dto.DiscoveredInfo
            } : null
        };
    }

    public async Task<bool> RejectExpeditionAsync(string expeditionId, string reason)
    {
        var expedition = await _expeditionRepository.GetByIdAsync(expeditionId);
        if (expedition == null || expedition.Status != ExpeditionStatus.Pending)
            return false;

        expedition.Status = ExpeditionStatus.Rejected;
        expedition.AdminComment = reason;
        expedition.ResolvedAt = DateTime.UtcNow;
        expedition.Result = new ExpeditionResult
        {
            Success = false,
            Message = reason
        };

        await _expeditionRepository.UpdateAsync(expedition);
        _logger.LogInformation($"Expedition {expeditionId} rejected: {reason}");
        return true;
    }

    public async Task<bool> SendSystemInfoAsync(string systemId, SendSystemInfoDto dto)
    {
        var system = await _systemRepository.GetByIdAsync(systemId);
        if (system == null) return false;

        var info = dto.Info ?? new DiscoveredSystemInfoDto
        {
            SystemName = system.Name,
            Type = system.Type,
            Description = system.Description
        };

        await CreateOrUpdateDiscoveryAsync(
            dto.FractionId,
            systemId,
            info,
            null,
            DiscoverySource.Admin,
            dto.KnowledgeLevel,
            dto.FullyExplored);

        _logger.LogInformation($"System info sent to {dto.FractionId} for system {systemId}");
        return true;
    }

    public async Task<SystemNotesDto?> GetSystemNotesAsync(string systemId)
    {
        var system = await _systemRepository.GetByIdAsync(systemId);
        if (system == null) return null;

        return new SystemNotesDto
        {
            SystemId = systemId,
            AdminNotes = system.AdminNotes,
            SecretInfo = system.SecretInfo
        };
    }

    public async Task<bool> SaveSystemNotesAsync(string systemId, SystemNotesDto dto)
    {
        var system = await _systemRepository.GetByIdAsync(systemId);
        if (system == null) return false;

        system.AdminNotes = dto.AdminNotes;
        system.SecretInfo = dto.SecretInfo;
        await _systemRepository.UpdateAsync(system);

        _logger.LogInformation($"System notes saved for {systemId}");
        return true;
    }

    public async Task<SystemDiscoveryStatusDto?> GetSystemDiscoveryStatusAsync(string systemId)
    {
        var system = await _systemRepository.GetByIdAsync(systemId);
        if (system == null) return null;

        var discoveries = await _discoveryRepository.GetBySystemIdAsync(systemId);
        
        // Lista wszystkich frakcji - pobierz z sesji gry
        var session = await _sessionRepository.GetActiveSessionAsync();
        var allFractionIds = session?.FractionIds ?? new List<string>();

        var fractionStatuses = new List<FractionDiscoveryStatusDto>();
        foreach (var fractionId in allFractionIds)
        {
            var discovery = discoveries.FirstOrDefault(d => d.FractionId == fractionId);
            fractionStatuses.Add(new FractionDiscoveryStatusDto
            {
                FractionId = fractionId,
                FractionName = fractionId, // TODO: Pobierz nazwę frakcji
                IsDiscovered = discovery != null,
                KnowledgeLevel = discovery?.KnowledgeLevel ?? 0,
                FullyExplored = discovery?.FullyExplored ?? false,
                DiscoveredAt = discovery?.DiscoveredAt
            });
        }

        return new SystemDiscoveryStatusDto
        {
            SystemId = systemId,
            SystemName = system.Name,
            Fractions = fractionStatuses
        };
    }

    public async Task<AllResearchSlotsDto> GetAllResearchSlotsAsync()
    {
        var states = await _stateRepository.GetAllAsync();
        var result = new List<FractionResearchSlotsDto>();

        foreach (var state in states)
        {
            var pendingCount = (await _expeditionRepository.GetPendingByFractionIdAsync(state.FractionId)).Count;
            result.Add(new FractionResearchSlotsDto
            {
                FractionId = state.FractionId,
                FractionName = state.FractionId, // TODO: Pobierz nazwę frakcji
                ResearchSlots = state.ResearchSlots,
                UsedSlotsThisTurn = state.UsedSlotsThisTurn,
                PendingExpeditions = pendingCount
            });
        }

        return new AllResearchSlotsDto { Fractions = result };
    }

    public async Task EndExplorationTurnAsync()
    {
        // Resetuj wykorzystane sloty dla wszystkich frakcji
        await _stateRepository.ResetUsedSlotsForAllAsync();
        _logger.LogInformation("Exploration turn ended - research slots reset");
    }

    #endregion

    #region Private Methods

    private async Task CreateOrUpdateDiscoveryAsync(
        string fractionId,
        string systemId,
        DiscoveredSystemInfoDto info,
        string? expeditionId,
        DiscoverySource source = DiscoverySource.Expedition,
        int knowledgeLevel = 1,
        bool fullyExplored = false)
    {
        var discovery = await _discoveryRepository.GetByFractionAndSystemAsync(fractionId, systemId);
        var isNewDiscovery = discovery == null;
        
        if (isNewDiscovery)
        {
            discovery = new SystemDiscovery
            {
                FractionId = fractionId,
                SystemId = systemId,
                Source = source,
                ExpeditionId = expeditionId
            };

            // Aktualizuj liczbę odkrytych systemów
            var state = await _stateRepository.GetByFractionIdAsync(fractionId);
            if (state != null)
            {
                state.DiscoveredSystemsCount++;
                await _stateRepository.UpdateAsync(state);
            }
        }

        // Aktualizuj informacje (zawsze nadpisuj jeśli nowe są lepsze)
        if (!string.IsNullOrEmpty(info.SystemName))
            discovery.KnownName = info.SystemName;
        if (!string.IsNullOrEmpty(info.Type))
            discovery.KnownType = info.Type;
        if (!string.IsNullOrEmpty(info.Description))
            discovery.KnownDescription = info.Description;
        if (!string.IsNullOrEmpty(info.Resources))
            discovery.KnownResources = info.Resources;
        if (!string.IsNullOrEmpty(info.Anomalies))
            discovery.KnownAnomalies = info.Anomalies;
        if (info.ConnectedSystems.Any())
            discovery.KnownConnections = info.ConnectedSystems;
        
        if (knowledgeLevel > discovery.KnowledgeLevel)
            discovery.KnowledgeLevel = knowledgeLevel;
        if (fullyExplored)
            discovery.FullyExplored = true;

        if (isNewDiscovery)
            await _discoveryRepository.CreateAsync(discovery);
        else
            await _discoveryRepository.UpdateAsync(discovery);
    }

    private static StarSystemAdminDto MapToAdminDto(StarSystem system)
    {
        return new StarSystemAdminDto
        {
            Id = system.Id,
            Name = system.Name,
            X = system.X,
            Y = system.Y,
            Type = system.Type,
            Description = system.Description,
            Resources = system.Resources,
            Anomalies = system.Anomalies,
            ControllingFractionId = system.ControllingFractionId,
            ConnectedSystems = system.ConnectedSystems,
            AdminNotes = system.AdminNotes,
            SecretInfo = system.SecretInfo,
            CreatedAt = system.CreatedAt,
            UpdatedAt = system.UpdatedAt
        };
    }

    private static SystemDiscoveryDto MapToDto(SystemDiscovery discovery)
    {
        return new SystemDiscoveryDto
        {
            Id = discovery.Id,
            FractionId = discovery.FractionId,
            SystemId = discovery.SystemId,
            KnowledgeLevel = discovery.KnowledgeLevel,
            FullyExplored = discovery.FullyExplored,
            KnownName = discovery.KnownName,
            KnownType = discovery.KnownType,
            KnownDescription = discovery.KnownDescription,
            KnownResources = discovery.KnownResources,
            KnownAnomalies = discovery.KnownAnomalies,
            KnownConnections = discovery.KnownConnections,
            FractionNotes = discovery.FractionNotes,
            Source = discovery.Source.ToString(),
            DiscoveredAt = discovery.DiscoveredAt
        };
    }

    #endregion
}

