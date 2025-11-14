import { useState, useEffect, useCallback } from 'react';
import { getBattle } from '../../../services/api';

/**
 * Hook do zarządzania stanem bitwy
 * @param {string} battleId - ID bitwy
 * @param {boolean} autoRefresh - Czy automatycznie odświeżać stan
 * @param {number} refreshInterval - Interwał odświeżania w ms
 */
export const useBattleState = (battleId, autoRefresh = false, refreshInterval = 2000) => {
  const [battleState, setBattleState] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const fetchBattleState = useCallback(async () => {
    if (!battleId) return;

    try {
      setLoading(true);
      const data = await getBattle(battleId);
      setBattleState(data);
      setError(null);
    } catch (err) {
      setError(err.message || 'Failed to fetch battle state');
      console.error('Error fetching battle state:', err);
    } finally {
      setLoading(false);
    }
  }, [battleId]);

  // Initial load
  useEffect(() => {
    fetchBattleState();
  }, [fetchBattleState]);

  // Auto-refresh
  useEffect(() => {
    if (!autoRefresh || !battleId) return;

    const interval = setInterval(fetchBattleState, refreshInterval);
    return () => clearInterval(interval);
  }, [autoRefresh, battleId, refreshInterval, fetchBattleState]);

  return {
    battleState,
    loading,
    error,
    refresh: fetchBattleState,
  };
};
