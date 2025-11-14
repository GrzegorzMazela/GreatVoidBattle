import { useState, useCallback } from 'react';
import { submitOrders } from '../../../services/api';

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

  /**
   * Dodaj rozkaz ruchu statku
   */
  const addMoveOrder = useCallback((shipId, targetX, targetY) => {
    setOrders(prev => {
      // Usuń poprzedni rozkaz dla tego statku
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
      // Usuń poprzedni rozkaz dla tego statku
      const filtered = prev.filter(o => o.shipId !== shipId);
      return [...filtered, {
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
      // Usuń poprzedni rozkaz dla tego statku
      const filtered = prev.filter(o => o.shipId !== shipId);
      return [...filtered, {
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
   * Wyczyść wszystkie rozkazy
   */
  const clearOrders = useCallback(() => {
    setOrders([]);
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

      const payload = {
        turnNumber,
        orders: orders,
      };

      await submitOrders(battleId, fractionId, payload);
      
      // Wyczyść rozkazy po udanym wysłaniu
      setOrders([]);
      
      return { success: true };
    } catch (err) {
      const errorMsg = err.response?.data?.message || err.message || 'Failed to submit orders';
      setError(errorMsg);
      return { success: false, error: errorMsg };
    } finally {
      setSubmitting(false);
    }
  }, [battleId, fractionId, turnNumber, orders]);

  /**
   * Pobierz rozkaz dla konkretnego statku
   */
  const getOrderForShip = useCallback((shipId) => {
    return orders.find(o => o.shipId === shipId);
  }, [orders]);

  return {
    orders,
    addMoveOrder,
    addLaserOrder,
    addMissileOrder,
    removeOrder,
    clearOrders,
    submit,
    getOrderForShip,
    submitting,
    error,
    hasOrders: orders.length > 0,
  };
};
