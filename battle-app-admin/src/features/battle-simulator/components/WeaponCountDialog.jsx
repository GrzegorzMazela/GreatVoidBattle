import { useState } from 'react';
import PropTypes from 'prop-types';
import './WeaponCountDialog.css';

/**
 * Dialog do wyboru liczby wystrzeliwanych pocisków
 */
export const WeaponCountDialog = ({
  weaponType,
  maxCount,
  targetShipName,
  onConfirm,
  onCancel,
}) => {
  const [count, setCount] = useState(1);

  const handleConfirm = () => {
    if (count > 0 && count <= maxCount) {
      onConfirm(count);
    }
  };

  const handleKeyDown = (e) => {
    if (e.key === 'Enter') {
      handleConfirm();
    } else if (e.key === 'Escape') {
      onCancel();
    }
  };

  const weaponIcon = weaponType === 'missile' ? '🚀' : '⚡';
  const weaponName = weaponType === 'missile' ? 'Rakiety' : 'Lasery';

  return (
    <div className="weapon-dialog-overlay" onClick={onCancel}>
      <div className="weapon-dialog" onClick={(e) => e.stopPropagation()}>
        <div className="weapon-dialog-header">
          <h3>
            <span className="weapon-dialog-icon">{weaponIcon}</span>
            {weaponName}
          </h3>
        </div>

        <div className="weapon-dialog-content">
          <p className="target-info">
            Cel: <strong>{targetShipName}</strong>
          </p>

          <div className="count-selector">
            <label>Liczba strzałów:</label>
            <div className="count-controls">
              <button
                className="count-btn"
                onClick={() => setCount(Math.max(1, count - 1))}
                disabled={count <= 1}
              >
                −
              </button>
              
              <input
                type="number"
                min="1"
                max={maxCount}
                value={count}
                onChange={(e) => {
                  const value = parseInt(e.target.value) || 1;
                  setCount(Math.min(maxCount, Math.max(1, value)));
                }}
                onKeyDown={handleKeyDown}
                autoFocus
              />
              
              <button
                className="count-btn"
                onClick={() => setCount(Math.min(maxCount, count + 1))}
                disabled={count >= maxCount}
              >
                +
              </button>
            </div>
            <span className="count-info">Dostępne: {maxCount}</span>
          </div>
        </div>

        <div className="weapon-dialog-actions">
          <button className="dialog-btn cancel-btn" onClick={onCancel}>
            Anuluj
          </button>
          <button 
            className="dialog-btn confirm-btn" 
            onClick={handleConfirm}
            disabled={count < 1 || count > maxCount}
          >
            Potwierdź ({count})
          </button>
        </div>
      </div>
    </div>
  );
};

WeaponCountDialog.propTypes = {
  weaponType: PropTypes.oneOf(['missile', 'laser']).isRequired,
  maxCount: PropTypes.number.isRequired,
  targetShipName: PropTypes.string.isRequired,
  onConfirm: PropTypes.func.isRequired,
  onCancel: PropTypes.func.isRequired,
};
