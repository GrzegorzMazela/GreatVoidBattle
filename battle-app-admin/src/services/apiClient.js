import axios from 'axios';

/**
 * Unified API client - single axios instance for all API calls
 */
const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000';

export const apiClient = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Interceptor adding auth headers
apiClient.interceptors.request.use(
  (config) => {
    // Discord token (for admin panel)
    const discordToken = localStorage.getItem('discord_token');
    if (discordToken) {
      config.headers['X-Discord-Token'] = discordToken;
    }
    
    // Fraction auth token (for battle simulator)
    const authToken = localStorage.getItem('authToken');
    if (authToken) {
      config.headers['X-Auth-Token'] = authToken;
    }
    
    return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor for error handling
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401 || error.response?.status === 403) {
      console.error('Authorization error:', error.response.data);
    }
    return Promise.reject(error);
  }
);

export default apiClient;

