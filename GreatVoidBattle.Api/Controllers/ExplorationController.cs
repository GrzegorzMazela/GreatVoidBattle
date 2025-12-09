using GreatVoidBattle.Application.Dto.Exploration;
using GreatVoidBattle.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace GreatVoidBattle.Api.Controllers;

[Route("api/exploration")]
[ApiController]
public class ExplorationController : ControllerBase
{
    private readonly ExplorationService _explorationService;

    public ExplorationController(ExplorationService explorationService)
    {
        _explorationService = explorationService;
    }

    #region Star Systems

    /// <summary>
    /// Pobierz wszystkie układy gwiezdne (admin)
    /// </summary>
    [HttpGet("systems")]
    [ProducesResponseType<List<StarSystemAdminDto>>(200)]
    public async Task<IActionResult> GetAllSystems()
    {
        var systems = await _explorationService.GetAllSystemsAsync();
        return Ok(systems);
    }

    /// <summary>
    /// Pobierz szczegóły układu (admin)
    /// </summary>
    [HttpGet("systems/{systemId}")]
    [ProducesResponseType<StarSystemAdminDto>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetSystem(string systemId)
    {
        var system = await _explorationService.GetSystemAsync(systemId);
        if (system == null)
            return NotFound();
        return Ok(system);
    }

    /// <summary>
    /// Utwórz nowy układ gwiezdny (admin)
    /// </summary>
    [HttpPost("systems")]
    [ProducesResponseType<StarSystemAdminDto>(201)]
    public async Task<IActionResult> CreateSystem([FromBody] CreateStarSystemDto dto)
    {
        var system = await _explorationService.CreateSystemAsync(dto);
        return CreatedAtAction(nameof(GetSystem), new { systemId = system.Id }, system);
    }

    /// <summary>
    /// Aktualizuj dane układu (admin)
    /// </summary>
    [HttpPut("systems/{systemId}")]
    [ProducesResponseType<StarSystemAdminDto>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateSystem(string systemId, [FromBody] UpdateStarSystemDto dto)
    {
        var system = await _explorationService.UpdateSystemAsync(systemId, dto);
        if (system == null)
            return NotFound();
        return Ok(system);
    }

    #endregion

    #region Fraction Exploration

    /// <summary>
    /// Pobierz znane układy dla frakcji
    /// </summary>
    [HttpGet("fraction/{fractionId}/known-systems")]
    [ProducesResponseType<List<SystemDiscoveryDto>>(200)]
    public async Task<IActionResult> GetKnownSystems(string fractionId)
    {
        var systems = await _explorationService.GetKnownSystemsAsync(fractionId);
        return Ok(systems);
    }

    /// <summary>
    /// Pobierz status eksploracji dla frakcji
    /// </summary>
    [HttpGet("fraction/{fractionId}/status")]
    [ProducesResponseType<ExplorationStatusDto>(200)]
    public async Task<IActionResult> GetExplorationStatus(string fractionId)
    {
        var status = await _explorationService.GetExplorationStatusAsync(fractionId);
        return Ok(status);
    }

    /// <summary>
    /// Wyślij ekspedycję do układu
    /// </summary>
    [HttpPost("fraction/{fractionId}/expeditions")]
    [ProducesResponseType<ExpeditionDto>(201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> SendExpedition(string fractionId, [FromBody] SendExpeditionDto dto)
    {
        var expedition = await _explorationService.SendExpeditionAsync(fractionId, dto);
        if (expedition == null)
            return BadRequest("Nie można wysłać ekspedycji - sprawdź dostępne sloty lub poprawność danych");
        return CreatedAtAction(nameof(GetPendingExpeditions), new { fractionId }, expedition);
    }

    /// <summary>
    /// Anuluj ekspedycję
    /// </summary>
    [HttpDelete("fraction/{fractionId}/expeditions/{expeditionId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CancelExpedition(string fractionId, string expeditionId)
    {
        var success = await _explorationService.CancelExpeditionAsync(fractionId, expeditionId);
        if (!success)
            return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Pobierz aktywne ekspedycje frakcji
    /// </summary>
    [HttpGet("fraction/{fractionId}/expeditions")]
    [ProducesResponseType<List<ExpeditionDto>>(200)]
    public async Task<IActionResult> GetPendingExpeditions(string fractionId)
    {
        var expeditions = await _explorationService.GetPendingExpeditionsAsync(fractionId);
        return Ok(expeditions);
    }

    /// <summary>
    /// Pobierz historię odkryć frakcji
    /// </summary>
    [HttpGet("fraction/{fractionId}/history")]
    [ProducesResponseType<ExplorationHistoryDto>(200)]
    public async Task<IActionResult> GetExplorationHistory(string fractionId)
    {
        var history = await _explorationService.GetExplorationHistoryAsync(fractionId);
        return Ok(history);
    }

    /// <summary>
    /// Pobierz sloty badawcze frakcji
    /// </summary>
    [HttpGet("fraction/{fractionId}/research-slots")]
    [ProducesResponseType<FractionResearchSlotsDto>(200)]
    public async Task<IActionResult> GetResearchSlots(string fractionId)
    {
        var slots = await _explorationService.GetResearchSlotsAsync(fractionId);
        return Ok(slots);
    }

    #endregion

    #region Admin Operations

    /// <summary>
    /// Ustaw sloty badawcze frakcji (admin)
    /// </summary>
    [HttpPut("admin/fraction/{fractionId}/research-slots")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> SetResearchSlots(string fractionId, [FromBody] SetResearchSlotsDto dto)
    {
        var success = await _explorationService.SetResearchSlotsAsync(fractionId, dto.Slots);
        if (!success)
            return BadRequest("Nie można ustawić slotów badawczych");
        return Ok();
    }

    /// <summary>
    /// Pobierz wszystkie aktywne ekspedycje (admin)
    /// </summary>
    [HttpGet("admin/pending-expeditions")]
    [ProducesResponseType<List<ExpeditionAdminDto>>(200)]
    public async Task<IActionResult> GetAllPendingExpeditions()
    {
        var expeditions = await _explorationService.GetAllPendingExpeditionsAsync();
        return Ok(expeditions);
    }

    /// <summary>
    /// Rozpatrz ekspedycję - wyślij wynik do frakcji (admin)
    /// </summary>
    [HttpPost("admin/expeditions/{expeditionId}/resolve")]
    [ProducesResponseType<ExpeditionAdminDto>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ResolveExpedition(string expeditionId, [FromBody] ResolveExpeditionDto dto)
    {
        var result = await _explorationService.ResolveExpeditionAsync(expeditionId, dto);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Odrzuć ekspedycję (admin)
    /// </summary>
    [HttpDelete("admin/expeditions/{expeditionId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> RejectExpedition(string expeditionId, [FromBody] RejectExpeditionRequest request)
    {
        var success = await _explorationService.RejectExpeditionAsync(expeditionId, request.Reason);
        if (!success)
            return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Wyślij informację o układzie do frakcji (admin)
    /// </summary>
    [HttpPost("admin/systems/{systemId}/send-info")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> SendSystemInfo(string systemId, [FromBody] SendSystemInfoDto dto)
    {
        var success = await _explorationService.SendSystemInfoAsync(systemId, dto);
        if (!success)
            return NotFound();
        return Ok(new { success = true });
    }

    /// <summary>
    /// Pobierz notatki admina o układzie
    /// </summary>
    [HttpGet("admin/systems/{systemId}/notes")]
    [ProducesResponseType<SystemNotesDto>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetSystemNotes(string systemId)
    {
        var notes = await _explorationService.GetSystemNotesAsync(systemId);
        if (notes == null)
            return NotFound();
        return Ok(notes);
    }

    /// <summary>
    /// Zapisz notatki admina o układzie
    /// </summary>
    [HttpPut("admin/systems/{systemId}/notes")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> SaveSystemNotes(string systemId, [FromBody] SystemNotesDto dto)
    {
        var success = await _explorationService.SaveSystemNotesAsync(systemId, dto);
        if (!success)
            return NotFound();
        return Ok();
    }

    /// <summary>
    /// Pobierz status odkrycia układu per frakcja (admin)
    /// </summary>
    [HttpGet("admin/systems/{systemId}/discovery-status")]
    [ProducesResponseType<SystemDiscoveryStatusDto>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetSystemDiscoveryStatus(string systemId)
    {
        var status = await _explorationService.GetSystemDiscoveryStatusAsync(systemId);
        if (status == null)
            return NotFound();
        return Ok(status);
    }

    /// <summary>
    /// Pobierz wszystkie sloty badawcze (admin)
    /// </summary>
    [HttpGet("admin/research-slots")]
    [ProducesResponseType<AllResearchSlotsDto>(200)]
    public async Task<IActionResult> GetAllResearchSlots()
    {
        var slots = await _explorationService.GetAllResearchSlotsAsync();
        return Ok(slots);
    }

    /// <summary>
    /// Zakończ turę eksploracji (admin)
    /// </summary>
    [HttpPost("admin/end-turn")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> EndExplorationTurn()
    {
        await _explorationService.EndExplorationTurnAsync();
        return Ok();
    }

    #endregion
}

/// <summary>
/// Request do odrzucenia ekspedycji
/// </summary>
public class RejectExpeditionRequest
{
    public string Reason { get; set; } = string.Empty;
}

