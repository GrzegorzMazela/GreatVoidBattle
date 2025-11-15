import { useState, useCallback, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useBattleState } from '../hooks/useBattleState';
import { useOrders } from '../hooks/useOrders';
import { setShipPosition, executeTurn } from '../../../services/api';
import { getPlayerSession } from '../../../services/authApi';
import { BattleCanvas } from './BattleCanvas';
import { ShipControlPanel } from './ShipControlPanel';
import { TurnController } from './TurnController';
import { WeaponCountDialog } from './WeaponCountDialog';
import { useModal } from '../../../hooks/useModal';
import { AlertModal } from '../../../components/modals/AlertModal';
import './BattleSimulator.css';

/**
 * Główny komponent symulatora bitwy
 * Łączy wszystkie elementy w jeden interfejs
 */
export const BattleSimulator = ({ sessionData }) => {
  const { battleId } = useParams();
  const navigate = useNavigate();
  
  const { battleState, loading, error, refresh } = useBattleState(battleId, false);
  
  // State dla wykonywania tury
  const [isExecuting, setIsExecuting] = useState(false);
  
  // State dla wybranego statku i frakcji gracza
  const [selectedShip, setSelectedShip] = useState(null);
  const [selectedFraction, setSelectedFraction] = useState(null);
  
  // Pobierz fractionId z sesji gracza (przekazane przez RequireAuth lub z localStorage)
  const playerFractionId = sessionData?.fractionId || getPlayerSession().fractionId;
  
  // State dla trybu strzelania
  const [weaponMode, setWeaponMode] = useState(null); // 'missile', 'laser', null
  
  // State dla dialogu wyboru liczby strzałów
  const [weaponDialog, setWeaponDialog] = useState(null); // { type, targetShip, targetFraction, maxCount }
  
  // Modal dla komunikatów
  const alertModal = useModal();

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

  // Oblicz ile pocisków/laserów wystrzelono w tej turze
  const getWeaponFiredCount = useCallback((shipId, weaponType) => {
    return ordersManager.getTotalOrderCount(shipId, weaponType);
  }, [ordersManager]);

  // Obsługa kliknięcia w statek
  const handleShipClick = useCallback((ship, fraction, mouseX, mouseY) => {
    // Prawy przycisk - odznacz
    if (!ship) {
      setSelectedShip(null);
      setSelectedFraction(null);
      setWeaponMode(null);
      return;
    }
    
    setSelectedShip(ship);
    setSelectedFraction(fraction);
    
    // Reset trybu broni przy wyborze nowego statku
    if (fraction.fractionId === playerFractionId) {
      setWeaponMode(null);
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
      alertModal.openModal({
        title: 'Błąd',
        message: `Błąd przy ustawianiu pozycji: ${error.response?.data?.message || error.message}`,
        variant: 'error'
      });
    }
  }, [battleId, refresh, alertModal]);

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
        // Kliknięto w własny statek - nie rób nic, to obsłuży handleShipClick
        return;
      } else {
        // Kliknięto we wrogi statek - tylko jeśli mamy wybraną broń
        if (weaponMode === 'missile') {
          // Tryb rakiet - sprawdź zasięg
          const distance = Math.sqrt(
            Math.pow(clickedShip.x - selectedShip.x, 2) + 
            Math.pow(clickedShip.y - selectedShip.y, 2)
          );
          
          const MISSILE_MAX_RANGE = 55;
          
          if (distance <= MISSILE_MAX_RANGE) {
            // Sprawdź ile ma dostępnych rakiet
            const firedCount = getWeaponFiredCount(selectedShip.shipId, 'missile');
            const totalMissiles = selectedShip.numberOfMissiles || 0;
            const available = totalMissiles - firedCount;
            
            if (available > 0) {
              // Otwórz dialog wyboru liczby rakiet
              setWeaponDialog({
                type: 'missile',
                targetShip: clickedShip,
                targetFraction: clickedFraction,
                maxCount: available
              });
            } else {
              alertModal.openModal({
                title: 'Brak amunicji',
                message: 'Brak dostępnych rakiet!',
                variant: 'warning'
              });
            }
          } else {
            alertModal.openModal({
              title: 'Cel poza zasięgiem',
              message: `Cel poza zasięgiem! Odległość: ${distance.toFixed(1)}, Max: ${MISSILE_MAX_RANGE}`,
              variant: 'warning'
            });
          }
        } else if (weaponMode === 'laser') {
          // Tryb lasera - sprawdź zasięg
          const distance = Math.sqrt(
            Math.pow(clickedShip.x - selectedShip.x, 2) + 
            Math.pow(clickedShip.y - selectedShip.y, 2)
          );
          
          const LASER_MAX_RANGE = 20;
          
          if (distance <= LASER_MAX_RANGE) {
            const firedCount = getWeaponFiredCount(selectedShip.shipId, 'laser');
            const totalLasers = selectedShip.numberOfLasers || 0;
            const available = totalLasers - firedCount;
            
            if (available > 0) {
              // Otwórz dialog wyboru liczby laserów
              setWeaponDialog({
                type: 'laser',
                targetShip: clickedShip,
                targetFraction: clickedFraction,
                maxCount: available
              });
            } else {
              alertModal.openModal({
                title: 'Brak amunicji',
                message: 'Brak dostępnych laserów!',
                variant: 'warning'
              });
            }
          } else {
            alertModal.openModal({
              title: 'Cel poza zasięgiem',
              message: `Cel poza zasięgiem! Odległość: ${distance.toFixed(1)}, Max: ${LASER_MAX_RANGE}`,
              variant: 'warning'
            });
          }
        }
        // Jeśli nie ma wybranej broni, nie rób nic (nie atakuj, nie zmieniaj focusu)
      }
    } else {
      // Kliknięto w puste pole - ruch (tylko jeśli nie ma wybranej broni)
      if (!weaponMode) {
        ordersManager.addMoveOrder(selectedShip.shipId, x, y);
      }
    }
  }, [selectedShip, playerFractionId, battleState, ordersManager, weaponMode, getWeaponFiredCount]);

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

    // TRYB PREPARATION - przemieść statek natychmiast
    if (battleState.status === 'Preparation') {
      if (!clickedShip) {
        handleMoveShipInPreparation(selectedShip, shipFraction, x, y);
      }
      return;
    }

    // TRYB INPROGRESS - zaplanuj rozkazy
    if (battleState.status === 'InProgress') {
      if (clickedShip && clickedFraction.fractionId !== playerFractionId && weaponMode) {
        // Kliknięto we wrogi statek z wybraną bronią - atakuj (NIE zmieniaj focusu)
        handleOrderInProgress(x, y);
        return false; // Zatrzymaj propagację do handleShipClick
      }
      
      // Inaczej - normalna logika (ruch lub zmiana statku)
      handleOrderInProgress(x, y);
      return;
    }
  }, [selectedShip, playerFractionId, battleState, findFractionByShip, handleMoveShipInPreparation, handleOrderInProgress, weaponMode]);

  // Obsługa anulowania rozkazu
  const handleClearOrder = useCallback(() => {
    if (selectedShip) {
      ordersManager.removeOrder(selectedShip.shipId);
    }
  }, [selectedShip, ordersManager]);

  // Obsługa potwierdzenia dialogu broni
  const handleWeaponDialogConfirm = useCallback((count) => {
    if (!weaponDialog || !selectedShip) return;

    const { type, targetShip, targetFraction } = weaponDialog;

    // Dodaj rozkazy (jeden rozkaz na każdy strzał)
    for (let i = 0; i < count; i++) {
      if (type === 'missile') {
        ordersManager.addMissileOrder(
          selectedShip.shipId,
          targetShip.shipId,
          targetFraction.fractionId
        );
      } else if (type === 'laser') {
        ordersManager.addLaserOrder(
          selectedShip.shipId,
          targetShip.shipId,
          targetFraction.fractionId
        );
      }
    }

    setWeaponDialog(null);
  }, [weaponDialog, selectedShip, ordersManager]);

  // Obsługa anulowania dialogu broni
  const handleWeaponDialogCancel = useCallback(() => {
    setWeaponDialog(null);
  }, []);

  // Obsługa zatwierdzenia rozkazów
  const handleSubmitOrders = useCallback(async () => {
    const result = await ordersManager.submit();
    if (result.success) {
      // Wyczyść lokalne rozkazy
      ordersManager.clearOrders();
      // Jeśli API zwróciło zaktualizowany stan, użyj go zamiast odświeżania
      if (result.battleState) {
        // Stan został już zaktualizowany przez submit, ale wywołaj refresh dla pewności
        await refresh();
      } else {
        await refresh();
      }
      alertModal.openModal({
        title: 'Sukces',
        message: 'Rozkazy zostały zatwierdzone!',
        variant: 'success'
      });
    } else {
      alertModal.openModal({
        title: 'Błąd',
        message: `Błąd: ${result.error}`,
        variant: 'error'
      });
    }
  }, [ordersManager, refresh, alertModal]);

  // Obsługa wykonania tury
  const handleTurnExecuted = useCallback(async (updatedBattle) => {
    console.log('Turn executed, new state:', updatedBattle);
    // Wyczyść rozkazy i wybór po wykonaniu tury
    ordersManager.clearOrders();
    ordersManager.resetSubmittedCounts(); // Reset liczników zatwierdzonych rozkazów
    setSelectedShip(null);
    setSelectedFraction(null);
    setWeaponMode(null);
    setWeaponDialog(null);
  }, [ordersManager]);

  // Obsługa bezpośredniego wykonania tury
  const handleExecuteTurn = useCallback(async () => {
    try {
      setIsExecuting(true);

      // Wykonaj turę
      const updatedBattle = await executeTurn(battleId);
      
      // Notify parent component
      await handleTurnExecuted(updatedBattle);
      
      // Refresh battle state
      await refresh();
      
    } catch (error) {
      const errorMsg = error.response?.data?.message || error.message || 'Failed to execute turn';
      alertModal.openModal({
        title: 'Błąd wykonania tury',
        message: errorMsg,
        variant: 'error'
      });
      console.error('Error executing turn:', error);
    } finally {
      setIsExecuting(false);
    }
  }, [battleId, handleTurnExecuted, refresh]);

  // Pokaż informację o frakcji gracza
  const playerFraction = battleState?.fractions.find(f => f.fractionId === playerFractionId);

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
      </div>
    );
  }

  if (!battleState) {
    return (
      <div className="battle-simulator error">
        <h2>Bitwa nie została znaleziona</h2>
      </div>
    );
  }

  const currentOrder = selectedShip 
    ? ordersManager.getOrderForShip(selectedShip.shipId) 
    : null;

  // Oblicz statystyki rozkazów dla wybranego statku
  const shipOrderStats = selectedShip ? {
    moveOrders: ordersManager.orders.filter(o => o.shipId === selectedShip.shipId && o.type === 'move').length,
    laserOrders: ordersManager.orders.filter(o => o.shipId === selectedShip.shipId && o.type === 'laser').length,
    missileOrders: ordersManager.orders.filter(o => o.shipId === selectedShip.shipId && o.type === 'missile').length,
  } : null;

  return (
    <div className="battle-simulator">
      <div className="battle-header">
        <div className="battle-title">
          <h1>{battleState.name}</h1>
          <div className="battle-size">
            {battleState.width} × {battleState.height}
          </div>

          {/* Informacja o graczu i jego frakcji */}
          {playerFraction && (
            <div className="player-info">
              <span className="player-label">Grasz jako:</span>
              <span className="player-name">{playerFraction.playerName || 'Gracz'}</span>
              <span className="player-fraction" style={{ color: playerFraction.fractionColor }}>
                ({playerFraction.fractionName})
              </span>
            </div>
          )}

          {/* Status frakcji w jednej linii */}
          <div className="fractions-status-inline">
            {battleState.fractions.map((fraction, index) => (
              <div key={fraction.fractionId} className="fraction-status-inline">
                <div 
                  className="fraction-color" 
                  style={{ 
                    backgroundColor: ['#4CAF50', '#F44336', '#2196F3', '#FF9800', '#9C27B0'][index % 5] 
                  }}
                />
                <span className="fraction-name">{fraction.fractionName}</span>
                <span className="ships-count">
                  {fraction.ships.length}
                </span>
                {fraction.isDefeated && (
                  <span className="defeated-badge">✗</span>
                )}
              </div>
            ))}
          </div>

          {/* Licznik tury i akcje */}
          <div className="turn-controls-inline">
            <div className="turn-number-inline">
              <span className="label">Tura:</span>
              <span className="value">{battleState.turnNumber}</span>
            </div>
            
            {battleState.status === 'InProgress' && (
              <>
                <button 
                  className="execute-turn-btn-inline"
                  onClick={handleExecuteTurn}
                  disabled={isExecuting}
                >
                  {isExecuting ? '⏳' : '▶'} Wykonaj turę
                </button>
                
                <button 
                  className="refresh-btn-inline"
                  onClick={refresh}
                  disabled={isExecuting}
                >
                  🔄
                </button>
              </>
            )}
          </div>
        </div>
      </div>

      <div className="battle-content">
        <div className="battle-main">
          <BattleCanvas
            battleState={battleState}
            selectedShip={selectedShip}
            onShipClick={handleShipClick}
            onCellClick={handleCellClick}
            orders={ordersManager.orders}
            weaponMode={weaponMode}
          />
        </div>

        <div className="battle-sidebar">
          <ShipControlPanel
            selectedShip={selectedShip}
            selectedFraction={selectedFraction}
            currentOrder={currentOrder}
            orderStats={shipOrderStats}
            onClearOrder={handleClearOrder}
            battleStatus={battleState.status}
            weaponMode={weaponMode}
            onWeaponModeChange={setWeaponMode}
            missileFiredCount={selectedShip ? getWeaponFiredCount(selectedShip.shipId, 'missile') : 0}
            laserFiredCount={selectedShip ? getWeaponFiredCount(selectedShip.shipId, 'laser') : 0}
            isPlayerShip={selectedFraction?.fractionId === playerFractionId}
            allOrders={ordersManager.orders}
            onSubmitOrders={handleSubmitOrders}
          />
        </div>
      </div>

      {weaponDialog && (
        <WeaponCountDialog
          weaponType={weaponDialog.type}
          maxCount={weaponDialog.maxCount}
          targetShipName={weaponDialog.targetShip.name}
          onConfirm={handleWeaponDialogConfirm}
          onCancel={handleWeaponDialogCancel}
        />
      )}

      {ordersManager.error && (
        <div className="orders-error">
          <strong>Błąd rozkazów:</strong> {ordersManager.error}
        </div>
      )}

      <AlertModal
        isOpen={alertModal.isOpen}
        onClose={alertModal.closeModal}
        title={alertModal.modalData.title}
        message={alertModal.modalData.message}
        variant={alertModal.modalData.variant}
      />
    </div>
  );
};
