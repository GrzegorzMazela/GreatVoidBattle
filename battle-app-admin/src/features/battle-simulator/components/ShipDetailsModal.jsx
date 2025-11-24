import PropTypes from 'prop-types';
import './ShipDetailsModal.css';

// Import ikon statków
import CorvetteIcon from '../../../assets/Corvette_256.png';
import DestroyerIcon from '../../../assets/Destroyer_256.png';
import CruiserIcon from '../../../assets/Cruiser_256.png';
import BattleshipIcon from '../../../assets/Battleship_256.png';
import SuperBattleshipIcon from '../../../assets/SuperBattleship_256.png';
import OrbitalFortIcon from '../../../assets/OrbitalFort_256.png';

// Mapowanie typów statków do większych ikon
const SHIP_ICONS_LARGE = {
  Corvette: CorvetteIcon,
  Destroyer: DestroyerIcon,
  Cruiser: CruiserIcon,
  Battleship: BattleshipIcon,
  SuperBattleship: SuperBattleshipIcon,
  OrbitalFort: OrbitalFortIcon,
};

// Maksymalne wartości dla każdego typu statku
const SHIP_MAX_VALUES = {
  Corvette: { hitPoints: 50, shields: 25, armor: 25, speed: 10 },
  Destroyer: { hitPoints: 100, shields: 50, armor: 50, speed: 8 },
  Cruiser: { hitPoints: 200, shields: 100, armor: 100, speed: 6 },
  Battleship: { hitPoints: 400, shields: 200, armor: 200, speed: 5 },
  SuperBattleship: { hitPoints: 600, shields: 300, armor: 300, speed: 5 },
  OrbitalFort: { hitPoints: 100, shields: 50, armor: 50, speed: 0 },
};

// Tłumaczenia typów statków
const SHIP_TYPE_NAMES = {
  Corvette: 'Korweta',
  Destroyer: 'Niszczyciel',
  Cruiser: 'Krążownik',
  Battleship: 'Pancernik',
  SuperBattleship: 'Super Pancernik',
  OrbitalFort: 'Fort Orbitalny',
};

/**
 * Modal ze szczegółowymi informacjami o statku
 */
export const ShipDetailsModal = ({ ship, onClose, fractionColor }) => {
  const shipIcon = SHIP_ICONS_LARGE[ship.type];
  const maxValues = SHIP_MAX_VALUES[ship.type] || { 
    hitPoints: 100, shields: 50, armor: 50, speed: 5 
  };
  const shipTypeName = SHIP_TYPE_NAMES[ship.type] || ship.type;

  // Oblicz procenty
  const hpPercent = Math.max(0, Math.min(100, (ship.hitPoints / maxValues.hitPoints) * 100));
  const shieldsPercent = Math.max(0, Math.min(100, (ship.shields / maxValues.shields) * 100));
  const armorPercent = Math.max(0, Math.min(100, (ship.armor / maxValues.armor) * 100));

  // Kolory pasków
  const getBarColor = (percent) => {
    if (percent > 66) return '#4CAF50';
    if (percent > 33) return '#FF9800';
    return '#F44336';
  };

  const handleBackdropClick = (e) => {
    if (e.target === e.currentTarget) {
      onClose();
    }
  };

  return (
    <div className="ship-details-modal-backdrop" onClick={handleBackdropClick}>
      <div className="ship-details-modal" style={{ '--fraction-color': fractionColor || '#4CAF50' }}>
        <div className="modal-header">
          <div className="ship-icon-large">
            {shipIcon ? (
              <img src={shipIcon} alt={ship.type} />
            ) : (
              <div className="ship-placeholder-large">{ship.type[0]}</div>
            )}
          </div>
          <div className="ship-title-info">
            <h2>{ship.name}</h2>
            <p className="ship-type-name">{shipTypeName}</p>
            <p className="ship-id">ID: {ship.shipId?.substring(0, 8)}...</p>
          </div>
        </div>

        <div className="modal-content">
          {/* Sekcja Status */}
          <div className="details-section">
            <h3>Status</h3>
            
            <div className="detail-stat-row">
              <span className="stat-label-detail">Punkty Wytrzymałości (HP)</span>
              <div className="stat-bar-container">
                <div className="stat-bar-bg-detail">
                  <div 
                    className="stat-bar-fill-detail hp"
                    style={{ 
                      width: `${hpPercent}%`,
                      backgroundColor: getBarColor(hpPercent)
                    }}
                  />
                </div>
                <span className="stat-value-detail">{ship.hitPoints}/{maxValues.hitPoints}</span>
              </div>
            </div>

            <div className="detail-stat-row">
              <span className="stat-label-detail">Tarcze (Shields)</span>
              <div className="stat-bar-container">
                <div className="stat-bar-bg-detail">
                  <div 
                    className="stat-bar-fill-detail shields"
                    style={{ 
                      width: `${shieldsPercent}%`,
                      backgroundColor: getBarColor(shieldsPercent)
                    }}
                  />
                </div>
                <span className="stat-value-detail">{ship.shields}/{maxValues.shields}</span>
              </div>
            </div>

            <div className="detail-stat-row">
              <span className="stat-label-detail">Pancerz (Armor)</span>
              <div className="stat-bar-container">
                <div className="stat-bar-bg-detail">
                  <div 
                    className="stat-bar-fill-detail armor"
                    style={{ 
                      width: `${armorPercent}%`,
                      backgroundColor: getBarColor(armorPercent)
                    }}
                  />
                </div>
                <span className="stat-value-detail">{ship.armor}/{maxValues.armor}</span>
              </div>
            </div>
          </div>

          {/* Sekcja Charakterystyki */}
          <div className="details-section">
            <h3>Charakterystyki</h3>
            <div className="characteristics-grid">
              <div className="characteristic-item">
                <span className="char-label">Pozycja</span>
                <span className="char-value">{Math.floor(ship.x || 0)}, {Math.floor(ship.y || 0)}</span>
              </div>
              <div className="characteristic-item">
                <span className="char-label">Prędkość</span>
                <span className="char-value">{ship.speed || maxValues.speed}</span>
              </div>
              <div className="characteristic-item">
                <span className="char-label">Moduły</span>
                <span className="char-value">{ship.numberOfModules || 0}</span>
              </div>
              <div className="characteristic-item">
                <span className="char-label">Status</span>
                <span className="char-value status-active">{ship.status || 'Active'}</span>
              </div>
            </div>
          </div>

          {/* Sekcja Uzbrojenie */}
          <div className="details-section">
            <h3>Uzbrojenie</h3>
            <div className="weapons-grid">
              {ship.numberOfLasers > 0 && (
                <div className="weapon-detail-item">
                  <div className="weapon-icon-detail laser">
                    <svg viewBox="0 0 24 24" fill="currentColor">
                      <path d="M12 2L3 14h4v8h10v-8h4L12 2zm0 4l5 8h-3v6H10v-6H7l5-8z"/>
                    </svg>
                  </div>
                  <div className="weapon-info">
                    <span className="weapon-name">Lasery</span>
                    <span className="weapon-count-detail">{ship.numberOfLasers}</span>
                  </div>
                </div>
              )}
              {ship.numberOfMissiles > 0 && (
                <div className="weapon-detail-item">
                  <div className="weapon-icon-detail missile">
                    <svg viewBox="0 0 24 24" fill="currentColor">
                      <path d="M2.81 14.12L5.64 11.29L8.17 10.79C11.39 6.41 17.29 4.23 19.76 5.17C21.78 6.47 21.06 11.87 17.77 15.89L17.33 18.46L14.5 21.29L12.62 15.5L7.5 14.39L2.81 14.12M15.55 7.6C16.77 7.6 17.76 8.59 17.76 9.81C17.76 11.03 16.77 12.02 15.55 12.02C14.33 12.02 13.34 11.03 13.34 9.81C13.34 8.59 14.33 7.6 15.55 7.6M5.08 12.41L6.94 14.27L2.81 15.73L5.08 12.41M11.73 18.53L13.59 20.39L10.27 16.26L11.73 18.53Z"/>
                    </svg>
                  </div>
                  <div className="weapon-info">
                    <span className="weapon-name">Pociski</span>
                    <span className="weapon-count-detail">{ship.numberOfMissiles}</span>
                  </div>
                </div>
              )}
              {ship.numberOfPointsDefense > 0 && (
                <div className="weapon-detail-item">
                  <div className="weapon-icon-detail defense">
                    <svg viewBox="0 0 24 24" fill="currentColor">
                      <path d="M12,1L3,5V11C3,16.55 6.84,21.74 12,23C17.16,21.74 21,16.55 21,11V5L12,1M12,5A3,3 0 0,1 15,8A3,3 0 0,1 12,11A3,3 0 0,1 9,8A3,3 0 0,1 12,5M17.13,17C15.92,18.85 14.11,20.24 12,20.92C9.89,20.24 8.08,18.85 6.87,17C6.53,16.5 6.24,16 6,15.47C6,13.82 8.71,12.47 12,12.47C15.29,12.47 18,13.79 18,15.47C17.76,16 17.47,16.5 17.13,17Z"/>
                    </svg>
                  </div>
                  <div className="weapon-info">
                    <span className="weapon-name">Obrona Punktowa</span>
                    <span className="weapon-count-detail">{ship.numberOfPointsDefense}</span>
                  </div>
                </div>
              )}
            </div>
            {ship.numberOfLasers === 0 && ship.numberOfMissiles === 0 && ship.numberOfPointsDefense === 0 && (
              <p className="no-weapons">Brak uzbrojenia</p>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

ShipDetailsModal.propTypes = {
  ship: PropTypes.shape({
    shipId: PropTypes.string.isRequired,
    name: PropTypes.string.isRequired,
    type: PropTypes.string.isRequired,
    hitPoints: PropTypes.number,
    shields: PropTypes.number,
    armor: PropTypes.number,
    speed: PropTypes.number,
    x: PropTypes.number,
    y: PropTypes.number,
    numberOfModules: PropTypes.number,
    numberOfLasers: PropTypes.number,
    numberOfMissiles: PropTypes.number,
    numberOfPointsDefense: PropTypes.number,
    status: PropTypes.string,
  }).isRequired,
  onClose: PropTypes.func.isRequired,
  fractionColor: PropTypes.string,
};
