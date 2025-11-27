import axios from 'axios';
import {
  Technology,
  FractionGameState,
  AddTechnologyRequest,
  GameSession,
  TechnologiesForTier
} from '../types/gameState';

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5112';

console.log('Game State API URL:', API_URL);

export const gameStateApi = {
  // Technologies
  getAllTechnologies: async (): Promise<Technology[]> => {
    try {
      console.log('Fetching all technologies from:', `${API_URL}/api/technologies`);
      const response = await axios.get(`${API_URL}/api/technologies`);
      console.log('Technologies response:', response.data);
      return response.data;
    } catch (error) {
      console.error('Error fetching technologies:', error);
      throw error;
    }
  },

  getTechnologiesByTier: async (tier: number): Promise<Technology[]> => {
    const response = await axios.get(`${API_URL}/api/technologies/tier/${tier}`);
    return response.data;
  },

  assignTechnology: async (request: AddTechnologyRequest): Promise<void> => {
    await axios.post(`${API_URL}/api/technologies/assign`, request);
  },

  removeTechnology: async (fractionId: string, technologyId: string): Promise<void> => {
    await axios.delete(`${API_URL}/api/technologies/fraction/${fractionId}/technology/${technologyId}`);
  },

  // Game State
  getActiveSession: async (): Promise<GameSession> => {
    const response = await axios.get(`${API_URL}/api/gamestate/session`);
    return response.data;
  },

  createSession: async (name: string, fractionIds: string[]): Promise<GameSession> => {
    const response = await axios.post(`${API_URL}/api/gamestate/session`, { name, fractionIds });
    return response.data;
  },

  getFractionState: async (fractionId: string): Promise<FractionGameState> => {
    const response = await axios.get(`${API_URL}/api/gamestate/fraction/${fractionId}`);
    return response.data;
  },

  getAvailableTechnologies: async (fractionId: string): Promise<TechnologiesForTier[]> => {
    const response = await axios.get(`${API_URL}/api/gamestate/fraction/${fractionId}/technologies`);
    return response.data;
  },

  advanceTier: async (fractionId: string): Promise<void> => {
    await axios.post(`${API_URL}/api/gamestate/fraction/${fractionId}/advance-tier`);
  },

  // Research Requests (for players)
  requestResearch: async (fractionId: string, technologyId: string): Promise<void> => {
    await axios.post(`${API_URL}/api/gamestate/fraction/${fractionId}/research-request`, {
      technologyId
    });
  },

  getPendingRequests: async (fractionId: string) => {
    const response = await axios.get(`${API_URL}/api/gamestate/fraction/${fractionId}/research-requests`);
    return response.data;
  },

  // Turn Management (for admin)
  getAllPendingRequests: async () => {
    const response = await axios.get(`${API_URL}/api/gamestate/admin/pending-requests`);
    return response.data;
  },

  endTurn: async (resolutions: Array<{fractionId: string, technologyId: string, approved: boolean, comment?: string}>) => {
    await axios.post(`${API_URL}/api/gamestate/admin/end-turn`, resolutions);
  }
};
