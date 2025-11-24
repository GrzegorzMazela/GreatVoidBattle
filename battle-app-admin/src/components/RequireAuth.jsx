import React, { useEffect, useState } from 'react';
import { initializePlayerSession, isPlayerAuthenticated } from '../services/authApi';

/**
 * Komponent zabezpieczający - wymusza autoryzację gracza
 * Użyj jako wrapper dla komponentów wymagających autoryzacji
 */
const RequireAuth = ({ children }) => {
  const [isLoading, setIsLoading] = useState(true);
  const [isAuthorized, setIsAuthorized] = useState(false);
  const [sessionData, setSessionData] = useState(null);

  useEffect(() => {
    // Inicjalizuj sesję przy montowaniu komponentu
    const session = initializePlayerSession();
    
    if (isPlayerAuthenticated()) {
      setIsAuthorized(true);
      setSessionData(session);
    } else {
      setIsAuthorized(false);
    }
    
    setIsLoading(false);
  }, []);

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500 mx-auto"></div>
          <p className="mt-4 text-gray-600">Ładowanie...</p>
        </div>
      </div>
    );
  }

  if (!isAuthorized) {
    return (
      <div className="flex items-center justify-center min-h-screen bg-gray-100">
        <div className="bg-white p-8 rounded-lg shadow-md max-w-md">
          <h2 className="text-2xl font-bold text-red-600 mb-4">Brak autoryzacji</h2>
          <p className="text-gray-700 mb-4">
            Nie masz dostępu do tej strony. Upewnij się, że masz poprawny link z tokenem autoryzacyjnym.
          </p>
          <p className="text-sm text-gray-500">
            Link powinien wyglądać tak:
            <code className="block mt-2 p-2 bg-gray-100 rounded">
              /battle/[battleId]?token=[token]&fractionId=[fractionId]
            </code>
          </p>
        </div>
      </div>
    );
  }

  // Przekaż dane sesji jako context lub props
  return React.cloneElement(children, { sessionData });
};

export default RequireAuth;
