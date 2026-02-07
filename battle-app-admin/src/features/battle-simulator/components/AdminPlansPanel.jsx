import PropTypes from 'prop-types';
import './AdminPlansPanel.css';

/**
 * Panel dla administratora: lista wszystkich planów ruchów, rakiet i strzałów laserowych
 */
export function AdminPlansPanel({ battleState }) {
  if (!battleState) return null;

  const fractions = battleState.fractions || [];
  const shipMovementPaths = battleState.shipMovementPaths || [];
  const missileMovementPaths = battleState.missileMovementPaths || [];
  const laserShots = battleState.laserShots || [];

  const getShipName = (shipId) => {
    for (const f of fractions) {
      const ship = f.ships?.find(s => s.shipId === shipId);
      if (ship) return `${ship.name} (${f.fractionName})`;
    }
    return shipId?.slice(0, 8) || '?';
  };

  return (
    <div className="admin-plans-panel">
      <div className="admin-plans-header">
        <h3>Szczegóły dla administratora</h3>
        <div className="admin-plans-meta">
          <span>Tura: <strong>{battleState.turnNumber}</strong></span>
          <span>Status: <strong>{battleState.status}</strong></span>
        </div>
      </div>

      <section className="admin-plans-section">
        <h4>Ruchy statków ({shipMovementPaths.length})</h4>
        {shipMovementPaths.length === 0 ? (
          <p className="admin-plans-empty">Brak zaplanowanych ruchów</p>
        ) : (
          <ul className="admin-plans-list">
            {shipMovementPaths.map((path, idx) => (
              <li key={path.shipId + idx}>
                <span className="ship-name">{getShipName(path.shipId)}</span>
                <span className="path-arrow">→</span>
                <span className="path-target">
                  ({Math.round(path.startPosition?.x ?? 0)}, {Math.round(path.startPosition?.y ?? 0)})
                  {' → '}
                  ({Math.round(path.targetPosition?.x ?? 0)}, {Math.round(path.targetPosition?.y ?? 0)})
                </span>
              </li>
            ))}
          </ul>
        )}
      </section>

      <section className="admin-plans-section">
        <h4>Rakiety w locie ({missileMovementPaths.length})</h4>
        {missileMovementPaths.length === 0 ? (
          <p className="admin-plans-empty">Brak rakiet w locie</p>
        ) : (
          <ul className="admin-plans-list">
            {missileMovementPaths.map((m) => (
              <li key={m.missileId}>
                <span className="ship-name">{m.shipName || getShipName(m.shipId)}</span>
                <span className="path-arrow">→</span>
                <span className="ship-name">{getShipName(m.targetId)}</span>
                <span className="path-detail">(celność: {m.accuracy}%)</span>
              </li>
            ))}
          </ul>
        )}
      </section>

      <section className="admin-plans-section">
        <h4>Strzały laserowe ({laserShots.length})</h4>
        {laserShots.length === 0 ? (
          <p className="admin-plans-empty">Brak strzałów laserowych</p>
        ) : (
          <ul className="admin-plans-list">
            {laserShots.map((ls) => (
              <li key={ls.laserId}>
                <span className="ship-name">{ls.shipName || getShipName(ls.shipId)}</span>
                <span className="path-arrow">→</span>
                <span className="ship-name">{ls.targetName || getShipName(ls.targetId)}</span>
              </li>
            ))}
          </ul>
        )}
      </section>

      <section className="admin-plans-section">
        <h4>Frakcje i statki</h4>
        <ul className="admin-plans-fractions">
          {fractions.map((f) => (
            <li key={f.fractionId}>
              <span
                className="fraction-dot"
                style={{ backgroundColor: f.fractionColor || '#888' }}
              />
              <strong>{f.fractionName}</strong>
              <span className="ships-count">({f.ships?.length ?? 0} statków)</span>
              {f.isDefeated && <span className="defeated-tag">zniszczona</span>}
            </li>
          ))}
        </ul>
      </section>
    </div>
  );
}

AdminPlansPanel.propTypes = {
  battleState: PropTypes.shape({
    turnNumber: PropTypes.number,
    status: PropTypes.string,
    fractions: PropTypes.array,
    shipMovementPaths: PropTypes.array,
    missileMovementPaths: PropTypes.array,
    laserShots: PropTypes.array,
  }),
};
