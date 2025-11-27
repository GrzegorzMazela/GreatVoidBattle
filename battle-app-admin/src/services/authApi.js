import apiClient from './apiClient';

// Re-export the client for backwards compatibility
const api = apiClient;

// === FRACTION API ===

/**
 * Tworzy nową frakcję w bitwie
 */
export const createFraction = async (battleId, fractionData) => {
  const response = await api.post(`/api/battles/${battleId}/fractions`, {
    fractionName: fractionData.fractionName,
    playerName: fractionData.playerName,
    fractionColor: fractionData.fractionColor,
  });
  
  // Zwraca { fractionId, authToken }
  return response.data;
};

/**
 * Pobiera wszystkie frakcje w bitwie
 */
export const getFractions = async (battleId) => {
  const response = await api.get(`/api/battles/${battleId}/fractions`);
  return response.data;
};

// === SHIPS API ===

/**
 * Dodaje statek do frakcji (wymaga AuthToken)
 */
export const addShip = async (battleId, fractionId, shipData) => {
  const response = await api.post(
    `/api/battles/${battleId}/fractions/${fractionId}/ships`,
    shipData
  );
  return response.data;
};

/**
 * Aktualizuje statek (wymaga AuthToken)
 */
export const updateShip = async (battleId, fractionId, shipId, shipData) => {
  const response = await api.put(
    `/api/battles/${battleId}/fractions/${fractionId}/ships/${shipId}`,
    shipData
  );
  return response.data;
};

/**
 * Pobiera wszystkie statki frakcji
 */
export const getShips = async (battleId, fractionId) => {
  const response = await api.get(
    `/api/battles/${battleId}/fractions/${fractionId}/ships`
  );
  return response.data;
};

/**
 * Ustawia pozycję statku (wymaga AuthToken)
 */
export const setShipPosition = async (battleId, fractionId, shipId, position) => {
  const response = await api.patch(
    `/api/battles/${battleId}/fractions/${fractionId}/ships/${shipId}/position`,
    { x: position.x, y: position.y }
  );
  return response.data;
};

// === AUTH HELPERS ===

/**
 * Inicjalizuje sesję gracza z URL
 * Wywołaj to przy załadowaniu aplikacji
 */
export const initializePlayerSession = () => {
  const urlParams = new URLSearchParams(window.location.search);
  const authToken = urlParams.get('token');
  const fractionId = urlParams.get('fractionId');
  const battleId = urlParams.get('battleId');

  if (authToken && fractionId) {
    localStorage.setItem('authToken', authToken);
    localStorage.setItem('fractionId', fractionId);
    
    if (battleId) {
      localStorage.setItem('battleId', battleId);
    }

    // Usuń tylko parametry auth z URL (token, fractionId, battleId), pozostaw inne
    urlParams.delete('token');
    urlParams.delete('fractionId');
    if (battleId) {
      urlParams.delete('battleId');
    }
    
    const newSearch = urlParams.toString();
    const newUrl = window.location.pathname + (newSearch ? '?' + newSearch : '');
    window.history.replaceState({}, document.title, newUrl);
    
    return { authToken, fractionId, battleId };
  }

  // Spróbuj pobrać z localStorage
  return {
    authToken: localStorage.getItem('authToken'),
    fractionId: localStorage.getItem('fractionId'),
    battleId: localStorage.getItem('battleId'),
  };
};

/**
 * Czyści sesję gracza
 */
export const clearPlayerSession = () => {
  localStorage.removeItem('authToken');
  localStorage.removeItem('fractionId');
  localStorage.removeItem('battleId');
};

/**
 * Sprawdza czy gracz jest zalogowany (przez auth token)
 */
export const isPlayerAuthenticated = () => {
  return !!localStorage.getItem('authToken') && !!localStorage.getItem('fractionId');
};

/**
 * Pobiera dane sesji gracza
 */
export const getPlayerSession = () => {
  return {
    authToken: localStorage.getItem('authToken'),
    fractionId: localStorage.getItem('fractionId'),
    battleId: localStorage.getItem('battleId'),
  };
};

export default api;
