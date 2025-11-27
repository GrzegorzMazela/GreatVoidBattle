import { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import { gameStateApi } from '../../services/gameStateApi';
import { FRACTION_NAMES } from '../../constants/fractions';
import { useNotification } from '../../contexts/NotificationContext';
import './OwnedTechnologies.css';

/**
 * Fraction view - shows all owned technologies grouped by tier
 * Only shows tiers that have at least one technology
 */
export const OwnedTechnologies = () => {
  const { fractionId: routeFractionId } = useParams();
  const [fractionState, setFractionState] = useState(null);
  const [loading, setLoading] = useState(true);
  const { showError } = useNotification();

  // Get fractionId from route or determine from path
  const fractionId = routeFractionId || getFractionIdFromPath();

  function getFractionIdFromPath() {
    const path = window.location.pathname;
    if (path.includes('hegemonia')) return 'hegemonia_titanum';
    if (path.includes('shimura')) return 'shimura_incorporated';
    if (path.includes('protektorat')) return 'protektorat_pogranicza';
    return null;
  }

  useEffect(() => {
    if (fractionId) {
      loadData();
    }
  }, [fractionId]);

  const loadData = async () => {
    try {
      setLoading(true);
      const state = await gameStateApi.getFractionState(fractionId);
      setFractionState(state);
    } catch (err) {
      console.error('Failed to load fraction state:', err);
      showError('Nie udało się załadować danych frakcji');
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="owned-technologies loading">
        <div className="spinner"></div>
        <p>Ładowanie technologii...</p>
      </div>
    );
  }

  if (!fractionState) {
    return (
      <div className="owned-technologies error">
        <p>Nie znaleziono danych frakcji</p>
      </div>
    );
  }

  // Group technologies by tier
  const techsByTier = fractionState.technologies.reduce((acc, tech) => {
    const tier = tech.tier || 1;
    if (!acc[tier]) acc[tier] = [];
    acc[tier].push(tech);
    return acc;
  }, {});

  const tiers = Object.keys(techsByTier).map(Number).sort((a, b) => a - b);

  const getSourceLabel = (source) => {
    switch (source) {
      case 'Research': return '🔬 Badania';
      case 'Trade': return '💰 Handel';
      case 'Exchange': return '🔄 Wymiana';
      case 'Other': return '📦 Inne';
      default: return source;
    }
  };

  return (
    <div className="owned-technologies">
      <div className="header">
        <h1>{FRACTION_NAMES[fractionId] || fractionId}</h1>
        <p className="subtitle">
          Posiadane technologie: {fractionState.technologies.length}
        </p>
      </div>

      {tiers.length === 0 ? (
        <div className="empty-state">
          <p>Frakcja nie posiada jeszcze żadnych technologii</p>
        </div>
      ) : (
        <div className="tiers-container">
          {tiers.map(tier => (
            <div key={tier} className="tier-section">
              <h2 className="tier-header">
                <span className="tier-badge">Tier {tier}</span>
                <span className="tech-count">{techsByTier[tier].length} technologii</span>
              </h2>
              
              <div className="tech-grid">
                {techsByTier[tier].map(tech => (
                  <div key={tech.technologyId} className="tech-card owned">
                    <div className="tech-card-header">
                      <h3>{tech.technologyName}</h3>
                    </div>
                    <div className="tech-card-body">
                      <div className="tech-source">
                        {getSourceLabel(tech.source)}
                        {tech.sourceFractionName && (
                          <span className="source-fraction"> od {tech.sourceFractionName}</span>
                        )}
                      </div>
                      {tech.sourceDescription && (
                        <p className="source-description">{tech.sourceDescription}</p>
                      )}
                      {tech.comment && (
                        <p className="tech-comment">{tech.comment}</p>
                      )}
                      <div className="acquired-date">
                        Zdobyto: {new Date(tech.acquiredDate).toLocaleDateString('pl-PL')}
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default OwnedTechnologies;

