using GreatVoidBattle.Core.Domains.Enums;

namespace GreatVoidBattle.Application.Exceptions;

public class WrongBattleStatusException : Exception
{
    public WrongBattleStatusException(BattleStatus battleStatus) : base($"Wrong Battle Status: {battleStatus}")
    {
    }
}
