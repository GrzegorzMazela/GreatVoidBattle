import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { getDiscordSession, clearDiscordSession } from '../../services/discordAuthApi';
import './UserProfile.css';

export const UserProfile = () => {
  const [showDropdown, setShowDropdown] = useState(false);
  const navigate = useNavigate();
  const session = getDiscordSession();

  if (!session) return null;

  const { user } = session;

  const getAvatarUrl = () => {
    if (user.avatar) {
      return `https://cdn.discordapp.com/avatars/${user.id}/${user.avatar}.png`;
    }
    // Default Discord avatar
    const defaultAvatarNumber = parseInt(user.discriminator) % 5;
    return `https://cdn.discordapp.com/embed/avatars/${defaultAvatarNumber}.png`;
  };

  const handleLogout = () => {
    clearDiscordSession();
    navigate('/login');
  };

  return (
    <div className="user-profile">
      <div 
        className="user-profile-trigger"
        onClick={() => setShowDropdown(!showDropdown)}
      >
        <img 
          src={getAvatarUrl()} 
          alt={user.username}
          className="user-avatar"
        />
        <div className="user-info">
          <span className="user-name">{user.username}</span>
          {user.discriminator !== '0' && (
            <span className="user-discriminator">#{user.discriminator}</span>
          )}
        </div>
      </div>

      {showDropdown && (
        <div className="user-dropdown">
          <div className="user-dropdown-header">
            <img 
              src={getAvatarUrl()} 
              alt={user.username}
              className="user-dropdown-avatar"
            />
            <div>
              <div className="user-dropdown-name">
                {user.username}
                {user.discriminator !== '0' && `#${user.discriminator}`}
              </div>
              <div className="user-dropdown-email">{user.email}</div>
            </div>
          </div>

          {user.isAdmin && (
            <div className="user-badge admin-badge">
              👑 Administrator
            </div>
          )}

          {user.fractionRole && (
            <div className="user-badge fraction-badge">
              🚀 {user.fractionRole}
            </div>
          )}

          {user.roles && user.roles.length > 0 && (
            <div className="user-roles">
              <div className="user-roles-title">Role Discord:</div>
              {user.roles.map((role, index) => (
                <div key={index} className="user-role-item">
                  {role}
                </div>
              ))}
            </div>
          )}

          <button 
            className="logout-button"
            onClick={handleLogout}
          >
            Wyloguj się
          </button>
        </div>
      )}

      {showDropdown && (
        <div 
          className="user-dropdown-overlay"
          onClick={() => setShowDropdown(false)}
        />
      )}
    </div>
  );
};
