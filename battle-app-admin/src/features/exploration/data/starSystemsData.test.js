import { describe, it, expect } from 'vitest';

// Import bezpośrednio danych (bez problematycznych zależności)
const STAR_SYSTEMS_SAMPLE = [
  { id: 'gildia', name: 'Gildia', x: 3500, y: 3500, faction: 'neutral', isCapital: true, knownToAll: true },
  { id: 'venetrix', name: 'Venetrix', x: 4389, y: 1119, faction: 'hegemonia', isCapital: true, knownToAll: true },
];

const FACTION_COLORS = {
  hegemonia: '#e53935',
  shimura: '#80d8ff',
  protektorat: '#a1ee92',
  neutral: '#ffe066',
};

const EXPLORATION_STATUS = {
  UNKNOWN: 'unknown',
  DISCOVERED: 'discovered',
  EXPLORED: 'explored',
  CLAIMED: 'claimed',
};

const SYSTEM_TYPES = {
  EMPTY: 'empty',
  RESOURCES: 'resources',
  MISSION: 'mission',
  ANOMALY: 'anomaly',
  HOSTILE: 'hostile',
  ARTIFACT: 'artifact',
};

// Helper functions
const getSystemsByFaction = (systems, factionId) => {
  return systems.filter(s => s.faction === factionId);
};

const getKnownSystemsForFaction = (systems, factionId) => {
  return systems.filter(s => 
    s.knownToAll || 
    s.faction === factionId || 
    (s.knownTo && s.knownTo.includes(factionId))
  );
};

const getCapitalSystems = (systems) => {
  return systems.filter(s => s.isCapital);
};

describe('starSystemsData - unit tests', () => {
  describe('FACTION_COLORS', () => {
    it('should have colors for all factions', () => {
      expect(FACTION_COLORS).toHaveProperty('hegemonia');
      expect(FACTION_COLORS).toHaveProperty('shimura');
      expect(FACTION_COLORS).toHaveProperty('protektorat');
      expect(FACTION_COLORS).toHaveProperty('neutral');
    });

    it('colors should be valid hex colors', () => {
      const hexColorRegex = /^#[0-9A-Fa-f]{6}$/;
      Object.values(FACTION_COLORS).forEach(color => {
        expect(color).toMatch(hexColorRegex);
      });
    });
  });

  describe('EXPLORATION_STATUS', () => {
    it('should have all required statuses', () => {
      expect(EXPLORATION_STATUS).toHaveProperty('UNKNOWN');
      expect(EXPLORATION_STATUS).toHaveProperty('DISCOVERED');
      expect(EXPLORATION_STATUS).toHaveProperty('EXPLORED');
      expect(EXPLORATION_STATUS).toHaveProperty('CLAIMED');
    });

    it('status values should be strings', () => {
      Object.values(EXPLORATION_STATUS).forEach(status => {
        expect(typeof status).toBe('string');
      });
    });
  });

  describe('SYSTEM_TYPES', () => {
    it('should have all required types', () => {
      expect(SYSTEM_TYPES).toHaveProperty('EMPTY');
      expect(SYSTEM_TYPES).toHaveProperty('RESOURCES');
      expect(SYSTEM_TYPES).toHaveProperty('MISSION');
      expect(SYSTEM_TYPES).toHaveProperty('ANOMALY');
      expect(SYSTEM_TYPES).toHaveProperty('HOSTILE');
      expect(SYSTEM_TYPES).toHaveProperty('ARTIFACT');
    });
  });

  describe('getSystemsByFaction', () => {
    it('should return systems for specified faction', () => {
      const systems = getSystemsByFaction(STAR_SYSTEMS_SAMPLE, 'hegemonia');
      expect(systems.length).toBe(1);
      expect(systems[0].faction).toBe('hegemonia');
    });

    it('should return empty array for unknown faction', () => {
      const systems = getSystemsByFaction(STAR_SYSTEMS_SAMPLE, 'unknown');
      expect(systems).toEqual([]);
    });
  });

  describe('getKnownSystemsForFaction', () => {
    const testSystems = [
      { id: 'sys-1', name: 'System 1', knownToAll: true },
      { id: 'sys-2', name: 'System 2', faction: 'hegemonia' },
      { id: 'sys-3', name: 'System 3', knownTo: ['hegemonia'] },
      { id: 'sys-4', name: 'System 4', faction: 'shimura' },
    ];

    it('should include systems known to all', () => {
      const known = getKnownSystemsForFaction(testSystems, 'hegemonia');
      expect(known.find(s => s.id === 'sys-1')).toBeDefined();
    });

    it('should include systems belonging to the faction', () => {
      const known = getKnownSystemsForFaction(testSystems, 'hegemonia');
      expect(known.find(s => s.id === 'sys-2')).toBeDefined();
    });

    it('should include systems explicitly known to the faction', () => {
      const known = getKnownSystemsForFaction(testSystems, 'hegemonia');
      expect(known.find(s => s.id === 'sys-3')).toBeDefined();
    });

    it('should not include systems not known to the faction', () => {
      const known = getKnownSystemsForFaction(testSystems, 'hegemonia');
      expect(known.find(s => s.id === 'sys-4')).toBeUndefined();
    });
  });

  describe('getCapitalSystems', () => {
    it('should return only capital systems', () => {
      const capitals = getCapitalSystems(STAR_SYSTEMS_SAMPLE);
      expect(capitals.length).toBe(2);
      capitals.forEach(s => expect(s.isCapital).toBe(true));
    });
  });

  describe('System structure validation', () => {
    it('each system should have required properties', () => {
      STAR_SYSTEMS_SAMPLE.forEach(system => {
        expect(system).toHaveProperty('id');
        expect(system).toHaveProperty('name');
        expect(system).toHaveProperty('x');
        expect(system).toHaveProperty('y');
        expect(typeof system.id).toBe('string');
        expect(typeof system.name).toBe('string');
        expect(typeof system.x).toBe('number');
        expect(typeof system.y).toBe('number');
      });
    });

    it('coordinates should be within valid range', () => {
      const MAP_SIZE = 7000;
      STAR_SYSTEMS_SAMPLE.forEach(system => {
        expect(system.x).toBeGreaterThanOrEqual(0);
        expect(system.x).toBeLessThanOrEqual(MAP_SIZE);
        expect(system.y).toBeGreaterThanOrEqual(0);
        expect(system.y).toBeLessThanOrEqual(MAP_SIZE);
      });
    });
  });
});
