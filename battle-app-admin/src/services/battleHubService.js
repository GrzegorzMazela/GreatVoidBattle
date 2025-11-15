import * as signalR from '@microsoft/signalr';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';

/**
 * Serwis do zarządzania połączeniem SignalR z BattleHub
 */
class BattleHubService {
  constructor() {
    this.connection = null;
    this.isConnected = false;
    this.currentBattleId = null;
    this.eventHandlers = {};
  }

  /**
   * Inicjalizuje połączenie SignalR
   */
  async connect() {
    if (this.connection && this.isConnected) {
      console.log('Already connected to BattleHub');
      return;
    }

    try {
      this.connection = new signalR.HubConnectionBuilder()
        .withUrl(`${API_BASE_URL}/hubs/battle`, {
          skipNegotiation: false,
          transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling
        })
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Information)
        .build();

      // Obsługa reconnect
      this.connection.onreconnecting((error) => {
        console.warn('SignalR reconnecting...', error);
        this.isConnected = false;
      });

      this.connection.onreconnected((connectionId) => {
        console.log('SignalR reconnected:', connectionId);
        this.isConnected = true;
        // Ponownie dołącz do grupy bitwy jeśli była aktywna
        if (this.currentBattleId) {
          this.joinBattle(this.currentBattleId);
        }
      });

      this.connection.onclose((error) => {
        console.error('SignalR connection closed:', error);
        this.isConnected = false;
      });

      await this.connection.start();
      this.isConnected = true;
      console.log('Connected to BattleHub');
    } catch (error) {
      console.error('Error connecting to BattleHub:', error);
      throw error;
    }
  }

  /**
   * Rozłącza połączenie
   */
  async disconnect() {
    if (this.connection) {
      if (this.currentBattleId) {
        await this.leaveBattle(this.currentBattleId);
      }
      await this.connection.stop();
      this.isConnected = false;
      this.connection = null;
      console.log('Disconnected from BattleHub');
    }
  }

  /**
   * Dołącza do grupy bitwy
   */
  async joinBattle(battleId) {
    if (!this.connection || !this.isConnected) {
      await this.connect();
    }

    try {
      await this.connection.invoke('JoinBattle', battleId);
      this.currentBattleId = battleId;
      console.log(`Joined battle: ${battleId}`);
    } catch (error) {
      console.error('Error joining battle:', error);
      throw error;
    }
  }

  /**
   * Opuszcza grupę bitwy
   */
  async leaveBattle(battleId) {
    if (!this.connection || !this.isConnected) {
      return;
    }

    try {
      await this.connection.invoke('LeaveBattle', battleId);
      if (this.currentBattleId === battleId) {
        this.currentBattleId = null;
      }
      console.log(`Left battle: ${battleId}`);
    } catch (error) {
      console.error('Error leaving battle:', error);
    }
  }

  /**
   * Nasłuchuje na event "PlayerFinishedTurn"
   */
  onPlayerFinishedTurn(callback) {
    if (!this.connection) return;
    
    // Usuń stary handler jeśli istnieje
    if (this.eventHandlers['PlayerFinishedTurn']) {
      this.connection.off('PlayerFinishedTurn', this.eventHandlers['PlayerFinishedTurn']);
    }
    
    this.connection.on('PlayerFinishedTurn', callback);
    this.eventHandlers['PlayerFinishedTurn'] = callback;
  }

  /**
   * Nasłuchuje na event "NewTurnStarted"
   */
  onNewTurnStarted(callback) {
    if (!this.connection) return;
    
    // Usuń stary handler jeśli istnieje
    if (this.eventHandlers['NewTurnStarted']) {
      this.connection.off('NewTurnStarted', this.eventHandlers['NewTurnStarted']);
    }
    
    this.connection.on('NewTurnStarted', callback);
    this.eventHandlers['NewTurnStarted'] = callback;
  }

  /**
   * Nasłuchuje na event "WaitingPlayersUpdated"
   */
  onWaitingPlayersUpdated(callback) {
    if (!this.connection) return;
    
    // Usuń stary handler jeśli istnieje
    if (this.eventHandlers['WaitingPlayersUpdated']) {
      this.connection.off('WaitingPlayersUpdated', this.eventHandlers['WaitingPlayersUpdated']);
    }
    
    this.connection.on('WaitingPlayersUpdated', callback);
    this.eventHandlers['WaitingPlayersUpdated'] = callback;
  }

  /**
   * Usuwa wszystkie handlery eventów
   */
  removeAllHandlers() {
    if (!this.connection) return;
    
    Object.entries(this.eventHandlers).forEach(([eventName, handler]) => {
      this.connection.off(eventName, handler);
    });
    this.eventHandlers = {};
  }
}

// Singleton instance
const battleHubService = new BattleHubService();

export default battleHubService;
