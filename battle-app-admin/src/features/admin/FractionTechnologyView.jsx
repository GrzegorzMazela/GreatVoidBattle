import { useState, useEffect } from 'react';
import { gameStateApi } from '../../services/gameStateApi';
import './FractionTechnologyView.css';

export const FractionTechnologyView = ({ fractionId }) => {
  const [state, setState] = useState(null);
  const [availableTechs, setAvailableTechs] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [selectedTier, setSelectedTier] = useState(1);

  useEffect(() => {
    loadData();
  }, [fractionId]);

  const loadData = async () => {
    setLoading(true);
    try {
      const [fractionState, techs] = await Promise.all([
        gameStateApi.getFractionState(fractionId),
        gameStateApi.getAvailableTechnologies(fractionId)
      ]);
      setState(fractionState);
      setAvailableTechs(techs);
      setSelectedTier(fractionState.currentTier);
    } catch (err) {
      setError('Failed to load data');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleResearch = async (techId) => {
    if (!state) return;

    setLoading(true);
    setError('');

    try {
      const request = {
        fractionId: state.fractionId,
        technologyId: techId,
        source: 'Research'
      };
      await gameStateApi.assignTechnology(request);
      await loadData();
    } catch (err) {
      setError('Failed to research technology');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleAdvanceTier = async () => {
    if (!state) return;

    setLoading(true);
    setError('');

    try {
      await gameStateApi.advanceTier(state.fractionId);
      await loadData();
    } catch (err) {
      setError('Failed to advance tier');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  if (loading && !state) {
    return <div className="loading">Loading...</div>;
  }

  if (error && !state) {
    return <div className="error">{error}</div>;
  }

  if (!state) {
    return null;
  }

  const currentTierData = availableTechs.find(t => t.tier === selectedTier);

  return (
    <div className="fraction-tech-view">
      <div className="tech-header">
        <div>
          <h1>{state.fractionName}</h1>
          <p className="tier-info">Current Tier: {state.currentTier}</p>
        </div>
        <div className="progress-card">
          <h3>Research Progress</h3>
          <div className="progress-bar">
            <div 
              className="progress-fill" 
              style={{ width: `${(state.researchedTechnologiesInCurrentTier / 15) * 100}%` }}
            />
          </div>
          <p>{state.researchedTechnologiesInCurrentTier} / 15 technologies researched</p>
          {state.canAdvanceToNextTier && (
            <button 
              className="btn-advance" 
              onClick={handleAdvanceTier}
              disabled={loading}
            >
              Advance to Tier {state.currentTier + 1}
            </button>
          )}
        </div>
      </div>

      {error && <div className="error-banner">{error}</div>}

      <div className="tier-selector">
        <label>View Tier:</label>
        <div className="tier-buttons">
          {availableTechs.map(tierData => (
            <button
              key={tierData.tier}
              className={selectedTier === tierData.tier ? 'active' : ''}
              onClick={() => setSelectedTier(tierData.tier)}
            >
              Tier {tierData.tier}
            </button>
          ))}
        </div>
      </div>

      {currentTierData && (
        <div className="technologies-section">
          <h2>Tier {selectedTier} Technologies</h2>
          
          <div className="tech-grid">
            {currentTierData.technologies.map(tech => (
              <div 
                key={tech.id} 
                className={`tech-card ${tech.isOwned ? 'owned' : ''} ${!tech.canResearch && !tech.isOwned ? 'locked' : ''}`}
              >
                <div className="tech-card-header">
                  <h3>{tech.name}</h3>
                  {tech.isOwned && <span className="badge owned-badge">Owned</span>}
                  {!tech.isOwned && tech.canResearch && <span className="badge available-badge">Available</span>}
                  {!tech.isOwned && !tech.canResearch && <span className="badge locked-badge">Locked</span>}
                </div>
                
                <p className="tech-description">{tech.description}</p>
                
                {tech.requiredTechnologies.length > 0 && (
                  <div className="requirements">
                    <strong>Requirements:</strong>
                    <ul>
                      {tech.requiredTechnologies.map(req => {
                        const isMissing = tech.missingRequirements.includes(req);
                        return (
                          <li key={req} className={isMissing ? 'missing' : 'met'}>
                            {req} {isMissing ? '✗' : '✓'}
                          </li>
                        );
                      })}
                    </ul>
                  </div>
                )}

                {!tech.isOwned && tech.canResearch && selectedTier === state.currentTier && (
                  <button 
                    className="btn-research"
                    onClick={() => handleResearch(tech.id)}
                    disabled={loading}
                  >
                    Research
                  </button>
                )}
              </div>
            ))}
          </div>
        </div>
      )}

      <div className="owned-technologies">
        <h2>Owned Technologies ({state.technologies.length})</h2>
        <div className="owned-list">
          {state.technologies.map(tech => (
            <div key={tech.technologyId} className="owned-item">
              <div>
                <strong>{tech.technologyName}</strong>
                <span className="source-badge">{tech.source}</span>
              </div>
              <small>Acquired: {new Date(tech.acquiredDate).toLocaleDateString()}</small>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
};
