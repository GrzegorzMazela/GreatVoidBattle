using GreatVoidBattle.Application.Dto.GameState;
using GreatVoidBattle.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace GreatVoidBattle.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameStateController : ControllerBase
{
    private readonly GameStateService _gameStateService;
    private readonly ILogger<GameStateController> _logger;

    public GameStateController(GameStateService gameStateService, ILogger<GameStateController> logger)
    {
        _gameStateService = gameStateService;
        _logger = logger;
    }

    [HttpGet("session")]
    public async Task<ActionResult<GameSessionDto>> GetActiveSession()
    {
        var session = await _gameStateService.GetActiveSessionAsync();
        if (session == null)
        {
            return NotFound("No active game session");
        }
        return Ok(session);
    }

    [HttpPost("session")]
    public async Task<ActionResult<GameSessionDto>> CreateSession([FromBody] CreateSessionRequest request)
    {
        var session = await _gameStateService.CreateSessionAsync(request.Name, request.FractionIds);
        return Ok(session);
    }

    [HttpGet("fraction/{fractionId}")]
    public async Task<ActionResult<FractionGameStateDto>> GetFractionState(string fractionId)
    {
        var state = await _gameStateService.GetFractionStateAsync(fractionId);
        if (state == null)
        {
            return NotFound($"Fraction state not found for {fractionId}");
        }
        return Ok(state);
    }

    [HttpGet("fraction/{fractionId}/technologies")]
    public async Task<ActionResult<List<TechnologiesForTierDto>>> GetAvailableTechnologies(string fractionId)
    {
        var technologies = await _gameStateService.GetAvailableTechnologiesAsync(fractionId);
        return Ok(technologies);
    }

    [HttpPost("fraction/{fractionId}/advance-tier")]
    public async Task<ActionResult> AdvanceTier(string fractionId)
    {
        var success = await _gameStateService.AdvanceTierAsync(fractionId);
        if (!success)
        {
            return BadRequest("Cannot advance tier. Requirements not met.");
        }
        return Ok();
    }

    // Research Requests (for players)
    [HttpPost("fraction/{fractionId}/research-request")]
    public async Task<ActionResult> RequestResearch(string fractionId, [FromBody] RequestResearchDto request)
    {
        var success = await _gameStateService.RequestResearchAsync(fractionId, request.TechnologyId);
        if (!success)
        {
            return BadRequest("Cannot request research. Check requirements.");
        }
        return Ok();
    }

    [HttpGet("fraction/{fractionId}/research-requests")]
    public async Task<ActionResult<List<ResearchRequestDto>>> GetPendingRequests(string fractionId)
    {
        var requests = await _gameStateService.GetPendingResearchRequestsAsync(fractionId);
        return Ok(requests);
    }

    // Turn Management (for admin)
    [HttpGet("admin/pending-requests")]
    public async Task<ActionResult<List<FractionResearchRequestsDto>>> GetAllPendingRequests()
    {
        var requests = await _gameStateService.GetAllPendingRequestsAsync();
        return Ok(requests);
    }

    [HttpPost("admin/end-turn")]
    public async Task<ActionResult> EndTurn([FromBody] List<TurnResolutionDto> resolutions)
    {
        var success = await _gameStateService.EndTurnAsync(resolutions);
        if (!success)
        {
            return BadRequest("Failed to end turn.");
        }
        return Ok();
    }
    
    [HttpPut("admin/fraction/{fractionId}/research-slots")]
    public async Task<ActionResult> SetResearchSlots(string fractionId, [FromBody] SetResearchSlotsRequest request)
    {
        var success = await _gameStateService.SetResearchSlotsAsync(fractionId, request.Slots);
        if (!success)
        {
            return BadRequest("Failed to set research slots.");
        }
        return Ok();
    }
    
    [HttpGet("admin/fractions")]
    public async Task<ActionResult<List<FractionGameStateDto>>> GetAllFractionStates()
    {
        var states = await _gameStateService.GetAllFractionStatesAsync();
        return Ok(states);
    }
}

public record CreateSessionRequest(string Name, List<string> FractionIds);
public record RequestResearchDto(string TechnologyId);
public record SetResearchSlotsRequest(int Slots);
