import { useEffect, useState } from 'react';
import { initializePlayerSession, isPlayerAuthenticated } from '../../services/authApi';

/**
 * Auth route for battle simulator - only requires auth key (no Discord login)
 * This is used for players accessing the simulator via a link with token
 */
export const FractionAuthRoute = ({ children }) => {
  const [isLoading, setIsLoading] = useState(true);
  const [isAuthorized, setIsAuthorized] = useState(false);
  const [sessionData, setSessionData] = useState(null);

  useEffect(() => {
    // Initialize session from URL params or localStorage
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
      <div style={{
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        height: '100vh',
        backgroundColor: '#1a1a2e',
        color: 'white'
      }}>
        <div style={{ textAlign: 'center' }}>
          <div style={{
            width: '48px',
            height: '48px',
            border: '4px solid rgba(255,255,255,0.3)',
            borderTopColor: '#3b82f6',
            borderRadius: '50%',
            margin: '0 auto 16px',
            animation: 'spin 1s linear infinite'
          }} />
          <style>{`@keyframes spin { to { transform: rotate(360deg); } }`}</style>
          <p>Ładowanie...</p>
        </div>
      </div>
    );
  }

  if (!isAuthorized) {
    return (
      <div style={{
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        height: '100vh',
        backgroundColor: '#1a1a2e',
        color: 'white',
        padding: '20px'
      }}>
        <div style={{
          backgroundColor: '#2d2d44',
          padding: '32px',
          borderRadius: '12px',
          maxWidth: '500px',
          textAlign: 'center'
        }}>
          <h2 style={{ color: '#ef4444', marginBottom: '16px' }}>Brak autoryzacji</h2>
          <p style={{ marginBottom: '16px', color: '#a0aec0' }}>
            Nie masz dostępu do symulatora bitwy. Upewnij się, że masz poprawny link z tokenem autoryzacyjnym.
          </p>
          <p style={{ fontSize: '14px', color: '#718096' }}>
            Link powinien wyglądać tak:
          </p>
          <code style={{
            display: 'block',
            marginTop: '8px',
            padding: '12px',
            backgroundColor: '#1a1a2e',
            borderRadius: '6px',
            fontSize: '13px',
            color: '#68d391',
            wordBreak: 'break-all'
          }}>
            /battles/[battleId]/simulator?token=[token]&fractionId=[fractionId]
          </code>
        </div>
      </div>
    );
  }

  // Clone children with sessionData
  return typeof children === 'function' 
    ? children({ sessionData }) 
    : children;
};

export default FractionAuthRoute;

