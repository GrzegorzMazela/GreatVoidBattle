namespace GreatVoidBattle.Application.Dto.Battles;

public class LaserShotDto
{
    public Guid LaserId { get; set; }
    public Guid ShipId { get; set; }
    public Guid TargetId { get; set; }
    public string ShipName { get; set; } = string.Empty;
    public string TargetName { get; set; } = string.Empty;
}
