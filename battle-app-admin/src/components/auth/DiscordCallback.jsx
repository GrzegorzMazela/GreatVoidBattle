import { useEffect, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { saveDiscordSession, validateDiscordToken } from '../../services/discordAuthApi';
import './DiscordCallback.css';

export const DiscordCallback = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const [status, setStatus] = useState('processing');
  const [error, setError] = useState(null);

  useEffect(() => {
    const handleCallback = async () => {
      const token = searchParams.get('token');
      const userId = searchParams.get('userId');
      const username = searchParams.get('username');
      const discriminator = searchParams.get('discriminator');
      const avatar = searchParams.get('avatar');
      const email = searchParams.get('email');

      if (!token || !userId) {
        setError('Brak wymaganych parametrów autoryzacji');
        setStatus('error');
        return;
      }

      try {
        // Waliduj token z backendem
        const validationResponse = await validateDiscordToken(token);
        
        console.log('Validation response:', validationResponse);
        
        if (!validationResponse.valid) {
          throw new Error('Token validation failed');
        }

        // Użyj danych użytkownika z backendu (zawiera isAdmin, fractionRoles, etc.)
        const user = validationResponse.user || {
          id: userId,
          username: username || 'Unknown',
          discriminator: discriminator || '0000',
          avatar: avatar || '',
          email: email || '',
          roles: validationResponse.roles || [],
          isAdmin: false,
          fractionRoles: []
        };
        
        console.log('User to save:', user);
        saveDiscordSession(token, user);
        
        setStatus('success');
        
        // Przekieruj do głównej strony po 1.5 sekundy
        setTimeout(() => {
          navigate('/');
        }, 1500);
      } catch (err) {
        console.error('Error during Discord callback:', err);
        setError('Nie udało się zalogować. Spróbuj ponownie.');
        setStatus('error');
      }
    };

    handleCallback();
  }, [searchParams, navigate]);

  return (
    <div className="discord-callback-container">
      <div className="discord-callback-card">
        {status === 'processing' && (
          <div className="discord-callback-processing">
            <div className="discord-callback-spinner"></div>
            <h2>Logowanie przez Discord...</h2>
            <p>Proszę czekać, trwa weryfikacja</p>
          </div>
        )}

        {status === 'success' && (
          <div className="discord-callback-success">
            <div className="discord-callback-icon success">✓</div>
            <h2>Zalogowano pomyślnie!</h2>
            <p>Przekierowywanie do aplikacji...</p>
          </div>
        )}

        {status === 'error' && (
          <div className="discord-callback-error">
            <div className="discord-callback-icon error">✗</div>
            <h2>Błąd logowania</h2>
            <p>{error}</p>
            <button 
              className="discord-callback-retry"
              onClick={() => navigate('/login')}
            >
              Spróbuj ponownie
            </button>
          </div>
        )}
      </div>
    </div>
  );
};
