import axios from 'axios';

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000';

const discordApi = axios.create({
  baseURL: `${API_URL}/api/auth/discord`,
  headers: {
    'Content-Type': 'application/json',
  },
});

/**
 * Pobiera URL do logowania przez Discord
 */
export const getDiscordLoginUrl = async () => {
  const response = await discordApi.get('/login');
  return response.data;
};

/**
 * Waliduje token Discord
 */
export const validateDiscordToken = async (token) => {
  const response = await discordApi.get('/validate', {
    headers: {
      'X-Discord-Token': token,
    },
  });
  return response.data;
};

/**
 * Pobiera informacje o aktualnie zalogowanym użytkowniku Discord
 */
export const getCurrentDiscordUser = async (token) => {
  const response = await discordApi.get('/user', {
    headers: {
      'X-Discord-Token': token,
    },
  });
  return response.data;
};

/**
 * Zapisuje sesję Discord użytkownika
 */
export const saveDiscordSession = (token, user) => {
  localStorage.setItem('discord_token', token);
  localStorage.setItem('discord_user', JSON.stringify(user));
};

/**
 * Pobiera sesję Discord użytkownika
 */
export const getDiscordSession = () => {
  const token = localStorage.getItem('discord_token');
  const userJson = localStorage.getItem('discord_user');
  
  if (!token || !userJson) {
    return null;
  }
  
  try {
    const user = JSON.parse(userJson);
    return { token, user };
  } catch {
    return null;
  }
};

/**
 * Czyści sesję Discord użytkownika
 */
export const clearDiscordSession = () => {
  localStorage.removeItem('discord_token');
  localStorage.removeItem('discord_user');
};

/**
 * Sprawdza czy użytkownik jest administratorem
 */
export const isAdmin = () => {
  const session = getDiscordSession();
  return session?.user?.isAdmin || false;
};

/**
 * Pobiera role frakcji użytkownika
 */
export const getUserFractionRoles = () => {
  const session = getDiscordSession();
  return session?.user?.fractionRoles || [];
};

/**
 * Sprawdza czy użytkownik ma określoną rolę
 */
export const hasRole = (roleName) => {
  const session = getDiscordSession();
  return session?.user?.roles?.includes(roleName) || false;
};

export default {
  getDiscordLoginUrl,
  validateDiscordToken,
  getCurrentDiscordUser,
  saveDiscordSession,
  getDiscordSession,
  clearDiscordSession,
  isAdmin,
  getUserFractionRoles,
  hasRole,
};
