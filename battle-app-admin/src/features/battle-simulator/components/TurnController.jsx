import { useState } from 'react';
import PropTypes from 'prop-types';
import { executeTurn } from '../../../services/api';
import './TurnController.css';

/**
 * Komponent kontrolujący przebieg tury
 * Pozwala na zatwierdzenie rozkazów i wykonanie tury
 */
export const TurnController = ({ 
  battleId,
  battleState,
  orders,
  onSubmitOrders,
  onTurnExecuted,
  onRefreshBattle,
}) => {
  const [isExecuting, setIsExecuting] = useState(false);
  const [executionError, setExecutionError] = useState(null);

  if (!battleState) {
    return null;
  }

  const isPreparation = battleState.status === 'Preparation';
  const isInProgress = battleState.status === 'InProgress';

  const handleExecuteTurn = async () => {
    try {
      setIsExecuting(true);
      setExecutionError(null);

      // Wykonaj turę
      const updatedBattle = await executeTurn(battleId);
      
      // Notify parent component
      onTurnExecuted?.(updatedBattle);
      
      // Refresh battle state
      await onRefreshBattle?.();
      
    } catch (error) {
      const errorMsg = error.response?.data?.message || error.message || 'Failed to execute turn';
      setExecutionError(errorMsg);
      console.error('Error executing turn:', error);
    } finally {
      setIsExecuting(false);
    }
  };

  const getOrdersSummary = () => {
    const moves = orders.filter(o => o.type === 'move').length;
    const lasers = orders.filter(o => o.type === 'laser').length;
    const missiles = orders.filter(o => o.type === 'missile').length;

    const parts = [];
    if (moves > 0) parts.push(`${moves} ruch${moves === 1 ? '' : 'ów'}`);
    if (lasers > 0) parts.push(`${lasers} laser${lasers === 1 ? '' : 'ów'}`);
    if (missiles > 0) parts.push(`${missiles} rakiet${missiles === 1 ? 'a' : missiles < 5 ? 'y' : ''}`);

    return parts.length > 0 ? parts.join(', ') : 'brak rozkazów';
  };

  return (
    <div className="turn-controller">
      <div className="turn-info">
        <div className="turn-number">
          <span className="label">Tura:</span>
          <span className="value">{battleState.turnNumber}</span>
        </div>
        <div className="battle-status">
          <span className="label">Status:</span>
          <span className={`status-badge ${battleState.status.toLowerCase()}`}>
            {battleState.status}
          </span>
        </div>
      </div>

      {isPreparation && (
        <div className="preparation-info">
          <h4>🔧 Tryb przygotowania</h4>
          <p>Kliknij w swój statek, a następnie w puste pole, aby go przesunąć.</p>
        </div>
      )}

      {isInProgress && (
        <>
          <div className="orders-summary">
            <h4>Zaplanowane rozkazy:</h4>
            <p className="summary-text">{getOrdersSummary()}</p>
            {orders.length > 0 && (
              <button 
                className="submit-orders-btn"
                onClick={onSubmitOrders}
                disabled={isExecuting}
              >
                Zatwierdź rozkazy
              </button>
            )}
          </div>

          <div className="turn-actions">
            <button 
              className="execute-turn-btn"
              onClick={handleExecuteTurn}
              disabled={isExecuting || isPreparation}
            >
              {isExecuting ? (
                <>
                  <span className="spinner"></span>
                  Wykonywanie tury...
                </>
              ) : (
                'Wykonaj turę'
              )}
            </button>
            
            <button 
              className="refresh-btn"
              onClick={onRefreshBattle}
              disabled={isExecuting}
            >
              🔄 Odśwież
            </button>
          </div>
        </>
      )}

      {executionError && (
        <div className="error-message">
          <strong>Błąd:</strong> {executionError}
        </div>
      )}

      <div className="fractions-status">
        <h4>Status frakcji:</h4>
        {battleState.fractions.map((fraction, index) => (
          <div key={fraction.fractionId} className="fraction-status">
            <div 
              className="fraction-color" 
              style={{ 
                backgroundColor: ['#4CAF50', '#F44336', '#2196F3', '#FF9800', '#9C27B0'][index % 5] 
              }}
            />
            <span className="fraction-name">{fraction.fractionName}</span>
            <span className="ships-count">
              {fraction.ships.length} statek{fraction.ships.length === 1 ? '' : fraction.ships.length < 5 ? 'i' : 'ów'}
            </span>
            {fraction.isDefeated && (
              <span className="defeated-badge">Pokonana</span>
            )}
          </div>
        ))}
      </div>
    </div>
  );
};

TurnController.propTypes = {
  battleId: PropTypes.string.isRequired,
  battleState: PropTypes.shape({
    turnNumber: PropTypes.number.isRequired,
    status: PropTypes.string.isRequired,
    fractions: PropTypes.arrayOf(PropTypes.shape({
      fractionId: PropTypes.string.isRequired,
      fractionName: PropTypes.string.isRequired,
      ships: PropTypes.array.isRequired,
      isDefeated: PropTypes.bool,
    })).isRequired,
  }),
  orders: PropTypes.arrayOf(PropTypes.object).isRequired,
  onSubmitOrders: PropTypes.func,
  onTurnExecuted: PropTypes.func,
  onRefreshBattle: PropTypes.func,
};
