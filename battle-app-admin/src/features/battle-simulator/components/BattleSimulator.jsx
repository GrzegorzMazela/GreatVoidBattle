import { useState, useCallback, useEffect, useRef } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useBattleState } from '../hooks/useBattleState';
import { useOrders } from '../hooks/useOrders';
import { setShipPosition, endPlayerTurn, getTurnLogs } from '../../../services/api';
import { getPlayerSession } from '../../../services/authApi';
import { BattleCanvas } from './BattleCanvas';
import { ShipBottomPanel } from './ShipBottomPanel';
import { OrdersPanel } from './OrdersPanel';
import { TurnController } from './TurnController';
import { WeaponCountDialog } from './WeaponCountDialog';
import { TurnWaitingModal } from './TurnWaitingModal';
import { TurnLogsModal } from './TurnLogsModal';
import { useModal } from '../../../hooks/useModal';
import { AlertModal } from '../../../components/modals/AlertModal';
import { useTurnSystem } from '../hooks/useTurnSystem';
import './BattleSimulator.css';

/**
 * Główny komponent symulatora bitwy
 * Łączy wszystkie elementy w jeden interfejs
 */
export const BattleSimulator = ({ sessionData }) => {
  const { battleId } = useParams();
  const navigate = useNavigate();
  
  const { battleState, loading, error, refresh } = useBattleState(battleId, false);
  
  // Ref do BattleCanvas dla centrowania widoku
  const battleCanvasRef = useRef(null);
  
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

  // State dla logów tury
  const [turnLogs, setTurnLogs] = useState(null);
  const [showTurnLogs, setShowTurnLogs] = useState(false);
  const [turnLogsNumber, setTurnLogsNumber] = useState(0);
  const [maxTurnNumber, setMaxTurnNumber] = useState(1);

  // Hook do zarządzania rozkazami
  const ordersManager = useOrders(
    battleId,
    playerFractionId,
    battleState?.turnNumber || 0
  );

  // Callback dla nowej tury - używa ref aby mieć dostęp do aktualnych wartości
  const handleNewTurn = useCallback(async (newTurnNumber) => {
    console.log('New turn started:', newTurnNumber);
    setMaxTurnNumber(prev => Math.max(prev, newTurnNumber));
    refresh(); // Odśwież stan bitwy
    
    // Wyczyść rozkazy i stan po nowej turze
    ordersManager.clearOrders();
    ordersManager.resetSubmittedCounts();
    setSelectedShip(null);
    setSelectedFraction(null);
    setWeaponMode(null);
    setWeaponDialog(null);
    
    // Pobierz logi z poprzedniej tury
    if (newTurnNumber > 1) {
      try {
        const previousTurn = newTurnNumber - 1;
        const session = getPlayerSession();
        const logsData = await getTurnLogs(battleId, playerFractionId, previousTurn, session.authToken);
        
        // Zawsze pokaż modal z logami (nawet jeśli pusta lista)
        setTurnLogs(logsData?.logs || []);
        setTurnLogsNumber(previousTurn);
        setShowTurnLogs(true);
      } catch (error) {
        console.error('Error fetching turn logs:', error);
        // W przypadku błędu pokaż komunikat
        alertModal.openModal({
          title: 'Nowa tura!',
          message: `Rozpoczyna się tura ${newTurnNumber}! (Nie udało się pobrać logów)`,
          variant: 'warning'
        });
      }
    }
  }, [refresh, ordersManager, alertModal, battleId, playerFractionId]);

  // System turowy z SignalR
  const turnSystem = useTurnSystem(battleId, playerFractionId, handleNewTurn, battleState);

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
    
    // Wycentruj widok na wybranym statku
    if (battleCanvasRef.current && ship) {
      battleCanvasRef.current.centerOnShip(ship);
    }
    
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
          // Backend używa Manhattan distance dla rakiet
          const distance = Math.abs(Math.floor(clickedShip.x) - Math.floor(selectedShip.x)) + 
                          Math.abs(Math.floor(clickedShip.y) - Math.floor(selectedShip.y));
          
          const MISSILE_MAX_RANGE = selectedShip.missileMaxRange || 55;
          
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
          
          const LASER_MAX_RANGE = selectedShip.laserMaxRange || 15;
          
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

  // Obsługa usunięcia konkretnego rozkazu z panelu
  const handleRemoveOrder = useCallback((globalIndex) => {
    ordersManager.removeOrderByGlobalIndex(globalIndex);
  }, [ordersManager]);

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

  // Obsługa zakończenia tury przez gracza
  const handleEndTurn = useCallback(async () => {
    try {
      setIsExecuting(true);

      // KROK 1: Najpierw wyślij wszystkie rozkazy, jeśli są jakieś
      if (ordersManager.hasOrders) {
        const submitResult = await ordersManager.submit();
        
        if (!submitResult.success) {
          // Jeśli wysłanie rozkazów się nie powiodło, zatrzymaj proces
          alertModal.openModal({
            title: 'Błąd wysyłania rozkazów',
            message: `Nie udało się wysłać rozkazów: ${submitResult.error}`,
            variant: 'error'
          });
          setIsExecuting(false);
          return;
        }
        
        // Wyczyść lokalne rozkazy po pomyślnym wysłaniu
        ordersManager.clearOrders();
      }

      // KROK 2: Pobierz token autoryzacyjny
      const session = getPlayerSession();
      const token = session.authToken;

      // KROK 3: Zakończ turę gracza
      const result = await endPlayerTurn(battleId, playerFractionId, token);
      
      console.log('End turn result:', result);

      // Jeśli wszyscy gracze są gotowi, tura została wykonana automatycznie
      if (result.allPlayersReady) {
        // Nowa tura rozpoczęta - odśwież stan (komunikat pokaże się z eventu SignalR)
        await refresh();
        // SignalR event NewTurnStarted obsłuży czyszczenie rozkazów i pokazanie komunikatu
      } else {
        // Czekamy na innych graczy
        turnSystem.finishTurn(result.waitingForPlayers || []);
      }
      
    } catch (error) {
      const errorMsg = error.response?.data?.message || error.message || 'Failed to end turn';
      alertModal.openModal({
        title: 'Błąd zakończenia tury',
        message: errorMsg,
        variant: 'error'
      });
      console.error('Error ending turn:', error);
    } finally {
      setIsExecuting(false);
    }
  }, [battleId, playerFractionId, refresh, ordersManager, alertModal, turnSystem]);

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
            {battleState.fractions.map((fraction) => (
              <div key={fraction.fractionId} className="fraction-status-inline">
                <div 
                  className="fraction-color" 
                  style={{ 
                    backgroundColor: fraction.fractionColor || '#4CAF50'
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
            
            {battleState.status === 'InProgress' && !turnSystem.turnFinished && (
              <>
                <button 
                  className="execute-turn-btn-inline"
                  onClick={handleEndTurn}
                  disabled={isExecuting || turnSystem.isWaitingForPlayers}
                >
                  {isExecuting ? '⏳' : '✓'} Zakończ turę
                </button>
                
                <button 
                  className="logs-btn-inline"
                  onClick={async () => {
                    if (battleState.turnNumber > 1) {
                      try {
                        const session = getPlayerSession();
                        const logsData = await getTurnLogs(
                          battleId,
                          playerFractionId,
                          battleState.turnNumber - 1,
                          session.authToken
                        );

                        if (logsData?.logs) {
                          setTurnLogs(logsData.logs);
                          setTurnLogsNumber(battleState.turnNumber - 1);
                          setShowTurnLogs(true);
                        }
                      } catch (error) {
                        console.error('Error fetching turn logs:', error);
                      }
                    }
                  }}
                  disabled={battleState.turnNumber <= 1}
                  title="Pokaż logi z poprzedniej tury"
                >
                  📋 Logi
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

            {turnSystem.turnFinished && (
              <div className="waiting-indicator">
                <span>⏳ Oczekiwanie na graczy...</span>
              </div>
            )}
          </div>
        </div>
      </div>

      <div className="battle-content">
        <div className="battle-main">
          <BattleCanvas
            ref={battleCanvasRef}
            battleState={battleState}
            selectedShip={selectedShip}
            onShipClick={handleShipClick}
            onCellClick={handleCellClick}
            orders={ordersManager.orders}
            weaponMode={weaponMode}
            playerFractionId={playerFractionId}
          />
          
          {/* Dolny panel z jednostkami gracza */}
          <ShipBottomPanel
            playerShips={playerFraction?.ships || []}
            selectedShip={selectedShip}
            onShipSelect={(ship) => handleShipClick(ship, playerFraction)}
            playerFractionColor={playerFraction?.fractionColor}
            weaponMode={weaponMode}
            onWeaponModeChange={setWeaponMode}
          />
        </div>

        {/* Panel rozkazów po prawej stronie */}
        {battleState.status === 'InProgress' && (
          <OrdersPanel
            orders={ordersManager.orders}
            ships={battleState.fractions.flatMap(f => f.ships)}
            onRemoveOrder={handleRemoveOrder}
          />
        )}
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

      <TurnWaitingModal
        isOpen={turnSystem.turnFinished && turnSystem.isWaitingForPlayers}
        waitingPlayers={turnSystem.waitingPlayers}
      />

      <TurnLogsModal
        isOpen={showTurnLogs}
        onClose={() => setShowTurnLogs(false)}
        logs={turnLogs}
        turnNumber={turnLogsNumber}
        battleId={battleId}
        fractionId={playerFractionId}
        authToken={getPlayerSession()?.authToken}
        maxTurn={maxTurnNumber}
      />

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
