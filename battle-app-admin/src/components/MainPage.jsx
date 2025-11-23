import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { getDiscordSession, clearDiscordSession } from '../services/discordAuthApi';
import mainImage from '../assets/main.png';
import './MainPage.css';

const MainPage = () => {
  const navigate = useNavigate();
  const session = getDiscordSession();

  useEffect(() => {
    if (!session) {
      // Jeśli nie jest zalogowany, przekieruj do logowania
      navigate('/login');
      return;
    }

    // Admin → panel administracyjny
    if (session.user.isAdmin) {
      navigate('/pustka-admin-panel');
    } 
    // Gracz z rolą frakcji → TODO: lista bitew lub wybór frakcji
    else if (session.user.fractionRole) {
      // Na razie zostaw na stronie głównej
      // W przyszłości: navigate('/battles');
    }
    // Gracz bez frakcji → informacja o braku dostępu
    else {
      // Zostaw na stronie głównej z informacją
    }
  }, [session, navigate]);

  const getAvatarUrl = () => {
    if (session?.user?.avatar) {
      return `https://cdn.discordapp.com/avatars/${session.user.id}/${session.user.avatar}.png`;
    }
    const defaultAvatarNumber = parseInt(session?.user?.discriminator || '0') % 5;
    return `https://cdn.discordapp.com/embed/avatars/${defaultAvatarNumber}.png`;
  };

  const handleLogout = () => {
    clearDiscordSession();
    navigate('/login');
  };

  return (
    <div className="main-page">
      <img src={mainImage} alt="Great Void Battle" className="main-image" />
      {session && !session.user.isAdmin && (
        <div className="user-panel">
          <div className="user-panel-header">
            <img 
              src={getAvatarUrl()} 
              alt={session.user.username}
              className="user-panel-avatar"
            />
            <div className="user-panel-info">
              <h3 className="user-panel-username">
                {session.user.username}
                {session.user.discriminator !== '0' && `#${session.user.discriminator}`}
              </h3>
              {session.user.fractionRole && (
                <div className="user-panel-fraction">
                  <span className="fraction-icon">🚀</span>
                  <span className="fraction-name">{session.user.fractionRole}</span>
                </div>
              )}
            </div>
          </div>
          
          <div className="user-panel-content">
            <p className="user-panel-message">
              Dostęp do bitew będzie dostępny wkrótce.
            </p>
          </div>

          <button 
            className="user-panel-logout"
            onClick={handleLogout}
          >
            Wyloguj się
          </button>
        </div>
      )}
    </div>
  );
};

export default MainPage;
