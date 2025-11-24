import { useMemo, useState } from 'react';
import { createPortal } from 'react-dom';
import PropTypes from 'prop-types';
import './ShipDock.css';

// Import ship icons
import CorvetteIcon from '../../../assets/Corvette_64.png';
import DestroyerIcon from '../../../assets/Destroyer_64.png';
import CruiserIcon from '../../../assets/Cruiser_64.png';
import BattleshipIcon from '../../../assets/Battleship_64.png';
import SuperBattleshipIcon from '../../../assets/SuperBattleship_64.png';
import OrbitalFortIcon from '../../../assets/OrbitalFort_64.png';

// Map ship types to icons
const SHIP_ICONS = {
  Corvette: CorvetteIcon,
  Destroyer: DestroyerIcon,
  Cruiser: CruiserIcon,
  Battleship: BattleshipIcon,
  SuperBattleship: SuperBattleshipIcon,
  OrbitalFort: OrbitalFortIcon,
};

// Map ship types to max HP
const SHIP_MAX_HP = {
  Corvette: 50,
  Destroyer: 100,
  Cruiser: 200,
  Battleship: 400,
  SuperBattleship: 600,
  OrbitalFort: 100,
};

/**
 * Dolny panel z ikonami statków gracza
 * Umożliwia szybki wybór statku i broni
 */
export const ShipDock = ({
  playerShips,
  selectedShip,
  onShipSelect,
  weaponMode,
  onWeaponModeChange,
  missileFiredCounts,
  laserFiredCounts,
  battleStatus,
}) => {
  const [hoveredShip, setHoveredShip] = useState(null);
  const [tooltipPosition, setTooltipPosition] = useState({ x: 0, y: 0 });

  // Sortuj statki według typu (większe statki najpierw)
  const sortedShips = useMemo(() => {
    const typeOrder = ['OrbitalFort', 'SuperBattleship', 'Battleship', 'Cruiser', 'Destroyer', 'Corvette'];
    return [...playerShips].sort((a, b) => {
      return typeOrder.indexOf(a.type) - typeOrder.indexOf(b.type);
    });
  }, [playerShips]);

  if (!playerShips || playerShips.length === 0) {
    return null;
  }

  const handleMouseEnter = (ship, event) => {
    const rect = event.currentTarget.getBoundingClientRect();
    setHoveredShip(ship);
    
    // Oszacuj szerokość tooltipa (min-width: 250px z CSS)
    const tooltipWidth = 250;
    let tooltipX = rect.left + rect.width / 2;
    
    // Sprawdź czy tooltip wychodzi poza lewą krawędź
    if (tooltipX - tooltipWidth / 2 < 10) {
      tooltipX = tooltipWidth / 2 + 10;
    }
    
    // Sprawdź czy tooltip wychodzi poza prawą krawędź
    if (tooltipX + tooltipWidth / 2 > window.innerWidth - 10) {
      tooltipX = window.innerWidth - tooltipWidth / 2 - 10;
    }
    
    setTooltipPosition({
      x: tooltipX,
      y: rect.top - 10,
    });
  };

  const handleMouseLeave = () => {
    setHoveredShip(null);
  };

  return (
    <div className="ship-dock">
      <div className="ship-dock-content">
        {/* Sekcja statków */}
        <div className="ship-dock-ships">
          <div className="ship-dock-ships-list">
            {sortedShips.map(ship => {
              const isSelected = selectedShip?.shipId === ship.shipId;
              const shipIcon = SHIP_ICONS[ship.type];
              const maxHp = SHIP_MAX_HP[ship.type] || 100;
              const hpPercent = (ship.hitPoints / maxHp) * 100;
              const shieldPercent = ship.shields ? (ship.shields / 100) * 100 : 0;
              const armorPercent = ship.armor ? (ship.armor / 100) * 100 : 0;
              
              return (
                <div
                  key={ship.shipId}
                  className={`ship-dock-item ${isSelected ? 'selected' : ''}`}
                  onClick={() => onShipSelect(ship)}
                  onMouseEnter={(e) => handleMouseEnter(ship, e)}
                  onMouseLeave={handleMouseLeave}
                  title={`${ship.name} (${ship.type})`}
                >
                  <div className="ship-dock-icon-wrapper">
                    {shipIcon && (
                      <img 
                        src={shipIcon} 
                        alt={ship.type} 
                        className="ship-dock-icon"
                      />
                    )}
                    {/* HP bar */}
                    <div className="ship-dock-hp-bar">
                      <div 
                        className="ship-dock-hp-fill"
                        style={{ 
                          width: `${hpPercent}%`,
                          backgroundColor: hpPercent > 50 ? '#4CAF50' : hpPercent > 25 ? '#FF9800' : '#F44336'
                        }}
                      />
                    </div>
                    {/* Shields bar */}
                    {ship.shields !== undefined && ship.shields > 0 && (
                      <div className="ship-dock-shield-bar">
                        <div 
                          className="ship-dock-shield-fill"
                          style={{ width: `${shieldPercent}%` }}
                        />
                      </div>
                    )}
                    {/* Armor bar */}
                    {ship.armor !== undefined && ship.armor > 0 && (
                      <div className="ship-dock-armor-bar">
                        <div 
                          className="ship-dock-armor-fill"
                          style={{ width: `${armorPercent}%` }}
                        />
                      </div>
                    )}
                  </div>
                  <div className="ship-dock-name">{ship.name || ship.type}</div>
                </div>
              );
            })}
          </div>
        </div>

        {/* Separator */}
        <div className="ship-dock-separator" />

        {/* Sekcja broni - wyświetlana tylko gdy jest wybrany statek i nie jest w trybie Preparation */}
        {selectedShip && battleStatus !== 'Preparation' && (
          <div className="ship-dock-weapons">
            <div className="ship-dock-weapons-list">
              {/* Rakiety */}
              <button
                className={`ship-dock-weapon-btn ${weaponMode === 'missile' ? 'active' : ''}`}
                onClick={() => onWeaponModeChange(weaponMode === 'missile' ? null : 'missile')}
                disabled={!selectedShip.numberOfMissiles || 
                  (missileFiredCounts[selectedShip.shipId] || 0) >= selectedShip.numberOfMissiles}
                title={`Rakiety: ${missileFiredCounts[selectedShip.shipId] || 0}/${selectedShip.numberOfMissiles || 0} (zasięg 35-55)`}
              >
                <span className="weapon-icon">🚀</span>
                <span className="weapon-count">
                  {missileFiredCounts[selectedShip.shipId] || 0}/{selectedShip.numberOfMissiles || 0}
                </span>
              </button>

              {/* Lasery */}
              <button
                className={`ship-dock-weapon-btn ${weaponMode === 'laser' ? 'active' : ''}`}
                onClick={() => onWeaponModeChange(weaponMode === 'laser' ? null : 'laser')}
                disabled={!selectedShip.numberOfLasers || 
                  (laserFiredCounts[selectedShip.shipId] || 0) >= selectedShip.numberOfLasers}
                title={`Lasery: ${laserFiredCounts[selectedShip.shipId] || 0}/${selectedShip.numberOfLasers || 0} (zasięg 0-20)`}
              >
                <span className="weapon-icon">⚡</span>
                <span className="weapon-count">
                  {laserFiredCounts[selectedShip.shipId] || 0}/{selectedShip.numberOfLasers || 0}
                </span>
              </button>
            </div>
          </div>
        )}
      </div>

      {/* Tooltip ze statystykami statku - renderowany przez Portal */}
      {hoveredShip && createPortal(
        <div 
          className="ship-dock-tooltip"
          style={{
            left: `${tooltipPosition.x}px`,
            top: `${tooltipPosition.y}px`,
          }}
        >
          <div className="ship-dock-tooltip-header">
            <h4>{hoveredShip.name || hoveredShip.type}</h4>
            <span className="ship-dock-tooltip-type">{hoveredShip.type}</span>
          </div>
          <div className="ship-dock-tooltip-stats">
            <div className="tooltip-stat-row">
              <span className="tooltip-stat-label">HP:</span>
              <span className="tooltip-stat-value">{hoveredShip.hitPoints} / {SHIP_MAX_HP[hoveredShip.type]}</span>
            </div>
            {hoveredShip.shields !== undefined && (
              <div className="tooltip-stat-row">
                <span className="tooltip-stat-label">Tarcze:</span>
                <span className="tooltip-stat-value" style={{ color: '#2196F3' }}>{hoveredShip.shields}</span>
              </div>
            )}
            {hoveredShip.armor !== undefined && (
              <div className="tooltip-stat-row">
                <span className="tooltip-stat-label">Pancerz:</span>
                <span className="tooltip-stat-value" style={{ color: '#9E9E9E' }}>{hoveredShip.armor}</span>
              </div>
            )}
            <div className="tooltip-stat-row">
              <span className="tooltip-stat-label">Prędkość:</span>
              <span className="tooltip-stat-value">{hoveredShip.speed || 0}</span>
            </div>
            {hoveredShip.numberOfMissiles !== undefined && (
              <div className="tooltip-stat-row">
                <span className="tooltip-stat-label">Rakiety:</span>
                <span className="tooltip-stat-value">
                  {(hoveredShip.numberOfMissiles || 0) - (missileFiredCounts[hoveredShip.shipId] || 0)} / {hoveredShip.numberOfMissiles || 0}
                </span>
              </div>
            )}
            {hoveredShip.numberOfLasers !== undefined && (
              <div className="tooltip-stat-row">
                <span className="tooltip-stat-label">Lasery:</span>
                <span className="tooltip-stat-value">
                  {(hoveredShip.numberOfLasers || 0) - (laserFiredCounts[hoveredShip.shipId] || 0)} / {hoveredShip.numberOfLasers || 0}
                </span>
              </div>
            )}
            {hoveredShip.numberOfPointsDefense !== undefined && (
              <div className="tooltip-stat-row">
                <span className="tooltip-stat-label">Obrona PD:</span>
                <span className="tooltip-stat-value">{hoveredShip.numberOfPointsDefense}</span>
              </div>
            )}
            <div className="tooltip-stat-row">
              <span className="tooltip-stat-label">Pozycja:</span>
              <span className="tooltip-stat-value">({Math.floor(hoveredShip.x)}, {Math.floor(hoveredShip.y)})</span>
            </div>
          </div>
        </div>,
        document.body
      )}
    </div>
  );
};

ShipDock.propTypes = {
  playerShips: PropTypes.arrayOf(PropTypes.shape({
    shipId: PropTypes.string.isRequired,
    name: PropTypes.string,
    type: PropTypes.string.isRequired,
    hitPoints: PropTypes.number.isRequired,
    numberOfMissiles: PropTypes.number,
    numberOfLasers: PropTypes.number,
  })).isRequired,
  selectedShip: PropTypes.object,
  onShipSelect: PropTypes.func.isRequired,
  weaponMode: PropTypes.oneOf(['missile', 'laser', null]),
  onWeaponModeChange: PropTypes.func.isRequired,
  missileFiredCounts: PropTypes.object.isRequired,
  laserFiredCounts: PropTypes.object.isRequired,
  battleStatus: PropTypes.string.isRequired,
};
