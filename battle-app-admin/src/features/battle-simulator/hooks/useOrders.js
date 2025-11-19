import { useState, useCallback } from 'react';
import { submitOrders } from '../../../services/api';
import { getPlayerSession } from '../../../services/authApi';

/**
 * Hook do zarządzania kolejką rozkazów
 * @param {string} battleId - ID bitwy
 * @param {string} fractionId - ID frakcji
 * @param {number} turnNumber - Numer aktualnej tury
 */
export const useOrders = (battleId, fractionId, turnNumber) => {
  const [orders, setOrders] = useState([]);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState(null);
  
  // Licznik zatwierdzonych rozkazów w bieżącej turze (resetowany tylko po wykonaniu tury)
  const [submittedOrdersCount, setSubmittedOrdersCount] = useState({});

  /**
   * Dodaj rozkaz ruchu statku
   */
  const addMoveOrder = useCallback((shipId, targetX, targetY) => {
    setOrders(prev => {
      // Usuń wszystkie poprzednie rozkazy dla tego statku (ruchy i ataki)
      // Nowy rozkaz ruchu anuluje wszystkie wcześniejsze akcje
      const filtered = prev.filter(o => o.shipId !== shipId);
      return [...filtered, {
        shipId,
        type: 'move',
        targetX,
        targetY,
      }];
    });
  }, []);

  /**
   * Dodaj rozkaz strzału laserowego
   */
  const addLaserOrder = useCallback((shipId, targetShipId, targetFractionId) => {
    setOrders(prev => {
      // Dodaj nowy rozkaz (NIE usuwaj poprzednich - statek może strzelić wielokrotnie)
      return [...prev, {
        shipId,
        type: 'laser',
        targetShipId,
        targetFractionId,
      }];
    });
  }, []);

  /**
   * Dodaj rozkaz strzału rakietowego
   */
  const addMissileOrder = useCallback((shipId, targetShipId, targetFractionId) => {
    setOrders(prev => {
      // Dodaj nowy rozkaz (NIE usuwaj poprzednich - statek może strzelić wielokrotnie)
      return [...prev, {
        shipId,
        type: 'missile',
        targetShipId,
        targetFractionId,
      }];
    });
  }, []);

  /**
   * Usuń rozkaz dla konkretnego statku
   */
  const removeOrder = useCallback((shipId) => {
    setOrders(prev => prev.filter(o => o.shipId !== shipId));
  }, []);

  /**
   * Usuń konkretny rozkaz na podstawie indeksu
   */
  const removeOrderByIndex = useCallback((shipId, orderIndex) => {
    setOrders(prev => {
      // Filtruj rozkazy dla tego statku
      const shipOrders = prev.filter(o => o.shipId === shipId);
      
      // Jeśli indeks jest poprawny
      if (orderIndex >= 0 && orderIndex < shipOrders.length) {
        // Usuń rozkaz na danym indeksie
        const orderToRemove = shipOrders[orderIndex];
        let removed = false;
        
        return prev.filter(o => {
          if (!removed && o === orderToRemove) {
            removed = true;
            return false;
          }
          return true;
        });
      }
      
      return prev;
    });
  }, []);

  /**
   * Usuń rozkaz na podstawie globalnego indeksu w tablicy orders
   */
  const removeOrderByGlobalIndex = useCallback((globalIndex) => {
    setOrders(prev => {
      if (globalIndex >= 0 && globalIndex < prev.length) {
        return prev.filter((_, index) => index !== globalIndex);
      }
      return prev;
    });
  }, []);

  /**
   * Usuń ostatni rozkaz danego typu dla statku
   */
  const removeLastOrderOfType = useCallback((shipId, orderType) => {
    setOrders(prev => {
      // Znajdź ostatni indeks rozkazu danego typu dla tego statku
      const lastIndex = prev.map((o, i) => 
        o.shipId === shipId && o.type === orderType ? i : -1
      ).filter(i => i !== -1).pop();
      
      if (lastIndex !== undefined) {
        return prev.filter((_, i) => i !== lastIndex);
      }
      return prev;
    });
  }, []);

  /**
   * Wyczyść wszystkie rozkazy
   */
  const clearOrders = useCallback(() => {
    setOrders([]);
  }, []);

  /**
   * Resetuj liczniki zatwierdzonych rozkazów (wywołaj po wykonaniu tury)
   */
  const resetSubmittedCounts = useCallback(() => {
    setSubmittedOrdersCount({});
  }, []);

  /**
   * Wyślij rozkazy do API
   */
  const submit = useCallback(async () => {
    if (!battleId || !fractionId || orders.length === 0) {
      return { success: false, error: 'No orders to submit' };
    }

    try {
      setSubmitting(true);
      setError(null);

      // Pobierz token autoryzacyjny
      const session = getPlayerSession();
      const token = session.authToken;

      const payload = {
        turnNumber,
        orders: orders,
      };

      const battleState = await submitOrders(battleId, fractionId, payload, token);
      
      // Zapisz zatwierdzone rozkazy do licznika
      const newCounts = { ...submittedOrdersCount };
      orders.forEach(order => {
        const key = `${order.shipId}_${order.type}`;
        newCounts[key] = (newCounts[key] || 0) + 1;
      });
      setSubmittedOrdersCount(newCounts);
      
      return { success: true, battleState };
    } catch (err) {
      const errorMsg = err.response?.data?.message || err.message || 'Failed to submit orders';
      setError(errorMsg);
      return { success: false, error: errorMsg };
    } finally {
      setSubmitting(false);
    }
  }, [battleId, fractionId, turnNumber, orders, submittedOrdersCount]);

  /**
   * Pobierz rozkaz dla konkretnego statku
   */
  const getOrderForShip = useCallback((shipId) => {
    return orders.find(o => o.shipId === shipId);
  }, [orders]);

  /**
   * Pobierz łączną liczbę rozkazów danego typu dla statku (lokalne + zatwierdzone)
   */
  const getTotalOrderCount = useCallback((shipId, orderType) => {
    const localCount = orders.filter(
      order => order.shipId === shipId && order.type === orderType
    ).length;
    const submittedCount = submittedOrdersCount[`${shipId}_${orderType}`] || 0;
    return localCount + submittedCount;
  }, [orders, submittedOrdersCount]);

  return {
    orders,
    addMoveOrder,
    addLaserOrder,
    addMissileOrder,
    removeOrder,
    removeOrderByIndex,
    removeOrderByGlobalIndex,
    removeLastOrderOfType,
    clearOrders,
    resetSubmittedCounts,
    submit,
    getOrderForShip,
    getTotalOrderCount,
    submitting,
    error,
    hasOrders: orders.length > 0,
  };
};
