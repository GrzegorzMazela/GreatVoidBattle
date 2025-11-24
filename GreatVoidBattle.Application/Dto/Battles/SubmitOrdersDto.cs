namespace GreatVoidBattle.Application.Dto.Battles;

public class SubmitOrdersDto
{
    public int TurnNumber { get; set; }
    public List<OrderDto> Orders { get; set; } = new();
}
