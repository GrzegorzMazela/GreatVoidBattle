import PropTypes from 'prop-types';
import './ShipCard.css';

// Import ikon statków
import CorvetteIcon from '../../../assets/Corvette_64.png';
import DestroyerIcon from '../../../assets/Destroyer_64.png';
import CruiserIcon from '../../../assets/Cruiser_64.png';
import BattleshipIcon from '../../../assets/Battleship_64.png';
import SuperBattleshipIcon from '../../../assets/SuperBattleship_64.png';
import OrbitalFortIcon from '../../../assets/OrbitalFort_64.png';

// Mapowanie typów statków do ikon
const SHIP_ICONS = {
  Corvette: CorvetteIcon,
  Destroyer: DestroyerIcon,
  Cruiser: CruiserIcon,
  Battleship: BattleshipIcon,
  SuperBattleship: SuperBattleshipIcon,
  OrbitalFort: OrbitalFortIcon,
};

// Maksymalne wartości dla każdego typu statku (z ShipFactory.cs)
const SHIP_MAX_VALUES = {
  Corvette: { hitPoints: 50, shields: 25, armor: 25 },
  Destroyer: { hitPoints: 100, shields: 50, armor: 50 },
  Cruiser: { hitPoints: 200, shields: 100, armor: 100 },
  Battleship: { hitPoints: 400, shields: 200, armor: 200 },
  SuperBattleship: { hitPoints: 600, shields: 300, armor: 300 },
  OrbitalFort: { hitPoints: 100, shields: 50, armor: 50 },
};

/**
 * Karta jednostki w dolnym panelu
 * Pokazuje ikonę, nazwę, paski zdrowia/tarczy/pancerza i ikonki broni
 */
export const ShipCard = ({ 
  ship, 
  isSelected, 
  isHovered, 
  onHover, 
  onLeave, 
  onClick,
  fractionColor 
}) => {
  const shipIcon = SHIP_ICONS[ship.type];
  const maxValues = SHIP_MAX_VALUES[ship.type] || { hitPoints: 100, shields: 50, armor: 50 };

  // Oblicz procenty
  const hpPercent = Math.max(0, Math.min(100, (ship.hitPoints / maxValues.hitPoints) * 100));
  const shieldsPercent = Math.max(0, Math.min(100, (ship.shields / maxValues.shields) * 100));
  const armorPercent = Math.max(0, Math.min(100, (ship.armor / maxValues.armor) * 100));

  // Kolory pasków w zależności od wartości
  const getBarColor = (percent) => {
    if (percent > 66) return '#4CAF50'; // Zielony
    if (percent > 33) return '#FF9800'; // Pomarańczowy
    return '#F44336'; // Czerwony
  };

  const handleClick = () => {
    if (onClick) {
      onClick(ship);
    }
  };

  const handleMouseEnter = () => {
    if (onHover) {
      onHover(ship);
    }
  };

  const handleMouseLeave = () => {
    if (onLeave) {
      onLeave();
    }
  };

  return (
    <div 
      className={`ship-card ${isSelected ? 'selected' : ''} ${isHovered ? 'hovered' : ''}`}
      onClick={handleClick}
      onMouseEnter={handleMouseEnter}
      onMouseLeave={handleMouseLeave}
      style={{ '--fraction-color': fractionColor || '#4CAF50' }}
    >
      {/* Górny rząd - ikona, nazwa, typ, broń */}
      <div className="ship-card-top-row">
        {/* Ikona statku */}
        <div className="ship-card-icon">
          {shipIcon ? (
            <img src={shipIcon} alt={ship.type} />
          ) : (
            <div className="ship-card-placeholder">{ship.type[0]}</div>
          )}
        </div>

        {/* Informacje o statku */}
        <div className="ship-card-info">
          <div className="ship-card-name">{ship.name}</div>
          <div className="ship-card-type">{ship.type}</div>
        </div>

        {/* Ikonki broni */}
        <div className="ship-card-weapons">
          {ship.numberOfLasers > 0 && (
            <div className="weapon-icon laser" title={`Lasery: ${ship.numberOfLasers}`}>
              <svg viewBox="0 0 24 24" fill="currentColor">
                <path d="M12 2L3 14h4v8h10v-8h4L12 2zm0 4l5 8h-3v6H10v-6H7l5-8z"/>
              </svg>
              <span className="weapon-count">{ship.numberOfLasers}</span>
            </div>
          )}
          {ship.numberOfMissiles > 0 && (
            <div className="weapon-icon missile" title={`Pociski: ${ship.numberOfMissiles}`}>
              <svg viewBox="0 0 24 24" fill="currentColor">
                <path d="M2.81 14.12L5.64 11.29L8.17 10.79C11.39 6.41 17.29 4.23 19.76 5.17C21.78 6.47 21.06 11.87 17.77 15.89L17.33 18.46L14.5 21.29L12.62 15.5L7.5 14.39L2.81 14.12M15.55 7.6C16.77 7.6 17.76 8.59 17.76 9.81C17.76 11.03 16.77 12.02 15.55 12.02C14.33 12.02 13.34 11.03 13.34 9.81C13.34 8.59 14.33 7.6 15.55 7.6M5.08 12.41L6.94 14.27L2.81 15.73L5.08 12.41M11.73 18.53L13.59 20.39L10.27 16.26L11.73 18.53Z"/>
              </svg>
              <span className="weapon-count">{ship.numberOfMissiles}</span>
            </div>
          )}
          {ship.numberOfPointsDefense > 0 && (
            <div className="weapon-icon defense" title={`Obrona punktowa: ${ship.numberOfPointsDefense}`}>
              <svg viewBox="0 0 24 24" fill="currentColor">
                <path d="M12,1L3,5V11C3,16.55 6.84,21.74 12,23C17.16,21.74 21,16.55 21,11V5L12,1M12,5A3,3 0 0,1 15,8A3,3 0 0,1 12,11A3,3 0 0,1 9,8A3,3 0 0,1 12,5M17.13,17C15.92,18.85 14.11,20.24 12,20.92C9.89,20.24 8.08,18.85 6.87,17C6.53,16.5 6.24,16 6,15.47C6,13.82 8.71,12.47 12,12.47C15.29,12.47 18,13.79 18,15.47C17.76,16 17.47,16.5 17.13,17Z"/>
              </svg>
              <span className="weapon-count">{ship.numberOfPointsDefense}</span>
            </div>
          )}
        </div>
      </div>

      {/* Paski statusu */}
      <div className="ship-card-bars">
        {/* Zdrowie (HP) */}
        <div className="ship-stat-bar">
          <div className="stat-label">HP</div>
          <div className="stat-bar-bg">
            <div 
              className="stat-bar-fill hp"
              style={{ 
                width: `${hpPercent}%`,
                backgroundColor: getBarColor(hpPercent)
              }}
            />
          </div>
          <div className="stat-value">{ship.hitPoints}/{maxValues.hitPoints}</div>
        </div>

        {/* Tarcza (Shields) */}
        <div className="ship-stat-bar">
          <div className="stat-label">SHD</div>
          <div className="stat-bar-bg">
            <div 
              className="stat-bar-fill shields"
              style={{ 
                width: `${shieldsPercent}%`,
                backgroundColor: getBarColor(shieldsPercent)
              }}
            />
          </div>
          <div className="stat-value">{ship.shields}/{maxValues.shields}</div>
        </div>

        {/* Pancerz (Armor) */}
        <div className="ship-stat-bar">
          <div className="stat-label">ARM</div>
          <div className="stat-bar-bg">
            <div 
              className="stat-bar-fill armor"
              style={{ 
                width: `${armorPercent}%`,
                backgroundColor: getBarColor(armorPercent)
              }}
            />
          </div>
          <div className="stat-value">{ship.armor}/{maxValues.armor}</div>
        </div>
      </div>
    </div>
  );
};

ShipCard.propTypes = {
  ship: PropTypes.shape({
    shipId: PropTypes.string.isRequired,
    name: PropTypes.string.isRequired,
    type: PropTypes.string.isRequired,
    hitPoints: PropTypes.number,
    shields: PropTypes.number,
    armor: PropTypes.number,
    numberOfLasers: PropTypes.number,
    numberOfMissiles: PropTypes.number,
    numberOfPointsDefense: PropTypes.number,
  }).isRequired,
  isSelected: PropTypes.bool,
  isHovered: PropTypes.bool,
  onHover: PropTypes.func,
  onLeave: PropTypes.func,
  onClick: PropTypes.func,
  fractionColor: PropTypes.string,
};
