import apiClient from './apiClient';

/**
 * API dla modułu eksploracji galaktyki
 */
export const explorationApi = {
  // ==================== UKŁADY GWIEZDNE ====================
  
  /**
   * Pobierz wszystkie układy gwiezdne
   */
  getAllSystems: async () => {
    const response = await apiClient.get('/api/exploration/systems');
    return response.data;
  },

  /**
   * Pobierz szczegóły układu
   */
  getSystem: async (systemId) => {
    const response = await apiClient.get(`/api/exploration/systems/${systemId}`);
    return response.data;
  },

  /**
   * Aktualizuj dane układu (admin)
   */
  updateSystem: async (systemId, data) => {
    const response = await apiClient.put(`/api/exploration/systems/${systemId}`, data);
    return response.data;
  },

  // ==================== EKSPLORACJA FRAKCJI ====================

  /**
   * Pobierz znane układy dla frakcji
   */
  getKnownSystems: async (fractionId) => {
    const response = await apiClient.get(`/api/exploration/fraction/${fractionId}/known-systems`);
    return response.data;
  },

  /**
   * Pobierz status eksploracji dla frakcji
   */
  getExplorationStatus: async (fractionId) => {
    const response = await apiClient.get(`/api/exploration/fraction/${fractionId}/status`);
    return response.data;
  },

  /**
   * Wyślij ekspedycję do układu
   */
  sendExpedition: async (fractionId, systemId) => {
    const response = await apiClient.post(`/api/exploration/fraction/${fractionId}/expeditions`, {
      systemId
    });
    return response.data;
  },

  /**
   * Anuluj ekspedycję
   */
  cancelExpedition: async (fractionId, expeditionId) => {
    await apiClient.delete(`/api/exploration/fraction/${fractionId}/expeditions/${expeditionId}`);
  },

  /**
   * Pobierz aktywne ekspedycje frakcji
   */
  getPendingExpeditions: async (fractionId) => {
    const response = await apiClient.get(`/api/exploration/fraction/${fractionId}/expeditions`);
    return response.data;
  },

  /**
   * Pobierz historię odkryć frakcji
   */
  getExplorationHistory: async (fractionId) => {
    const response = await apiClient.get(`/api/exploration/fraction/${fractionId}/history`);
    return response.data;
  },

  // ==================== SLOTY BADAWCZE ====================

  /**
   * Pobierz sloty badawcze frakcji
   */
  getResearchSlots: async (fractionId) => {
    const response = await apiClient.get(`/api/exploration/fraction/${fractionId}/research-slots`);
    return response.data;
  },

  /**
   * Ustaw sloty badawcze frakcji (admin)
   */
  setResearchSlots: async (fractionId, slots) => {
    await apiClient.put(`/api/exploration/admin/fraction/${fractionId}/research-slots`, { slots });
  },

  // ==================== ADMIN ====================

  /**
   * Pobierz wszystkie aktywne ekspedycje (admin)
   */
  getAllPendingExpeditions: async () => {
    const response = await apiClient.get('/api/exploration/admin/pending-expeditions');
    return response.data;
  },

  /**
   * Rozpatrz ekspedycję - wyślij wynik do frakcji (admin)
   */
  resolveExpedition: async (expeditionId, resolution) => {
    const response = await apiClient.post(`/api/exploration/admin/expeditions/${expeditionId}/resolve`, resolution);
    return response.data;
  },

  /**
   * Odrzuć ekspedycję (admin)
   */
  rejectExpedition: async (expeditionId, reason) => {
    await apiClient.delete(`/api/exploration/admin/expeditions/${expeditionId}`, {
      data: { reason }
    });
  },

  /**
   * Wyślij informację o układzie do frakcji (admin)
   */
  sendSystemInfo: async (systemId, fractionId, data) => {
    const response = await apiClient.post(`/api/exploration/admin/systems/${systemId}/send-info`, {
      fractionId,
      ...data
    });
    return response.data;
  },

  /**
   * Pobierz notatki admina o układzie
   */
  getSystemNotes: async (systemId) => {
    const response = await apiClient.get(`/api/exploration/admin/systems/${systemId}/notes`);
    return response.data;
  },

  /**
   * Zapisz notatki admina o układzie
   */
  saveSystemNotes: async (systemId, notes) => {
    await apiClient.put(`/api/exploration/admin/systems/${systemId}/notes`, notes);
  },

  /**
   * Pobierz status odkrycia układu per frakcja (admin)
   */
  getSystemDiscoveryStatus: async (systemId) => {
    const response = await apiClient.get(`/api/exploration/admin/systems/${systemId}/discovery-status`);
    return response.data;
  },

  /**
   * Pobierz wszystkie sloty badawcze (admin)
   */
  getAllResearchSlots: async () => {
    const response = await apiClient.get('/api/exploration/admin/research-slots');
    return response.data;
  },

  /**
   * Zakończ turę eksploracji (admin)
   */
  endExplorationTurn: async () => {
    await apiClient.post('/api/exploration/admin/end-turn');
  }
};

export default explorationApi;

