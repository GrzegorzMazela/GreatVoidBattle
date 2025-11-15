import { useState, useEffect } from 'react';
import { 
  getPlayerSession, 
  isPlayerAuthenticated, 
  clearPlayerSession 
} from '../services/authApi';

/**
 * Hook do zarządzania sesją gracza
 * Użyj w komponentach wymagających dostępu do danych sesji
 */
export const usePlayerSession = () => {
  const [session, setSession] = useState(getPlayerSession());
  const [isAuthenticated, setIsAuthenticated] = useState(isPlayerAuthenticated());

  useEffect(() => {
    // Aktualizuj stan przy montowaniu
    setSession(getPlayerSession());
    setIsAuthenticated(isPlayerAuthenticated());
  }, []);

  const logout = () => {
    clearPlayerSession();
    setSession({ authToken: null, fractionId: null, battleId: null });
    setIsAuthenticated(false);
  };

  return {
    ...session,
    isAuthenticated,
    logout,
  };
};

export default usePlayerSession;
