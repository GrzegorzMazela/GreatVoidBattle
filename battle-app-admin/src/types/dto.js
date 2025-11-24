export const ShipTypes = ['Corvette','Destroyer','Cruiser','Battleship','SuperBattleship','OrbitalFort'];

export function emptyBattlePayload() {
  return { name: '', width: 500, height: 500 };
}

export function emptyShipPayload() {
  return { name: '', type: 'Corvette', positionX: 0, positionY: 0 };
}
