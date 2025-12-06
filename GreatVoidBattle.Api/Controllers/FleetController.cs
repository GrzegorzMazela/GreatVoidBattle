using GreatVoidBattle.Application.Dto.Fleet;
using GreatVoidBattle.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace GreatVoidBattle.Api.Controllers;

[Route("api/game/fractions/{fractionId}/[controller]")]
[ApiController]
public class FleetController(FleetService fleetService) : ControllerBase
{
    /// <summary>
    /// Pobiera pełny stan floty frakcji wraz z układami planetarnymi
    /// </summary>
    [HttpGet]
    [ProducesResponseType<FractionFleetDto>(200)]
    public async Task<IActionResult> GetFractionFleet(string fractionId)
    {
        var result = await fleetService.GetFractionFleetAsync(fractionId);
        return Ok(result);
    }
    
    #region Ships
    
    /// <summary>
    /// Pobiera listę statków we flocie frakcji
    /// </summary>
    [HttpGet("ships")]
    [ProducesResponseType<List<FleetShipDto>>(200)]
    public async Task<IActionResult> GetFleetShips(string fractionId)
    {
        var result = await fleetService.GetFleetShipsAsync(fractionId);
        return Ok(result);
    }
    
    /// <summary>
    /// Pobiera szczegóły statku z floty
    /// </summary>
    [HttpGet("ships/{shipId}")]
    [ProducesResponseType<FleetShipDto>(200)]
    public async Task<IActionResult> GetFleetShip(string fractionId, string shipId)
    {
        var result = await fleetService.GetFleetShipAsync(fractionId, shipId);
        if (result == null) return NotFound();
        return Ok(result);
    }
    
    /// <summary>
    /// Dodaje nowy statek do floty frakcji
    /// </summary>
    [HttpPost("ships")]
    [ProducesResponseType<FleetShipDto>(201)]
    public async Task<IActionResult> CreateFleetShip(string fractionId, [FromBody] CreateFleetShipDto dto)
    {
        var result = await fleetService.CreateFleetShipAsync(fractionId, dto);
        return CreatedAtAction(nameof(GetFleetShip), new { fractionId, shipId = result.Id }, result);
    }
    
    /// <summary>
    /// Aktualizuje statek we flocie
    /// </summary>
    [HttpPut("ships/{shipId}")]
    [ProducesResponseType<FleetShipDto>(200)]
    public async Task<IActionResult> UpdateFleetShip(string fractionId, string shipId, [FromBody] UpdateFleetShipDto dto)
    {
        var result = await fleetService.UpdateFleetShipAsync(fractionId, shipId, dto);
        if (result == null) return NotFound();
        return Ok(result);
    }
    
    /// <summary>
    /// Usuwa statek z floty
    /// </summary>
    [HttpDelete("ships/{shipId}")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteFleetShip(string fractionId, string shipId)
    {
        var result = await fleetService.DeleteFleetShipAsync(fractionId, shipId);
        if (!result) return NotFound();
        return NoContent();
    }
    
    /// <summary>
    /// Przypisuje statek do układu planetarnego
    /// </summary>
    [HttpPatch("ships/{shipId}/assign")]
    [ProducesResponseType<FleetShipDto>(200)]
    public async Task<IActionResult> AssignShipToSystem(string fractionId, string shipId, [FromBody] AssignShipToSystemDto dto)
    {
        var result = await fleetService.AssignShipToSystemAsync(fractionId, shipId, dto);
        if (result == null) return NotFound();
        return Ok(result);
    }
    
    #endregion
    
    #region Planetary Systems
    
    /// <summary>
    /// Pobiera listę układów planetarnych frakcji
    /// </summary>
    [HttpGet("systems")]
    [ProducesResponseType<List<PlanetarySystemDto>>(200)]
    public async Task<IActionResult> GetPlanetarySystems(string fractionId)
    {
        var result = await fleetService.GetPlanetarySystemsAsync(fractionId);
        return Ok(result);
    }
    
    /// <summary>
    /// Pobiera szczegóły układu planetarnego
    /// </summary>
    [HttpGet("systems/{systemId}")]
    [ProducesResponseType<PlanetarySystemDto>(200)]
    public async Task<IActionResult> GetPlanetarySystem(string fractionId, string systemId)
    {
        var result = await fleetService.GetPlanetarySystemAsync(fractionId, systemId);
        if (result == null) return NotFound();
        return Ok(result);
    }
    
    /// <summary>
    /// Tworzy nowy układ planetarny
    /// </summary>
    [HttpPost("systems")]
    [ProducesResponseType<PlanetarySystemDto>(201)]
    public async Task<IActionResult> CreatePlanetarySystem(string fractionId, [FromBody] CreatePlanetarySystemDto dto)
    {
        var result = await fleetService.CreatePlanetarySystemAsync(fractionId, dto);
        return CreatedAtAction(nameof(GetPlanetarySystem), new { fractionId, systemId = result.Id }, result);
    }
    
    /// <summary>
    /// Aktualizuje układ planetarny
    /// </summary>
    [HttpPut("systems/{systemId}")]
    [ProducesResponseType<PlanetarySystemDto>(200)]
    public async Task<IActionResult> UpdatePlanetarySystem(string fractionId, string systemId, [FromBody] UpdatePlanetarySystemDto dto)
    {
        var result = await fleetService.UpdatePlanetarySystemAsync(fractionId, systemId, dto);
        if (result == null) return NotFound();
        return Ok(result);
    }
    
    /// <summary>
    /// Usuwa układ planetarny
    /// </summary>
    [HttpDelete("systems/{systemId}")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeletePlanetarySystem(string fractionId, string systemId)
    {
        var result = await fleetService.DeletePlanetarySystemAsync(fractionId, systemId);
        if (!result) return NotFound();
        return NoContent();
    }
    
    #endregion
}

