import { useState, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useBattleState } from '../hooks/useBattleState';
import { useOrders } from '../hooks/useOrders';
import { setShipPosition } from '../../../services/api';
import { BattleCanvas } from './BattleCanvas';
import { ShipControlPanel } from './ShipControlPanel';
import { TurnController } from './TurnController';
import './BattleSimulator.css';

/**
 * Główny komponent symulatora bitwy
 * Łączy wszystkie elementy w jeden interfejs
 */
export const BattleSimulator = () => {
  const { battleId } = useParams();
  const navigate = useNavigate();
  
  const { battleState, loading, error, refresh } = useBattleState(battleId, false);
  
  // State dla wybranego statku i frakcji gracza
  const [selectedShip, setSelectedShip] = useState(null);
  const [selectedFraction, setSelectedFraction] = useState(null);
  const [playerFractionId, setPlayerFractionId] = useState(null);

  // Hook do zarządzania rozkazami
  const ordersManager = useOrders(
    battleId,
    playerFractionId,
    battleState?.turnNumber || 0
  );

  // Znajdź frakcję na podstawie statku
  const findFractionByShip = useCallback((shipId) => {
    if (!battleState) return null;
    
    for (const fraction of battleState.fractions) {
      if (fraction.ships.some(s => s.shipId === shipId)) {
        return fraction;
      }
    }
    return null;
  }, [battleState]);

  // Obsługa kliknięcia w statek
  const handleShipClick = useCallback((ship, fraction) => {
    setSelectedShip(ship);
    setSelectedFraction(fraction);
    
    // Jeśli to pierwszy wybór, ustaw frakcję gracza
    if (!playerFractionId) {
      setPlayerFractionId(fraction.fractionId);
    }
  }, [playerFractionId]);

  // Obsługa ruchu w trybie przygotowania
  const handleMoveShipInPreparation = useCallback(async (ship, fraction, x, y) => {
    try {
      await setShipPosition(battleId, fraction.fractionId, ship.shipId, x, y);
      await refresh();
      // Aktualizuj wybrany statek po przesunięciu
      setSelectedShip(prev => prev ? { ...prev, x, y } : null);
    } catch (error) {
      alert(`Błąd przy ustawianiu pozycji: ${error.response?.data?.message || error.message}`);
    }
  }, [battleId, refresh]);

  // Obsługa rozkazów w trybie rozgrywki
  const handleOrderInProgress = useCallback((x, y) => {
    if (!selectedShip) return;

    // Znajdź czy jest tam jakiś statek
    let clickedShip = null;
    let clickedFraction = null;

    for (const fraction of battleState.fractions) {
      for (const ship of fraction.ships) {
        if (Math.floor(ship.x) === x && Math.floor(ship.y) === y) {
          clickedShip = ship;
          clickedFraction = fraction;
          break;
        }
      }
      if (clickedShip) break;
    }

    if (clickedShip) {
      // Kliknięto w statek
      if (clickedFraction.fractionId === playerFractionId) {
        // Kliknięto w własny statek - wybierz go
        setSelectedShip(clickedShip);
        setSelectedFraction(clickedFraction);
      } else {
        // Kliknięto we wrogi statek - zaatakuj
        // Domyślnie laser (możemy to rozszerzyć o wybór)
        ordersManager.addLaserOrder(
          selectedShip.shipId,
          clickedShip.shipId,
          clickedFraction.fractionId
        );
      }
    } else {
      // Kliknięto w puste pole - ruch
      ordersManager.addMoveOrder(selectedShip.shipId, x, y);
    }
  }, [selectedShip, playerFractionId, battleState, ordersManager]);

  // Obsługa kliknięcia w komórkę
  const handleCellClick = useCallback((x, y) => {
    if (!selectedShip || !playerFractionId) {
      return;
    }

    // Sprawdź czy to statek gracza
    const shipFraction = findFractionByShip(selectedShip.shipId);
    if (!shipFraction || shipFraction.fractionId !== playerFractionId) {
      console.warn('Nie możesz wydawać rozkazów obcym statkom');
      return;
    }

    // TRYB PREPARATION - przemieść statek natychmiast
    if (battleState.status === 'Preparation') {
      handleMoveShipInPreparation(selectedShip, shipFraction, x, y);
      return;
    }

    // TRYB INPROGRESS - zaplanuj rozkazy
    if (battleState.status === 'InProgress') {
      handleOrderInProgress(x, y);
      return;
    }
  }, [selectedShip, playerFractionId, battleState, findFractionByShip, handleMoveShipInPreparation, handleOrderInProgress]);

  // Obsługa anulowania rozkazu
  const handleClearOrder = useCallback(() => {
    if (selectedShip) {
      ordersManager.removeOrder(selectedShip.shipId);
    }
  }, [selectedShip, ordersManager]);

  // Obsługa zatwierdzenia rozkazów
  const handleSubmitOrders = useCallback(async () => {
    const result = await ordersManager.submit();
    if (result.success) {
      alert('Rozkazy zostały zatwierdzone!');
      await refresh();
    } else {
      alert(`Błąd: ${result.error}`);
    }
  }, [ordersManager, refresh]);

  // Obsługa wykonania tury
  const handleTurnExecuted = useCallback(async (updatedBattle) => {
    console.log('Turn executed, new state:', updatedBattle);
    // Wyczyść wybór po wykonaniu tury
    setSelectedShip(null);
    setSelectedFraction(null);
  }, []);

  // Wybór frakcji gracza
  const handleSelectPlayerFraction = useCallback((fractionId) => {
    setPlayerFractionId(fractionId);
    setSelectedShip(null);
    setSelectedFraction(null);
    ordersManager.clearOrders();
  }, [ordersManager]);

  if (loading) {
    return (
      <div className="battle-simulator loading">
        <div className="loading-spinner"></div>
        <p>Ładowanie bitwy...</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className="battle-simulator error">
        <h2>Błąd</h2>
        <p>{error}</p>
        <button onClick={() => navigate('/battles')}>Powrót do listy bitew</button>
      </div>
    );
  }

  if (!battleState) {
    return (
      <div className="battle-simulator error">
        <h2>Bitwa nie została znaleziona</h2>
        <button onClick={() => navigate('/battles')}>Powrót do listy bitew</button>
      </div>
    );
  }

  const currentOrder = selectedShip 
    ? ordersManager.getOrderForShip(selectedShip.shipId) 
    : null;

  return (
    <div className="battle-simulator">
      <div className="battle-header">
        <div className="battle-title">
          <button 
            className="back-btn"
            onClick={() => navigate('/battles')}
          >
            ← Powrót
          </button>
          <h1>{battleState.name}</h1>
          <div className="battle-size">
            {battleState.width} × {battleState.height}
          </div>
        </div>

        {!playerFractionId && battleState.fractions.length > 0 && (
          <div className="fraction-selector">
            <p>Wybierz swoją frakcję:</p>
            <div className="fraction-buttons">
              {battleState.fractions.map((fraction, index) => (
                <button
                  key={fraction.fractionId}
                  className="fraction-select-btn"
                  style={{
                    backgroundColor: ['#4CAF50', '#F44336', '#2196F3', '#FF9800', '#9C27B0'][index % 5]
                  }}
                  onClick={() => handleSelectPlayerFraction(fraction.fractionId)}
                >
                  {fraction.fractionName}
                </button>
              ))}
            </div>
          </div>
        )}
      </div>

      <div className="battle-content">
        <div className="battle-main">
          <BattleCanvas
            battleState={battleState}
            selectedShip={selectedShip}
            onShipClick={handleShipClick}
            onCellClick={handleCellClick}
            orders={ordersManager.orders}
          />
        </div>

        <div className="battle-sidebar">
          <ShipControlPanel
            selectedShip={selectedShip}
            selectedFraction={selectedFraction}
            currentOrder={currentOrder}
            onClearOrder={handleClearOrder}
            battleStatus={battleState.status}
          />

          <TurnController
            battleId={battleId}
            battleState={battleState}
            orders={ordersManager.orders}
            onSubmitOrders={handleSubmitOrders}
            onTurnExecuted={handleTurnExecuted}
            onRefreshBattle={refresh}
          />
        </div>
      </div>

      {ordersManager.error && (
        <div className="orders-error">
          <strong>Błąd rozkazów:</strong> {ordersManager.error}
        </div>
      )}
    </div>
  );
};
