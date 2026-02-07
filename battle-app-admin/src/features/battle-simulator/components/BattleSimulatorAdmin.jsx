import { useState, useCallback, useRef } from 'react';
import { useParams, Link } from 'react-router-dom';
import { useBattleStateAdmin } from '../hooks/useBattleStateAdmin';
import { BattleCanvas } from './BattleCanvas';
import { AdminPlansPanel } from './AdminPlansPanel';
import { useNotification } from '../../../contexts/NotificationContext';
import './BattleSimulator.css';

/**
 * Widok symulatora dla administratora – wszystkie plany ruchów, rakiety i lasery
 * Link z detali bitwy (np. /pustka-admin-panel/:battleId/simulator-admin)
 */
export function BattleSimulatorAdmin() {
  const { battleId } = useParams();
  const { battleState, loading, error, refresh } = useBattleStateAdmin(battleId, true, 4000);
  const [selectedShip, setSelectedShip] = useState(null);
  const [selectedFraction, setSelectedFraction] = useState(null);
  const battleCanvasRef = useRef(null);
  const { showSuccess, showError } = useNotification();

  const handleShipClick = useCallback((ship, fraction) => {
    setSelectedShip(ship);
    setSelectedFraction(fraction);
  }, []);

  const handleCellClick = useCallback(() => {
    // W widoku admina nie wydajemy rozkazów
  }, []);

  const copyAdminSimulatorLink = useCallback(() => {
    const url = `${window.location.origin}/pustka-admin-panel/${battleId}/simulator-admin`;
    if (navigator.clipboard?.writeText) {
      navigator.clipboard.writeText(url).then(
        () => showSuccess('Link skopiowany do schowka'),
        () => showError('Nie udało się skopiować')
      );
    } else {
      showError('Skopiuj link ręcznie: ' + url);
    }
  }, [battleId, showSuccess, showError]);

  if (loading) {
    return (
      <div className="battle-simulator loading">
        <div className="loading-spinner" />
        <p>Ładowanie bitwy (widok admin)…</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className="battle-simulator error">
        <h2>Błąd</h2>
        <p>{error}</p>
        <Link to={`/pustka-admin-panel/${battleId}`}>Powrót do detali bitwy</Link>
      </div>
    );
  }

  if (!battleState) {
    return (
      <div className="battle-simulator error">
        <h2>Bitwa nie została znaleziona</h2>
        <Link to="/pustka-admin-panel">Powrót do listy</Link>
      </div>
    );
  }

  return (
    <div className="battle-simulator battle-simulator-admin">
      <div className="battle-header">
        <div className="battle-title">
          <h1>{battleState.name} — widok admin</h1>
          <div className="battle-size">
            {battleState.width} × {battleState.height}
          </div>
          <div className="battle-admin-meta">
            <span>Tura: <strong>{battleState.turnNumber}</strong></span>
            <span>Status: <strong>{battleState.status}</strong></span>
          </div>
          <div className="battle-admin-actions">
            <Link
              to={`/pustka-admin-panel/${battleId}`}
              className="admin-simulator-back-link"
            >
              ← Detal bitwy
            </Link>
            <button
              type="button"
              className="admin-simulator-copy-link"
              onClick={copyAdminSimulatorLink}
            >
              📋 Kopiuj link do tego widoku
            </button>
            <button type="button" className="admin-simulator-refresh" onClick={refresh}>
              🔄 Odśwież
            </button>
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
            orders={[]}
            weaponMode={null}
            playerFractionId={null}
          />
        </div>

        <aside className="battle-sidebar-admin">
          <AdminPlansPanel battleState={battleState} />
        </aside>
      </div>
    </div>
  );
}
