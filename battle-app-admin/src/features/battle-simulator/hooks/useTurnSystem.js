import { useState, useEffect, useCallback, useRef } from 'react';
import battleHubService from '../../../services/battleHubService';

/**
 * Hook do zarządzania systemem turowym z SignalR
 */
export const useTurnSystem = (battleId, fractionId, onNewTurn, battleState) => {
  const [isWaitingForPlayers, setIsWaitingForPlayers] = useState(false);
  const [waitingPlayers, setWaitingPlayers] = useState([]);
  const [turnFinished, setTurnFinished] = useState(false);
  
  // Użyj ref aby callback był stabilny
  const onNewTurnRef = useRef(onNewTurn);
  useEffect(() => {
    onNewTurnRef.current = onNewTurn;
  }, [onNewTurn]);

  // Sprawdź przy inicjalizacji i zmianie stanu bitwy czy gracz już zakończył turę
  useEffect(() => {
    if (!battleState || !fractionId) return;

    const playerFraction = battleState.fractions?.find(f => f.fractionId === fractionId);
    
    if (playerFraction && playerFraction.turnFinished) {
      // Gracz już zakończył turę - ustaw stan oczekiwania
      setTurnFinished(true);
      
      // Zbierz listę graczy którzy jeszcze nie zakończyli
      const waiting = battleState.fractions
        .filter(f => !f.isDefeated && !f.turnFinished)
        .map(f => ({
          fractionId: f.fractionId,
          fractionName: f.fractionName,
          playerName: f.playerName
        }));
      
      setWaitingPlayers(waiting);
      setIsWaitingForPlayers(waiting.length > 0);
    } else if (playerFraction && !playerFraction.turnFinished) {
      // Gracz jeszcze nie zakończył tury - wyczyść stan oczekiwania
      setTurnFinished(false);
      setIsWaitingForPlayers(false);
      setWaitingPlayers([]);
    }
  }, [battleState, fractionId]);

  // Połącz z hubem przy montowaniu
  useEffect(() => {
    const connectToHub = async () => {
      try {
        // Najpierw usuń stare handlery jeśli istnieją
        battleHubService.removeAllHandlers();
        
        await battleHubService.connect();
        await battleHubService.joinBattle(battleId);

        // Nasłuchuj na eventy - te callbacki są stabilne
        battleHubService.onPlayerFinishedTurn((data) => {
          console.log('Player finished turn:', data);
          // Możesz tutaj pokazać notyfikację że gracz zakończył turę
        });

        battleHubService.onNewTurnStarted((data) => {
          console.log('New turn started:', data);
          setIsWaitingForPlayers(false);
          setWaitingPlayers([]);
          setTurnFinished(false);
          
          // Wywołaj callback z nowym numerem tury
          if (onNewTurnRef.current) {
            onNewTurnRef.current(data.TurnNumber);
          }
        });

        battleHubService.onWaitingPlayersUpdated((players) => {
          console.log('Waiting players updated:', players);
          setWaitingPlayers(players || []);
          // Jeśli lista się zmniejszyła, znaczy że ktoś zakończył turę
          if (players && players.length > 0) {
            setIsWaitingForPlayers(true);
          }
        });

      } catch (error) {
        console.error('Error connecting to battle hub:', error);
      }
    };

    connectToHub();

    // Cleanup
    return () => {
      battleHubService.leaveBattle(battleId);
      battleHubService.removeAllHandlers();
    };
  }, [battleId]); // Tylko battleId jako dependency

  // Funkcja do zakończenia tury gracza
  const finishTurn = useCallback((waitingPlayersList) => {
    setTurnFinished(true);
    setIsWaitingForPlayers(waitingPlayersList.length > 0);
    setWaitingPlayers(waitingPlayersList);
  }, []);

  return {
    isWaitingForPlayers,
    waitingPlayers,
    turnFinished,
    finishTurn,
  };
};
