import { Navigate } from 'react-router-dom';
import { getDiscordSession } from '../../services/discordAuthApi';

export const ProtectedRoute = ({ children, requireAdmin = false, requirePlayer = true, allowedRoles = [] }) => {
  const session = getDiscordSession();

  // Debug logging
  console.log('ProtectedRoute - Session:', session);
  console.log('ProtectedRoute - Require Admin:', requireAdmin);
  console.log('ProtectedRoute - Require Player:', requirePlayer);
  console.log('ProtectedRoute - User isAdmin:', session?.user?.isAdmin);
  console.log('ProtectedRoute - User roles:', session?.user?.roles);
  console.log('ProtectedRoute - Type of isAdmin:', typeof session?.user?.isAdmin);
  console.log('ProtectedRoute - User object:', JSON.stringify(session?.user, null, 2));

  // Brak sesji - przekieruj do logowania
  if (!session) {
    return <Navigate to="/login" replace />;
  }

  // Sprawdź czy wymaga roli gracza (podstawowa autoryzacja)
  // Admin zawsze ma dostęp, więc pomijamy sprawdzenie dla admina
  if (requirePlayer && !session.user.isAdmin) {
    const hasPlayerRole = session.user.roles?.includes('gracz') || 
                          session.user.roles?.includes('Gracz');
    
    if (!hasPlayerRole) {
      console.log('Access denied - user is not a player');
      return (
        <div style={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          height: '100vh',
          backgroundColor: '#1a1a2e',
          color: 'white',
          fontSize: '24px',
          textAlign: 'center'
        }}>
          <div>
            <h1>Brak dostępu</h1>
            <p>Musisz mieć rolę "gracz" aby uzyskać dostęp</p>
          </div>
        </div>
      );
    }
  }

  // Sprawdź czy wymaga roli admina
  if (requireAdmin && !session.user.isAdmin) {
    console.log('Access denied - user is not admin');
    return (
      <div style={{
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        height: '100vh',
        backgroundColor: '#1a1a2e',
        color: 'white',
        fontSize: '24px',
        textAlign: 'center'
      }}>
        <div>
          <h1>Brak dostępu</h1>
          <p>Ta strona wymaga uprawnień administratora</p>
        </div>
      </div>
    );
  }

  // Sprawdź czy użytkownik ma odpowiednią rolę frakcji
  if (allowedRoles.length > 0) {
    const userFractionRoles = session.user.fractionRoles || [];
    const hasRole = allowedRoles.some(role => 
      session.user.roles?.includes(role) || userFractionRoles.includes(role)
    );

    if (!hasRole && !session.user.isAdmin) { // Admin ma dostęp wszędzie
      return (
        <div style={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          height: '100vh',
          backgroundColor: '#1a1a2e',
          color: 'white',
          fontSize: '24px',
          textAlign: 'center'
        }}>
          <div>
            <h1>Brak dostępu</h1>
            <p>Ta strona jest dostępna tylko dla określonych frakcji</p>
            <p>Wymagane role: {allowedRoles.join(', ')}</p>
          </div>
        </div>
      );
    }
  }

  return children;
};
