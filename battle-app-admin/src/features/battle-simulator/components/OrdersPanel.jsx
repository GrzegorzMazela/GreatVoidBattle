import PropTypes from 'prop-types';
import './OrdersPanel.css';

/**
 * Panel z listą rozkazów dla danej tury
 * Wyświetla wszystkie rozkazy gracza z możliwością ich anulowania
 */
export const OrdersPanel = ({ 
  orders, 
  ships,
  onRemoveOrder
}) => {
  if (!orders || orders.length === 0) {
    return (
      <div className="orders-panel">
        <div className="orders-header">Rozkazy</div>
        <div className="orders-empty">
          <p>Brak rozkazów</p>
          <p className="orders-hint">Wybierz statek i wydaj rozkaz</p>
        </div>
      </div>
    );
  }

  // Funkcja pomocnicza do znajdowania nazwy statku
  const getShipName = (shipId) => {
    const ship = ships.find(s => s.shipId === shipId);
    return ship?.name || `Statek ${shipId.substring(0, 8)}`;
  };

  // Funkcja do obliczenia liczby tur do osiągnięcia celu
  const calculateTurnsToTarget = (order, ship) => {
    if (!ship) return '?';
    
    if (order.type === 'move') {
      // Oblicz dystans Manhattan
      const distance = Math.abs(order.targetX - ship.x) + Math.abs(order.targetY - ship.y);
      const speed = ship.speed || 5; // Domyślna prędkość
      return Math.ceil(distance / speed);
    } else if (order.type === 'missile') {
      // Rakiety poruszają się z własną prędkością
      const MISSILE_SPEED = 10; // Przykładowa prędkość rakiety
      const targetShip = ships.find(s => s.shipId === order.targetShipId);
      if (!targetShip) return '?';
      
      const distance = Math.abs(targetShip.x - ship.x) + Math.abs(targetShip.y - ship.y);
      return Math.ceil(distance / MISSILE_SPEED);
    }
    
    return 1; // Laser trafia natychmiast
  };

  // Formatowanie opisu rozkazu
  const formatOrderDescription = (order) => {
    const shipName = getShipName(order.shipId);
    const ship = ships.find(s => s.shipId === order.shipId);
    const turns = calculateTurnsToTarget(order, ship);
    
    switch (order.type) {
      case 'move':
        return {
          icon: '🚀',
          text: `${shipName} → ruch do (${order.targetX}, ${order.targetY})`,
          turns: `${turns} ${turns === 1 ? 'tura' : turns < 5 ? 'tury' : 'tur'}`,
          class: 'move'
        };
      case 'laser': {
        const laserTarget = getShipName(order.targetShipId);
        return {
          icon: '⚡',
          text: `${shipName} → laser w ${laserTarget}`,
          turns: 'natychmiast',
          class: 'laser'
        };
      }
      case 'missile': {
        const missileTarget = getShipName(order.targetShipId);
        return {
          icon: '🚀',
          text: `${shipName} → rakieta w ${missileTarget}`,
          turns: `${turns} ${turns === 1 ? 'tura' : turns < 5 ? 'tury' : 'tur'}`,
          class: 'missile'
        };
      }
      default:
        return {
          icon: '❓',
          text: `${shipName} → nieznany rozkaz`,
          turns: '?',
          class: 'unknown'
        };
    }
  };

  return (
    <div className="orders-panel">
      <div className="orders-header">
        Rozkazy
        <span className="orders-count">{orders.length}</span>
      </div>
      
      <div className="orders-list">
        {orders.map((order, globalIndex) => {
          const orderInfo = formatOrderDescription(order);
          
          return (
            <div 
              key={`${order.shipId}-${globalIndex}`} 
              className={`order-item ${orderInfo.class}`}
            >
              <div className="order-icon">{orderInfo.icon}</div>
              <div className="order-content">
                <div className="order-text">{orderInfo.text}</div>
                <div className="order-turns">{orderInfo.turns}</div>
              </div>
              <button
                className="order-remove"
                onClick={() => onRemoveOrder(globalIndex)}
                title="Anuluj rozkaz"
              >
                ✕
              </button>
            </div>
          );
        })}
      </div>
    </div>
  );
};

OrdersPanel.propTypes = {
  orders: PropTypes.arrayOf(PropTypes.shape({
    shipId: PropTypes.string.isRequired,
    type: PropTypes.oneOf(['move', 'laser', 'missile']).isRequired,
    targetX: PropTypes.number,
    targetY: PropTypes.number,
    targetShipId: PropTypes.string,
    targetFractionId: PropTypes.string,
  })).isRequired,
  ships: PropTypes.arrayOf(PropTypes.shape({
    shipId: PropTypes.string.isRequired,
    name: PropTypes.string.isRequired,
    x: PropTypes.number.isRequired,
    y: PropTypes.number.isRequired,
    speed: PropTypes.number,
  })).isRequired,
  onRemoveOrder: PropTypes.func.isRequired,
};
