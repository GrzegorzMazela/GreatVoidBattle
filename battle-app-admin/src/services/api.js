import apiClient from './apiClient';

// Re-export the client for backwards compatibility
export const api = apiClient;

// Battles
export const listBattles = async () => (await apiClient.get('/api/battles')).data;
export const getBattle = async (id) => (await apiClient.get(`/api/battles/${id}`)).data;
export const getBattleAdmin = async (id) => (await apiClient.get(`/api/battles/${id}/admin`)).data;
export const createBattle = async (payload) => (await apiClient.post('/api/battles', payload)).data;
export const deleteBattle = async (id) => (await apiClient.delete(`/api/battles/${id}`)).data;
export const startBattle = async (id) => (await apiClient.post(`/api/battles/${id}/start`)).data;

// Fractions
export const listFractions = async (battleId) => (await apiClient.get(`/api/battles/${battleId}/fractions`)).data;
export const listFractionsAdmin = async (battleId) => (await apiClient.get(`/api/battles/${battleId}/fractions/admin`)).data;
export const getFraction = async (battleId, fractionId) => (await apiClient.get(`/api/battles/${battleId}/fractions/${fractionId}`)).data;
export const createFraction = async (battleId, payload) => (await apiClient.post(`/api/battles/${battleId}/fractions`, payload)).data;
export const updateFraction = async (battleId, fractionId, payload) => (await apiClient.put(`/api/battles/${battleId}/fractions/${fractionId}`, payload)).data;

// Ships
export const listShips = async (battleId, fractionId) => (await apiClient.get(`/api/battles/${battleId}/fractions/${fractionId}/ships`)).data;
export const getShip = async (battleId, fractionId, shipId) => (await apiClient.get(`/api/battles/${battleId}/fractions/${fractionId}/ships/${shipId}`)).data;
export const createShip = async (battleId, fractionId, payload) => (await apiClient.post(`/api/battles/${battleId}/fractions/${fractionId}/ships`, payload)).data;
export const updateShip = async (battleId, fractionId, shipId, payload) => (await apiClient.put(`/api/battles/${battleId}/fractions/${fractionId}/ships/${shipId}`, payload)).data;
export const deleteShip = async (battleId, fractionId, shipId) => (await apiClient.delete(`/api/battles/${battleId}/fractions/${fractionId}/ships/${shipId}`)).data;
export const setShipPosition = async (battleId, fractionId, shipId, x, y) => (await apiClient.patch(`/api/battles/${battleId}/fractions/${fractionId}/ships/${shipId}/position`, { x, y })).data;

// Battle simulation - token passed via header by interceptor
export const submitOrders = async (battleId, fractionId, payload, token) => {
  const response = await apiClient.post(
    `/api/battles/${battleId}/fractions/${fractionId}/orders`,
    payload,
    {
      headers: {
        'X-Auth-Token': token
      }
    }
  );
  return response.data;
};

export const executeTurn = async (battleId) => (await apiClient.post(`/api/battles/${battleId}/execute-turn`)).data;

export const endPlayerTurn = async (battleId, fractionId, token) => {
  const response = await apiClient.post(
    `/api/battles/${battleId}/fractions/${fractionId}/end-turn`,
    {},
    {
      headers: {
        'X-Auth-Token': token
      }
    }
  );
  return response.data;
};

export const getTurnLogs = async (battleId, fractionId, turnNumber, token) => {
  const response = await apiClient.get(
    `/api/battles/${battleId}/fractions/${fractionId}/turn-logs/${turnNumber}`,
    {
      headers: {
        'X-Auth-Token': token
      }
    }
  );
  return response.data;
};

export const getAdminTurnLogs = async (battleId, turnNumber) => {
  const response = await apiClient.get(
    `/api/battles/${battleId}/admin-logs/${turnNumber}`
  );
  return response.data;
};

// Fleet Management - Zarządzanie flotą frakcji
export const getFractionFleet = async (fractionId) => 
  (await apiClient.get(`/api/game/fractions/${fractionId}/fleet`)).data;

export const listFleetShips = async (fractionId) => 
  (await apiClient.get(`/api/game/fractions/${fractionId}/fleet/ships`)).data;

export const getFleetShip = async (fractionId, shipId) => 
  (await apiClient.get(`/api/game/fractions/${fractionId}/fleet/ships/${shipId}`)).data;

export const createFleetShip = async (fractionId, payload) => 
  (await apiClient.post(`/api/game/fractions/${fractionId}/fleet/ships`, payload)).data;

export const updateFleetShip = async (fractionId, shipId, payload) => 
  (await apiClient.put(`/api/game/fractions/${fractionId}/fleet/ships/${shipId}`, payload)).data;

export const deleteFleetShip = async (fractionId, shipId) => 
  (await apiClient.delete(`/api/game/fractions/${fractionId}/fleet/ships/${shipId}`)).data;

export const assignShipToSystem = async (fractionId, shipId, systemId) => 
  (await apiClient.patch(`/api/game/fractions/${fractionId}/fleet/ships/${shipId}/assign`, { systemId })).data;

// Planetary Systems - Układy planetarne
export const listPlanetarySystems = async (fractionId) => 
  (await apiClient.get(`/api/game/fractions/${fractionId}/fleet/systems`)).data;

export const getPlanetarySystem = async (fractionId, systemId) => 
  (await apiClient.get(`/api/game/fractions/${fractionId}/fleet/systems/${systemId}`)).data;

export const createPlanetarySystem = async (fractionId, payload) => 
  (await apiClient.post(`/api/game/fractions/${fractionId}/fleet/systems`, payload)).data;

export const updatePlanetarySystem = async (fractionId, systemId, payload) => 
  (await apiClient.put(`/api/game/fractions/${fractionId}/fleet/systems/${systemId}`, payload)).data;

export const deletePlanetarySystem = async (fractionId, systemId) => 
  (await apiClient.delete(`/api/game/fractions/${fractionId}/fleet/systems/${systemId}`)).data;