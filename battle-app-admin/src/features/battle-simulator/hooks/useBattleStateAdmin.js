import { useState, useEffect, useCallback, useRef } from 'react';
import { getBattleAdmin } from '../../../services/api';

/**
 * Hook do pobierania stanu bitwy w widoku admina (z pełnymi danymi: ścieżki ruchów, rakiety, lasery)
 * Odświeżanie w tle (auto-refresh i przycisk Odśwież) nie ustawia loading, żeby nie resetować pozycji/zoomu mapy.
 * @param {string} battleId - ID bitwy
 * @param {boolean} autoRefresh - Czy automatycznie odświeżać stan
 * @param {number} refreshInterval - Interwał odświeżania w ms
 */
export const useBattleStateAdmin = (battleId, autoRefresh = true, refreshInterval = 3000) => {
  const [battleState, setBattleState] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const hasLoadedOnce = useRef(false);

  const fetchBattleState = useCallback(async () => {
    if (!battleId) return;

    const isFirstLoad = !hasLoadedOnce.current;
    if (isFirstLoad) {
      setLoading(true);
    }

    try {
      const data = await getBattleAdmin(battleId);
      hasLoadedOnce.current = true;
      setBattleState(data);
      setError(null);
    } catch (err) {
      setError(err.message || 'Failed to fetch battle state');
      console.error('Error fetching battle state (admin):', err);
    } finally {
      if (isFirstLoad) {
        setLoading(false);
      }
    }
  }, [battleId]);

  useEffect(() => {
    fetchBattleState();
  }, [fetchBattleState]);

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
