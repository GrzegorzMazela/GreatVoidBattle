import apiClient from './apiClient';

export const gameStateApi = {
  // Technologies
  getAllTechnologies: async () => {
    const response = await apiClient.get('/api/technologies');
    return response.data;
  },

  getTechnologiesByTier: async (tier) => {
    const response = await apiClient.get(`/api/technologies/tier/${tier}`);
    return response.data;
  },

  assignTechnology: async (request) => {
    await apiClient.post('/api/technologies/assign', request);
  },

  removeTechnology: async (fractionId, technologyId) => {
    await apiClient.delete(`/api/technologies/fraction/${fractionId}/technology/${technologyId}`);
  },

  // Game State
  getActiveSession: async () => {
    const response = await apiClient.get('/api/gamestate/session');
    return response.data;
  },

  createSession: async (name, fractionIds) => {
    const response = await apiClient.post('/api/gamestate/session', { name, fractionIds });
    return response.data;
  },

  getFractionState: async (fractionId) => {
    const response = await apiClient.get(`/api/gamestate/fraction/${fractionId}`);
    return response.data;
  },
  
  getAllFractionStates: async () => {
    const response = await apiClient.get('/api/gamestate/admin/fractions');
    return response.data;
  },

  getAvailableTechnologies: async (fractionId) => {
    const response = await apiClient.get(`/api/gamestate/fraction/${fractionId}/technologies`);
    return response.data;
  },

  advanceTier: async (fractionId) => {
    await apiClient.post(`/api/gamestate/fraction/${fractionId}/advance-tier`);
  },
  
  // Admin: Set research slots
  setResearchSlots: async (fractionId, slots) => {
    await apiClient.put(`/api/gamestate/admin/fraction/${fractionId}/research-slots`, { slots });
  },

  // Research Requests (for players)
  requestResearch: async (fractionId, technologyId) => {
    await apiClient.post(`/api/gamestate/fraction/${fractionId}/research-request`, {
      technologyId
    });
  },
  
  cancelResearchRequest: async (fractionId, technologyId) => {
    await apiClient.delete(`/api/gamestate/fraction/${fractionId}/research-request/${technologyId}`);
  },

  getPendingRequests: async (fractionId) => {
    const response = await apiClient.get(`/api/gamestate/fraction/${fractionId}/research-requests`);
    return response.data;
  },

  // Turn Management (for admin)
  getAllPendingRequests: async () => {
    const response = await apiClient.get('/api/gamestate/admin/pending-requests');
    return response.data;
  },

  endTurn: async (resolutions) => {
    await apiClient.post('/api/gamestate/admin/end-turn', resolutions);
  }
};

