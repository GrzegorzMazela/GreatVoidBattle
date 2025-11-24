namespace GreatVoidBattle.Application.Dto.Battles;

public record BattleDto(
    Guid BattleId, 
    string Name, 
    DateTime CreatedAt, 
    string Status,
    int TurnNumber,
    int Width,
    int Height,
    IEnumerable<string> Fractions
);