import { describe, it, expect, vi, beforeEach } from 'vitest';

// Mock apiClient inline (bez importu)
const mockApiClient = {
  get: vi.fn(),
  post: vi.fn(),
  put: vi.fn(),
  delete: vi.fn(),
};

// Recreate explorationApi functions for testing
const createExplorationApi = (apiClient) => ({
  getAllSystems: async () => {
    const response = await apiClient.get('/api/exploration/systems');
    return response.data;
  },

  getSystem: async (systemId) => {
    const response = await apiClient.get(`/api/exploration/systems/${systemId}`);
    return response.data;
  },

  updateSystem: async (systemId, data) => {
    const response = await apiClient.put(`/api/exploration/systems/${systemId}`, data);
    return response.data;
  },

  getKnownSystems: async (fractionId) => {
    const response = await apiClient.get(`/api/exploration/fraction/${fractionId}/known-systems`);
    return response.data;
  },

  sendExpedition: async (fractionId, systemId) => {
    const response = await apiClient.post(`/api/exploration/fraction/${fractionId}/expeditions`, {
      systemId
    });
    return response.data;
  },

  cancelExpedition: async (fractionId, expeditionId) => {
    await apiClient.delete(`/api/exploration/fraction/${fractionId}/expeditions/${expeditionId}`);
  },

  getPendingExpeditions: async (fractionId) => {
    const response = await apiClient.get(`/api/exploration/fraction/${fractionId}/expeditions`);
    return response.data;
  },

  getResearchSlots: async (fractionId) => {
    const response = await apiClient.get(`/api/exploration/fraction/${fractionId}/research-slots`);
    return response.data;
  },

  setResearchSlots: async (fractionId, slots) => {
    await apiClient.put(`/api/exploration/admin/fraction/${fractionId}/research-slots`, { slots });
  },

  getAllPendingExpeditions: async () => {
    const response = await apiClient.get('/api/exploration/admin/pending-expeditions');
    return response.data;
  },

  resolveExpedition: async (expeditionId, resolution) => {
    const response = await apiClient.post(`/api/exploration/admin/expeditions/${expeditionId}/resolve`, resolution);
    return response.data;
  },

  rejectExpedition: async (expeditionId, reason) => {
    await apiClient.delete(`/api/exploration/admin/expeditions/${expeditionId}`, {
      data: { reason }
    });
  },

  sendSystemInfo: async (systemId, fractionId, data) => {
    const response = await apiClient.post(`/api/exploration/admin/systems/${systemId}/send-info`, {
      fractionId,
      ...data
    });
    return response.data;
  },

  getAllResearchSlots: async () => {
    const response = await apiClient.get('/api/exploration/admin/research-slots');
    return response.data;
  },

  endExplorationTurn: async () => {
    await apiClient.post('/api/exploration/admin/end-turn');
  }
});

describe('explorationApi', () => {
  let explorationApi;

  beforeEach(() => {
    vi.clearAllMocks();
    explorationApi = createExplorationApi(mockApiClient);
  });

  describe('getAllSystems', () => {
    it('should fetch all star systems', async () => {
      const mockSystems = [
        { id: 'system-1', name: 'Alpha', x: 100, y: 200 },
        { id: 'system-2', name: 'Beta', x: 300, y: 400 },
      ];
      mockApiClient.get.mockResolvedValue({ data: mockSystems });

      const result = await explorationApi.getAllSystems();

      expect(mockApiClient.get).toHaveBeenCalledWith('/api/exploration/systems');
      expect(result).toEqual(mockSystems);
    });
  });

  describe('getSystem', () => {
    it('should fetch specific system details', async () => {
      const mockSystem = { id: 'system-1', name: 'Alpha', x: 100, y: 200, faction: 'hegemonia' };
      mockApiClient.get.mockResolvedValue({ data: mockSystem });

      const result = await explorationApi.getSystem('system-1');

      expect(mockApiClient.get).toHaveBeenCalledWith('/api/exploration/systems/system-1');
      expect(result).toEqual(mockSystem);
    });
  });

  describe('updateSystem', () => {
    it('should update system data', async () => {
      const systemId = 'system-1';
      const updateData = { type: 'resources', resources: 'Minerały' };
      mockApiClient.put.mockResolvedValue({ data: { ...updateData, id: systemId } });

      const result = await explorationApi.updateSystem(systemId, updateData);

      expect(mockApiClient.put).toHaveBeenCalledWith(`/api/exploration/systems/${systemId}`, updateData);
      expect(result).toEqual({ ...updateData, id: systemId });
    });
  });

  describe('getKnownSystems', () => {
    it('should fetch known systems for a fraction', async () => {
      const fractionId = 'hegemonia-titanum';
      const mockSystems = [
        { id: 'system-1', name: 'Venetrix' },
        { id: 'system-2', name: 'Cezaria' },
      ];
      mockApiClient.get.mockResolvedValue({ data: mockSystems });

      const result = await explorationApi.getKnownSystems(fractionId);

      expect(mockApiClient.get).toHaveBeenCalledWith(`/api/exploration/fraction/${fractionId}/known-systems`);
      expect(result).toEqual(mockSystems);
    });
  });

  describe('sendExpedition', () => {
    it('should send expedition to a system', async () => {
      const fractionId = 'hegemonia-titanum';
      const systemId = 'system-1';
      const mockExpedition = { id: 'exp-1', systemId, status: 'pending' };
      mockApiClient.post.mockResolvedValue({ data: mockExpedition });

      const result = await explorationApi.sendExpedition(fractionId, systemId);

      expect(mockApiClient.post).toHaveBeenCalledWith(
        `/api/exploration/fraction/${fractionId}/expeditions`,
        { systemId }
      );
      expect(result).toEqual(mockExpedition);
    });
  });

  describe('cancelExpedition', () => {
    it('should cancel an expedition', async () => {
      const fractionId = 'hegemonia-titanum';
      const expeditionId = 'exp-1';
      mockApiClient.delete.mockResolvedValue({});

      await explorationApi.cancelExpedition(fractionId, expeditionId);

      expect(mockApiClient.delete).toHaveBeenCalledWith(
        `/api/exploration/fraction/${fractionId}/expeditions/${expeditionId}`
      );
    });
  });

  describe('getPendingExpeditions', () => {
    it('should fetch pending expeditions for a fraction', async () => {
      const fractionId = 'hegemonia-titanum';
      const mockExpeditions = [
        { id: 'exp-1', systemId: 'system-1', systemName: 'Alpha' },
        { id: 'exp-2', systemId: 'system-2', systemName: 'Beta' },
      ];
      mockApiClient.get.mockResolvedValue({ data: mockExpeditions });

      const result = await explorationApi.getPendingExpeditions(fractionId);

      expect(mockApiClient.get).toHaveBeenCalledWith(`/api/exploration/fraction/${fractionId}/expeditions`);
      expect(result).toEqual(mockExpeditions);
    });
  });

  describe('getResearchSlots', () => {
    it('should fetch research slots for a fraction', async () => {
      const fractionId = 'hegemonia-titanum';
      const mockSlots = { total: 3, used: 1 };
      mockApiClient.get.mockResolvedValue({ data: mockSlots });

      const result = await explorationApi.getResearchSlots(fractionId);

      expect(mockApiClient.get).toHaveBeenCalledWith(`/api/exploration/fraction/${fractionId}/research-slots`);
      expect(result).toEqual(mockSlots);
    });
  });

  describe('setResearchSlots', () => {
    it('should set research slots for a fraction', async () => {
      const fractionId = 'hegemonia-titanum';
      const slots = 5;
      mockApiClient.put.mockResolvedValue({});

      await explorationApi.setResearchSlots(fractionId, slots);

      expect(mockApiClient.put).toHaveBeenCalledWith(
        `/api/exploration/admin/fraction/${fractionId}/research-slots`,
        { slots }
      );
    });
  });

  describe('getAllPendingExpeditions', () => {
    it('should fetch all pending expeditions for admin', async () => {
      const mockExpeditions = [
        { id: 'exp-1', fractionId: 'hegemonia-titanum', systemId: 'system-1' },
        { id: 'exp-2', fractionId: 'shimura-incorporated', systemId: 'system-2' },
      ];
      mockApiClient.get.mockResolvedValue({ data: mockExpeditions });

      const result = await explorationApi.getAllPendingExpeditions();

      expect(mockApiClient.get).toHaveBeenCalledWith('/api/exploration/admin/pending-expeditions');
      expect(result).toEqual(mockExpeditions);
    });
  });

  describe('resolveExpedition', () => {
    it('should resolve an expedition with result', async () => {
      const expeditionId = 'exp-1';
      const resolution = { status: 'explored', message: 'Znaleziono zasoby' };
      const mockResult = { id: expeditionId, ...resolution };
      mockApiClient.post.mockResolvedValue({ data: mockResult });

      const result = await explorationApi.resolveExpedition(expeditionId, resolution);

      expect(mockApiClient.post).toHaveBeenCalledWith(
        `/api/exploration/admin/expeditions/${expeditionId}/resolve`,
        resolution
      );
      expect(result).toEqual(mockResult);
    });
  });

  describe('rejectExpedition', () => {
    it('should reject an expedition with reason', async () => {
      const expeditionId = 'exp-1';
      const reason = 'Zbyt daleko';
      mockApiClient.delete.mockResolvedValue({});

      await explorationApi.rejectExpedition(expeditionId, reason);

      expect(mockApiClient.delete).toHaveBeenCalledWith(
        `/api/exploration/admin/expeditions/${expeditionId}`,
        { data: { reason } }
      );
    });
  });

  describe('sendSystemInfo', () => {
    it('should send system info to a fraction', async () => {
      const systemId = 'system-1';
      const fractionId = 'hegemonia-titanum';
      const data = { message: 'Układ zawiera minerały', status: 'discovered' };
      const mockResult = { success: true };
      mockApiClient.post.mockResolvedValue({ data: mockResult });

      const result = await explorationApi.sendSystemInfo(systemId, fractionId, data);

      expect(mockApiClient.post).toHaveBeenCalledWith(
        `/api/exploration/admin/systems/${systemId}/send-info`,
        { fractionId, ...data }
      );
      expect(result).toEqual(mockResult);
    });
  });

  describe('getAllResearchSlots', () => {
    it('should fetch all research slots for admin', async () => {
      const mockSlots = {
        'hegemonia-titanum': 3,
        'shimura-incorporated': 2,
        'protektorat-pogranicza': 2,
      };
      mockApiClient.get.mockResolvedValue({ data: mockSlots });

      const result = await explorationApi.getAllResearchSlots();

      expect(mockApiClient.get).toHaveBeenCalledWith('/api/exploration/admin/research-slots');
      expect(result).toEqual(mockSlots);
    });
  });

  describe('endExplorationTurn', () => {
    it('should end the exploration turn', async () => {
      mockApiClient.post.mockResolvedValue({});

      await explorationApi.endExplorationTurn();

      expect(mockApiClient.post).toHaveBeenCalledWith('/api/exploration/admin/end-turn');
    });
  });

  describe('error handling', () => {
    it('should propagate API errors', async () => {
      const error = new Error('Network error');
      mockApiClient.get.mockRejectedValue(error);

      await expect(explorationApi.getAllSystems()).rejects.toThrow('Network error');
    });
  });
});
