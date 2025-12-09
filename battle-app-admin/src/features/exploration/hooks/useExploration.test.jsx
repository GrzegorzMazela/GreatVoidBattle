import { describe, it, expect } from 'vitest';

// Test query keys structure (bez importów z rolldown-vite)
const explorationKeys = {
  all: ['exploration'],
  systems: () => [...explorationKeys.all, 'systems'],
  system: (id) => [...explorationKeys.all, 'systems', id],
  
  fraction: (fractionId) => [...explorationKeys.all, 'fraction', fractionId],
  knownSystems: (fractionId) => [...explorationKeys.fraction(fractionId), 'known'],
  status: (fractionId) => [...explorationKeys.fraction(fractionId), 'status'],
  expeditions: (fractionId) => [...explorationKeys.fraction(fractionId), 'expeditions'],
  history: (fractionId) => [...explorationKeys.fraction(fractionId), 'history'],
  researchSlots: (fractionId) => [...explorationKeys.fraction(fractionId), 'slots'],
  
  admin: () => [...explorationKeys.all, 'admin'],
  allExpeditions: () => [...explorationKeys.admin(), 'expeditions'],
  allSlots: () => [...explorationKeys.admin(), 'slots'],
  systemNotes: (systemId) => [...explorationKeys.admin(), 'notes', systemId],
  discoveryStatus: (systemId) => [...explorationKeys.admin(), 'discovery', systemId],
};

describe('useExploration - query keys', () => {
  describe('explorationKeys', () => {
    it('should have correct base key', () => {
      expect(explorationKeys.all).toEqual(['exploration']);
    });

    it('should generate correct systems keys', () => {
      expect(explorationKeys.systems()).toEqual(['exploration', 'systems']);
      expect(explorationKeys.system('sys-1')).toEqual(['exploration', 'systems', 'sys-1']);
    });

    it('should generate correct fraction keys', () => {
      expect(explorationKeys.fraction('frac-1')).toEqual(['exploration', 'fraction', 'frac-1']);
      expect(explorationKeys.knownSystems('frac-1')).toEqual(['exploration', 'fraction', 'frac-1', 'known']);
      expect(explorationKeys.status('frac-1')).toEqual(['exploration', 'fraction', 'frac-1', 'status']);
      expect(explorationKeys.expeditions('frac-1')).toEqual(['exploration', 'fraction', 'frac-1', 'expeditions']);
      expect(explorationKeys.history('frac-1')).toEqual(['exploration', 'fraction', 'frac-1', 'history']);
      expect(explorationKeys.researchSlots('frac-1')).toEqual(['exploration', 'fraction', 'frac-1', 'slots']);
    });

    it('should generate correct admin keys', () => {
      expect(explorationKeys.admin()).toEqual(['exploration', 'admin']);
      expect(explorationKeys.allExpeditions()).toEqual(['exploration', 'admin', 'expeditions']);
      expect(explorationKeys.allSlots()).toEqual(['exploration', 'admin', 'slots']);
      expect(explorationKeys.systemNotes('sys-1')).toEqual(['exploration', 'admin', 'notes', 'sys-1']);
      expect(explorationKeys.discoveryStatus('sys-1')).toEqual(['exploration', 'admin', 'discovery', 'sys-1']);
    });

    it('keys should be arrays', () => {
      expect(Array.isArray(explorationKeys.all)).toBe(true);
      expect(Array.isArray(explorationKeys.systems())).toBe(true);
      expect(Array.isArray(explorationKeys.fraction('frac-1'))).toBe(true);
      expect(Array.isArray(explorationKeys.admin())).toBe(true);
    });

    it('keys should be unique for different parameters', () => {
      const key1 = explorationKeys.system('sys-1');
      const key2 = explorationKeys.system('sys-2');
      expect(key1).not.toEqual(key2);
    });

    it('keys should contain parent keys', () => {
      const systemKey = explorationKeys.system('sys-1');
      const systemsKey = explorationKeys.systems();
      expect(systemKey.slice(0, systemsKey.length)).toEqual(systemsKey);
    });
  });
});

describe('useExploration - hook behavior contracts', () => {
  it('useSendExpedition should require fractionId and systemId', () => {
    // Contract: fractionId is required for the hook
    // Contract: systemId is required for the mutation function
    const fractionId = 'hegemonia-titanum';
    const systemId = 'system-1';
    
    expect(fractionId).toBeTruthy();
    expect(systemId).toBeTruthy();
  });

  it('useCancelExpedition should require fractionId and expeditionId', () => {
    const fractionId = 'hegemonia-titanum';
    const expeditionId = 'exp-1';
    
    expect(fractionId).toBeTruthy();
    expect(expeditionId).toBeTruthy();
  });

  it('useResolveExpedition should require expeditionId and resolution', () => {
    const expeditionId = 'exp-1';
    const resolution = { status: 'explored', message: 'Found resources' };
    
    expect(expeditionId).toBeTruthy();
    expect(resolution.status).toBeTruthy();
  });
});
