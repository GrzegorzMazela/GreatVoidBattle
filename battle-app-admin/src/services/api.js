import axios from 'axios';

export const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
});   

// Battles
export const listBattles = async () => (await api.get('/api/battles')).data;
export const getBattle = async (id) => (await api.get(`/api/battles/${id}`)).data;
export const createBattle = async (payload) => (await api.post('/api/battles', payload)).data;
export const deleteBattle = async (id) => (await api.delete(`/api/battles/${id}`)).data;

// Fractions
export const listFractions = async (battleId) => (await api.get(`/api/battles/${battleId}/fractions`)).data;
export const createFraction = async (battleId, payload) => (await api.post(`/api/battles/${battleId}/fractions`, payload)).data;

// Ships
export const listShips = async (battleId, fractionId) => (await api.get(`/api/battles/${battleId}/fractions/${fractionId}/ships`)).data;
export const getShip = async (battleId, fractionId, shipId) => (await api.get(`/api/battles/${battleId}/fractions/${fractionId}/ships/${shipId}`)).data;
export const createShip = async (battleId, fractionId, payload) => (await api.post(`/api/battles/${battleId}/fractions/${fractionId}/ships`, payload)).data;
export const updateShip = async (battleId, fractionId, shipId, payload) => (await api.put(`/api/battles/${battleId}/fractions/${fractionId}/ships/${shipId}`, payload)).data;
export const deleteShip = async (battleId, fractionId, shipId) => (await api.delete(`/api/battles/${battleId}/fractions/${fractionId}/ships/${shipId}`)).data;
export const setShipPosition = async (battleId, fractionId, shipId, x, y) => (await api.patch(`/api/battles/${battleId}/fractions/${fractionId}/ships/${shipId}/position`, { x, y })).data;

// Battle simulation
export const submitOrders = async (battleId, fractionId, payload) => (await api.post(`/api/battles/${battleId}/fractions/${fractionId}/orders`, payload)).data;
export const executeTurn = async (battleId) => (await api.post(`/api/battles/${battleId}/execute-turn`)).data;
