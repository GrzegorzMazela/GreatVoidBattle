namespace GreatVoidBattle.Application.Dto.Battles;

public class CreateBattleDto
{
    public string Name { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
}