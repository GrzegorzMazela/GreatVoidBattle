import { useState } from 'react';
import PropTypes from 'prop-types';
import { ShipCard } from './ShipCard';
import { ShipDetailsModal } from './ShipDetailsModal';
import './ShipBottomPanel.css';

/**
 * Dolny panel z listą jednostek gracza
 * Wyświetla karty statków z paskami zdrowia, tarczy i pancerza oraz ikonkami broni
 */
export const ShipBottomPanel = ({ 
  playerShips, 
  selectedShip, 
  onShipSelect,
  playerFractionColor,
  weaponMode,
  onWeaponModeChange,
}) => {
  const [hoveredShip, setHoveredShip] = useState(null);
  const [detailsShip, setDetailsShip] = useState(null);

  if (!playerShips || playerShips.length === 0) {
    return null;
  }

  const handleShipHover = (ship) => {
    setHoveredShip(ship);
  };

  const handleShipLeave = () => {
    setHoveredShip(null);
  };

  const handleShipClick = (ship) => {
    // Jeśli kliknięto ponownie w ten sam statek, pokaż modal
    if (selectedShip?.shipId === ship.shipId) {
      setDetailsShip(ship);
    } else {
      // Wybierz statek
      if (onShipSelect) {
        onShipSelect(ship);
      }
    }
  };

  const handleCloseModal = () => {
    setDetailsShip(null);
  };

  return (
    <>
      <div className="ship-bottom-panel">
        <div className="ship-panel-content">
          <div className="ship-cards-container">
          {playerShips.map((ship) => (
            <ShipCard
              key={ship.shipId}
              ship={ship}
              isSelected={selectedShip?.shipId === ship.shipId}
              isHovered={hoveredShip?.shipId === ship.shipId}
              onHover={handleShipHover}
              onLeave={handleShipLeave}
              onClick={handleShipClick}
              fractionColor={playerFractionColor}
            />
          ))}
          </div>
        </div>
        
        {/* Przyciski broni po prawej stronie */}
        {selectedShip && (
          <div className="weapon-controls">
            <button
              className={`weapon-btn laser ${weaponMode === 'laser' ? 'active' : ''}`}
              onClick={() => onWeaponModeChange(weaponMode === 'laser' ? null : 'laser')}
              disabled={!selectedShip.numberOfLasers || selectedShip.numberOfLasers === 0}
              title="Lasery"
            >
              <svg viewBox="0 0 24 24" fill="currentColor">
                <path d="M12 2L3 14h4v8h10v-8h4L12 2zm0 4l5 8h-3v6H10v-6H7l5-8z"/>
              </svg>
              <span>{selectedShip.numberOfLasers || 0}</span>
            </button>
            
            <button
              className={`weapon-btn missile ${weaponMode === 'missile' ? 'active' : ''}`}
              onClick={() => onWeaponModeChange(weaponMode === 'missile' ? null : 'missile')}
              disabled={!selectedShip.numberOfMissiles || selectedShip.numberOfMissiles === 0}
              title="Pociski"
            >
              <svg viewBox="0 0 24 24" fill="currentColor">
                <path d="M2.81 14.12L5.64 11.29L8.17 10.79C11.39 6.41 17.29 4.23 19.76 5.17C21.78 6.47 21.06 11.87 17.77 15.89L17.33 18.46L14.5 21.29L12.62 15.5L7.5 14.39L2.81 14.12M15.55 7.6C16.77 7.6 17.76 8.59 17.76 9.81C17.76 11.03 16.77 12.02 15.55 12.02C14.33 12.02 13.34 11.03 13.34 9.81C13.34 8.59 14.33 7.6 15.55 7.6M5.08 12.41L6.94 14.27L2.81 15.73L5.08 12.41M11.73 18.53L13.59 20.39L10.27 16.26L11.73 18.53Z"/>
              </svg>
              <span>{selectedShip.numberOfMissiles || 0}</span>
            </button>
          </div>
        )}
      </div>

      {detailsShip && (
        <ShipDetailsModal
          ship={detailsShip}
          onClose={handleCloseModal}
          fractionColor={playerFractionColor}
        />
      )}
    </>
  );
};

ShipBottomPanel.propTypes = {
  playerShips: PropTypes.arrayOf(PropTypes.shape({
    shipId: PropTypes.string.isRequired,
    name: PropTypes.string.isRequired,
    type: PropTypes.string.isRequired,
    hitPoints: PropTypes.number,
    shields: PropTypes.number,
    armor: PropTypes.number,
    numberOfLasers: PropTypes.number,
    numberOfMissiles: PropTypes.number,
    numberOfPointsDefense: PropTypes.number,
  })),
  selectedShip: PropTypes.object,
  onShipSelect: PropTypes.func,
  playerFractionColor: PropTypes.string,
  weaponMode: PropTypes.oneOf(['missile', 'laser', null]),
  onWeaponModeChange: PropTypes.func,
};
