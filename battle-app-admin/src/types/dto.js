export const ShipTypes = ['Corvette','Destroyer','Cruiser','Battleship','SuperBattleship','OrbitalFort'];
export const ShipCategories = ['Combat', 'OrbitalStation', 'Research'];

// Domyślne statystyki statków według typu
export const ShipDefaultStats = {
  Corvette: { speed: 10, hitPoints: 50, shields: 25, armor: 25, modules: 1 },
  Destroyer: { speed: 8, hitPoints: 100, shields: 50, armor: 50, modules: 2 },
  Cruiser: { speed: 6, hitPoints: 200, shields: 100, armor: 100, modules: 4 },
  Battleship: { speed: 5, hitPoints: 400, shields: 200, armor: 200, modules: 8 },
  SuperBattleship: { speed: 5, hitPoints: 600, shields: 300, armor: 300, modules: 12 },
  OrbitalFort: { speed: 0, hitPoints: 100, shields: 50, armor: 50, modules: 2 }
};

export function emptyBattlePayload() {
  return { name: '', width: 500, height: 500 };
}

export function emptyShipPayload() {
  const defaults = ShipDefaultStats['Corvette'];
  return { 
    name: '', 
    type: 'Corvette', 
    positionX: 0, 
    positionY: 0,
    speed: defaults.speed,
    hitPoints: defaults.hitPoints,
    shields: defaults.shields,
    armor: defaults.armor
  };
}

// Payload dla statku w flocie frakcji
export function emptyFleetShipPayload() {
  const defaults = ShipDefaultStats['Corvette'];
  return { 
    name: '', 
    type: 'Corvette', 
    category: 'Combat',
    speed: defaults.speed,
    hitPoints: defaults.hitPoints,
    shields: defaults.shields,
    armor: defaults.armor,
    modules: Array.from({ length: defaults.modules }, () => ({
      weaponTypes: ['Missile', 'Laser', 'PointDefense']
    })),
    stationedInSystemId: null
  };
}

// Payload dla układu planetarnego
export function emptyPlanetarySystemPayload() {
  return { 
    name: '', 
    description: '' 
  };
}
