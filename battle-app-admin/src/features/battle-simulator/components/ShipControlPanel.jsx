import PropTypes from 'prop-types';
import './ShipControlPanel.css';

// Map ship types to max HP (from ShipFactory.cs)
const SHIP_MAX_HP = {
  Corvette: 50,
  Destroyer: 100,
  Cruiser: 200,
  Battleship: 400,
  SuperBattleship: 600,
  OrbitalFort: 100,
};

/**
 * Panel kontrolny dla wybranego statku
 * Pokazuje informacje o statku i pozwala anulować rozkazy
 */
export const ShipControlPanel = ({ 
  selectedShip, 
  selectedFraction,
  currentOrder,
  orderStats,
  onClearOrder,
  battleStatus,
  weaponMode,
  onWeaponModeChange,
  missileFiredCount,
  laserFiredCount,
  isPlayerShip,
  allOrders,
  onSubmitOrders,
}) => {
  if (!selectedShip || !selectedFraction) {
    return (
      <div className="ship-control-panel empty">
        <p>Wybierz statek, aby zobaczyć szczegóły</p>
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

  const hpPercent = (selectedShip.hitPoints / (SHIP_MAX_HP[selectedShip.type] || 100)) * 100;

  // Oblicz dostępną broń
  const totalMissiles = selectedShip.numberOfMissiles || 0;
  const totalLasers = selectedShip.numberOfLasers || 0;
  const pointDefense = selectedShip.numberOfPointsDefense || 0;
  const availableMissiles = totalMissiles - (missileFiredCount || 0);
  const availableLasers = totalLasers - (laserFiredCount || 0);

  return (
    <div className="ship-control-panel">
      <div className="ship-info">
        <h3>{selectedShip.name}</h3>
        <div className="ship-type-badge">
          {selectedShip.type}
        </div>
        <div className="fraction-badge" style={{ 
          backgroundColor: selectedFraction.color || '#4CAF50' 
        }}>
          {selectedFraction.fractionName}
        </div>
      </div>

      {/* Pozycja - widoczna dla wszystkich */}
      <div className="ship-stats">
        <div className="stat-row">
          <span className="stat-label">Pozycja:</span>
          <span className="stat-value">
            ({Math.floor(selectedShip.x)}, {Math.floor(selectedShip.y)})
          </span>
        </div>
      </div>

      {/* Szczegółowe statystyki - tylko dla statków gracza */}
      {!isPlayerShip && (
        <div className="enemy-ship-notice">
          <p>⚠️ Statek przeciwnika - szczegóły ukryte</p>
        </div>
      )}

      {isPlayerShip && (
        <>
          {/* Sekcja broni - tylko dla statków gracza */}
          {battleStatus !== 'Preparation' && (
            <div className="weapon-controls">
              <h4>Broń:</h4>
              <div className="weapon-buttons">
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
                <div className="weapon-hint">
                  {weaponMode === 'missile' ? (
                    <p>💡 Zasięg rakiet: 35-55</p>
                  ) : (
                    <p>💡 Zasięg laserów: 0-20</p>
                  )}
                </div>
              )}
            </div>
          )}

          <div className="ship-stats">
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

            <div className="stat-row">
              <span className="stat-label">Obrona (PD):</span>
              <span className="stat-value">{pointDefense}</span>
            </div>
          </div>
        </>
      )}

      {battleStatus === 'Preparation' ? (
        <div className="action-hints">
          {isPlayerShip && <p className="hint-note">💡 Niebieski obszar pokazuje zasięg ruchu</p>}
        </div>
      ) : (
        <>
          {/* Sekcja rozkazów - tylko dla statków gracza */}
          {isPlayerShip && (
            <>
              <div className="current-order">
                <h4>Rozkazy w tej turze:</h4>
                <div className="order-stats">
                  {orderStats && (
                    <>
                      {orderStats.moveOrders > 0 && (
                        <div className="order-stat-item">
                          <span className="order-stat-icon">🚶</span>
                          <span className="order-stat-label">Ruch:</span>
                          <span className="order-stat-value">{orderStats.moveOrders}</span>
                        </div>
                      )}
                      {orderStats.laserOrders > 0 && (
                        <div className="order-stat-item">
                          <span className="order-stat-icon">⚡</span>
                          <span className="order-stat-label">Lasery:</span>
                          <span className="order-stat-value">{orderStats.laserOrders}</span>
                        </div>
                      )}
                      {orderStats.missileOrders > 0 && (
                        <div className="order-stat-item">
                          <span className="order-stat-icon">🚀</span>
                          <span className="order-stat-label">Rakiety:</span>
                          <span className="order-stat-value">{orderStats.missileOrders}</span>
                        </div>
                      )}
                      {!orderStats.moveOrders && !orderStats.laserOrders && !orderStats.missileOrders && (
                        <span className="no-order">Brak rozkazów</span>
                      )}
                    </>
                  )}
                </div>

                {/* Przycisk zatwierdzenia rozkazów */}
                {allOrders && allOrders.length > 0 && (
                  <button 
                    className="submit-orders-btn"
                    onClick={onSubmitOrders}
                  >
                    Zatwierdź rozkazy
                  </button>
                )}
              </div>

              {/* Log wszystkich rozkazów z scrollowaniem */}
              <div className="orders-log">
                <h4>Log rozkazów:</h4>
                <div className="orders-log-content">
                  {allOrders && allOrders.length > 0 ? (
                    allOrders.map((order, index) => (
                      <div key={index} className="order-log-item">
                        <span className="order-log-index">#{index + 1}</span>
                        <span className="order-log-ship">{order.shipId.substring(0, 8)}</span>
                        {order.type === 'move' && (
                          <span className="order-log-details">
                            🚶 Ruch → ({order.targetX}, {order.targetY})
                          </span>
                        )}
                        {order.type === 'laser' && (
                          <span className="order-log-details">
                            ⚡ Laser → {order.targetShipId?.substring(0, 8)}
                          </span>
                        )}
                        {order.type === 'missile' && (
                          <span className="order-log-details">
                            🚀 Rakieta → {order.targetShipId?.substring(0, 8)}
                          </span>
                        )}
                      </div>
                    ))
                  ) : (
                    <p className="orders-log-empty">Brak rozkazów w tej turze</p>
                  )}
                </div>
              </div>
            </>
          )}
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
    numberOfMissiles: PropTypes.number,
    numberOfLasers: PropTypes.number,
    numberOfPointsDefense: PropTypes.number,
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
  orderStats: PropTypes.shape({
    moveOrders: PropTypes.number,
    laserOrders: PropTypes.number,
    missileOrders: PropTypes.number,
  }),
  onClearOrder: PropTypes.func,
  battleStatus: PropTypes.string,
  weaponMode: PropTypes.oneOf(['missile', 'laser', null]),
  onWeaponModeChange: PropTypes.func,
  missileFiredCount: PropTypes.number,
  laserFiredCount: PropTypes.number,
  isPlayerShip: PropTypes.bool,
  allOrders: PropTypes.arrayOf(PropTypes.shape({
    shipId: PropTypes.string.isRequired,
    type: PropTypes.string.isRequired,
    targetX: PropTypes.number,
    targetY: PropTypes.number,
    targetShipId: PropTypes.string,
  })),
  onSubmitOrders: PropTypes.func,
};
