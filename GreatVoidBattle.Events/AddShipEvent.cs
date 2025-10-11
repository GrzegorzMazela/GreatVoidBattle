using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace GreatVoidBattle.Events;
internal class AddShipEvent
{
    public Guid ShipId { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public ShipType Type { get; set; }

    public double PositionX { get; set; }
    public double PositionY { get; set; }

}
