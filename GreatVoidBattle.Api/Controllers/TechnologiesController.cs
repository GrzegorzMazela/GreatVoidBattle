using GreatVoidBattle.Application.Dto.GameState;
using GreatVoidBattle.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace GreatVoidBattle.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TechnologiesController : ControllerBase
{
    private readonly TechnologyConfigService _technologyConfig;
    private readonly GameStateService _gameStateService;
    private readonly ILogger<TechnologiesController> _logger;

    public TechnologiesController(
        TechnologyConfigService technologyConfig,
        GameStateService gameStateService,
        ILogger<TechnologiesController> logger)
    {
        _technologyConfig = technologyConfig;
        _gameStateService = gameStateService;
        _logger = logger;
    }

    [HttpGet]
    public ActionResult<List<TechnologyDto>> GetAllTechnologies()
    {
        var technologies = _technologyConfig.GetAllTechnologies();
        _logger.LogInformation($"Returning {technologies.Count} technologies");
        
        var dtos = technologies.Select(t => new TechnologyDto
        {
            Id = t.Id,
            Name = t.Name,
            Description = t.Description,
            Tier = t.Tier,
            RequiredTechnologies = t.RequiredTechnologies
        }).ToList();
        
        return Ok(dtos);
    }

    [HttpGet("tier/{tier}")]
    public ActionResult<List<TechnologyDto>> GetTechnologiesByTier(int tier)
    {
        var technologies = _technologyConfig.GetTechnologiesByTier(tier);
        var dtos = technologies.Select(t => new TechnologyDto
        {
            Id = t.Id,
            Name = t.Name,
            Description = t.Description,
            Tier = t.Tier,
            RequiredTechnologies = t.RequiredTechnologies
        }).ToList();
        return Ok(dtos);
    }

    [HttpPost("assign")]
    public async Task<ActionResult> AssignTechnologyToFraction([FromBody] AddTechnologyRequestDto request)
    {
        var success = await _gameStateService.AddTechnologyToFractionAsync(request);
        if (!success)
        {
            return BadRequest("Failed to assign technology");
        }
        return Ok();
    }

    [HttpDelete("fraction/{fractionId}/technology/{technologyId}")]
    public async Task<ActionResult> RemoveTechnologyFromFraction(string fractionId, string technologyId)
    {
        var success = await _gameStateService.RemoveTechnologyFromFractionAsync(fractionId, technologyId);
        if (!success)
        {
            return BadRequest("Failed to remove technology");
        }
        return Ok();
    }
}
