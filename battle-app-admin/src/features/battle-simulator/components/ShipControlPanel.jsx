import PropTypes from 'prop-types';
import './ShipControlPanel.css';

/**
 * Panel kontrolny dla wybranego statku
 * Pokazuje informacje o statku i pozwala anulować rozkazy
 */
export const ShipControlPanel = ({ 
  selectedShip, 
  selectedFraction,
  currentOrder,
  onClearOrder,
  battleStatus,
}) => {
  if (!selectedShip || !selectedFraction) {
    const isPreparation = battleStatus === 'Preparation';
    
    return (
      <div className="ship-control-panel empty">
        <p>Wybierz statek, aby {isPreparation ? 'go przesunąć' : 'wydać rozkaz'}</p>
        <div className="instructions">
          <h4>Instrukcje:</h4>
          {isPreparation ? (
            <ul>
              <li><strong>Kliknij statek</strong> - wybierz go</li>
              <li><strong>Kliknij puste pole</strong> - przesuń statek</li>
              <li><strong>Scroll</strong> - zoom</li>
              <li><strong>Ctrl + przeciągnij</strong> - przesuń widok</li>
            </ul>
          ) : (
            <ul>
              <li><strong>Kliknij statek</strong> - wybierz go</li>
              <li><strong>Kliknij puste pole</strong> - wydaj rozkaz ruchu</li>
              <li><strong>Kliknij wrogi statek</strong> - wydaj rozkaz ataku</li>
              <li><strong>Scroll</strong> - zoom</li>
              <li><strong>Ctrl + przeciągnij</strong> - przesuń widok</li>
            </ul>
          )}
        </div>
      </div>
    );
  }

  const getOrderDescription = () => {
    if (!currentOrder) return 'Brak rozkazów';

    switch (currentOrder.type) {
      case 'move':
        return `Ruch do (${currentOrder.targetX}, ${currentOrder.targetY})`;
      case 'laser':
        return 'Strzał laserowy';
      case 'missile':
        return 'Strzał rakietowy';
      default:
        return 'Nieznany rozkaz';
    }
  };

  const hpPercent = (selectedShip.hitPoints / 100) * 100;

  return (
    <div className="ship-control-panel">
      <div className="ship-info">
        <h3>{selectedShip.name}</h3>
        <div className="fraction-badge" style={{ 
          backgroundColor: selectedFraction.color || '#4CAF50' 
        }}>
          {selectedFraction.fractionName}
        </div>
      </div>

      <div className="ship-stats">
        <div className="stat-row">
          <span className="stat-label">Pozycja:</span>
          <span className="stat-value">
            ({Math.floor(selectedShip.x)}, {Math.floor(selectedShip.y)})
          </span>
        </div>

        <div className="stat-row">
          <span className="stat-label">Prędkość:</span>
          <span className="stat-value">{selectedShip.speed || 0}</span>
        </div>

        <div className="stat-row">
          <span className="stat-label">HP:</span>
          <div className="stat-bar-container">
            <div 
              className="stat-bar hp" 
              style={{ 
                width: `${hpPercent}%`,
                backgroundColor: hpPercent > 50 ? '#4CAF50' : hpPercent > 25 ? '#FF9800' : '#F44336'
              }}
            />
            <span className="stat-bar-text">{selectedShip.hitPoints}</span>
          </div>
        </div>

        {selectedShip.shields !== undefined && (
          <div className="stat-row">
            <span className="stat-label">Tarcze:</span>
            <span className="stat-value">{selectedShip.shields}</span>
          </div>
        )}

        {selectedShip.armor !== undefined && (
          <div className="stat-row">
            <span className="stat-label">Pancerz:</span>
            <span className="stat-value">{selectedShip.armor}</span>
          </div>
        )}
      </div>

      {battleStatus === 'Preparation' ? (
        <div className="action-hints">
          <p><strong>Tryb przygotowania:</strong></p>
          <ul>
            <li>Kliknij puste pole - przesuń statek</li>
          </ul>
          <p className="hint-note">💡 Niebieski obszar pokazuje zasięg ruchu</p>
        </div>
      ) : (
        <>
          <div className="current-order">
            <h4>Aktualny rozkaz:</h4>
            <div className="order-info">
              <span className={currentOrder ? 'has-order' : 'no-order'}>
                {getOrderDescription()}
              </span>
              {currentOrder && (
                <button 
                  className="clear-order-btn"
                  onClick={onClearOrder}
                  title="Anuluj rozkaz"
                >
                  ✕
                </button>
              )}
            </div>
          </div>

          <div className="action-hints">
            <p><strong>Kliknij:</strong></p>
            <ul>
              <li>Puste pole w zasięgu - wydaj rozkaz ruchu</li>
              <li>Wrogi statek - zaatakuj</li>
            </ul>
            <p className="hint-note">💡 Niebieski obszar pokazuje zasięg ruchu</p>
          </div>
        </>
      )}
    </div>
  );
};

ShipControlPanel.propTypes = {
  selectedShip: PropTypes.shape({
    shipId: PropTypes.string.isRequired,
    name: PropTypes.string.isRequired,
    x: PropTypes.number.isRequired,
    y: PropTypes.number.isRequired,
    speed: PropTypes.number,
    hitPoints: PropTypes.number,
    shields: PropTypes.number,
    armor: PropTypes.number,
  }),
  selectedFraction: PropTypes.shape({
    fractionId: PropTypes.string.isRequired,
    fractionName: PropTypes.string.isRequired,
    color: PropTypes.string,
  }),
  currentOrder: PropTypes.shape({
    type: PropTypes.string.isRequired,
    targetX: PropTypes.number,
    targetY: PropTypes.number,
  }),
  onClearOrder: PropTypes.func,
  battleStatus: PropTypes.string,
};
