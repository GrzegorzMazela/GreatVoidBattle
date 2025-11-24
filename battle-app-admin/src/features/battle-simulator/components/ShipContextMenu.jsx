import PropTypes from 'prop-types';
import './ShipContextMenu.css';

/**
 * Okno kontekstowe ze statystykami statku i opcjami strzelania
 */
export const ShipContextMenu = ({ 
  ship,
  position,
  weaponMode,
  onWeaponModeChange,
  missileFiredCount,
  laserFiredCount,
  onClose,
}) => {
  if (!ship) return null;

  // Oblicz ile broni ma statek
  const totalMissiles = ship.numberOfMissiles || 0;
  const totalLasers = ship.numberOfLasers || 0;
  const pointDefense = ship.numberOfPointsDefense || 0;

  // Oblicz dostępną broń
  const availableMissiles = totalMissiles - (missileFiredCount || 0);
  const availableLasers = totalLasers - (laserFiredCount || 0);

  return (
    <div 
      className="ship-context-menu" 
      style={{
        left: `${position.x}px`,
        top: `${position.y}px`,
      }}
    >
      <div className="context-menu-header">
        <h4>{ship.name}</h4>
        <button className="close-btn" onClick={onClose}>✕</button>
      </div>

      <div className="context-menu-weapons-compact">
        <button 
          className={`weapon-btn-compact ${weaponMode === 'missile' ? 'active' : ''} ${availableMissiles === 0 ? 'disabled' : ''}`}
          onClick={() => availableMissiles > 0 && onWeaponModeChange('missile')}
          disabled={availableMissiles === 0}
          title={`Rakiety: ${missileFiredCount || 0}/${totalMissiles} (zasięg 35-55)`}
        >
          <span className="weapon-icon">🚀</span>
          <span className="weapon-count-compact">{missileFiredCount || 0}/{totalMissiles}</span>
        </button>
        <button 
          className={`weapon-btn-compact ${weaponMode === 'laser' ? 'active' : ''} ${availableLasers === 0 ? 'disabled' : ''}`}
          onClick={() => availableLasers > 0 && onWeaponModeChange('laser')}
          disabled={availableLasers === 0}
          title={`Lasery: ${laserFiredCount || 0}/${totalLasers} (zasięg 0-20)`}
        >
          <span className="weapon-icon">⚡</span>
          <span className="weapon-count-compact">{laserFiredCount || 0}/{totalLasers}</span>
        </button>
      </div>

      {weaponMode && (
        <div className="context-menu-hint-compact">
          {weaponMode === 'missile' ? (
            <p>💡 Zasięg rakiet: {ship.missileEffectiveRange || 35}-{ship.missileMaxRange || 55}</p>
          ) : (
            <p>💡 Zasięg laserów: 0-{ship.laserMaxRange || 15}</p>
          )}
        </div>
      )}

      <div className="context-menu-stats">
        <div className="stat-row">
          <span className="stat-icon">❤️</span>
          <span className="stat-label">HP:</span>
          <span className="stat-value">{ship.hitPoints}</span>
        </div>

        <div className="stat-row">
          <span className="stat-icon">🛡️</span>
          <span className="stat-label">Tarcze:</span>
          <span className="stat-value">{ship.shields || 0}</span>
        </div>

        <div className="stat-row">
          <span className="stat-icon">🔰</span>
          <span className="stat-label">Pancerz:</span>
          <span className="stat-value">{ship.armor || 0}</span>
        </div>

        <div className="stat-row">
          <span className="stat-icon">🎯</span>
          <span className="stat-label">Obrona (PD):</span>
          <span className="stat-value">{pointDefense}</span>
        </div>
      </div>
    </div>
  );
};

ShipContextMenu.propTypes = {
  ship: PropTypes.shape({
    shipId: PropTypes.string.isRequired,
    name: PropTypes.string.isRequired,
    hitPoints: PropTypes.number,
    shields: PropTypes.number,
    armor: PropTypes.number,
    numberOfMissiles: PropTypes.number,
    numberOfLasers: PropTypes.number,
    numberOfPointsDefense: PropTypes.number,
  }),
  position: PropTypes.shape({
    x: PropTypes.number.isRequired,
    y: PropTypes.number.isRequired,
  }).isRequired,
  weaponMode: PropTypes.oneOf(['missile', 'laser', null]),
  onWeaponModeChange: PropTypes.func.isRequired,
  missileFiredCount: PropTypes.number,
  laserFiredCount: PropTypes.number,
  onClose: PropTypes.func.isRequired,
};
