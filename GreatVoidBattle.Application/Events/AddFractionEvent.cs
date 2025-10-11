using GreatVoidBattle.Application.Events.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreatVoidBattle.Application.Events;

public class AddFractionEvent : BattleEvent
{
    public string Name { get; set; }
}