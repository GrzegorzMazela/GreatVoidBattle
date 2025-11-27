import { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import { gameStateApi } from '../../services/gameStateApi';
import { FRACTION_NAMES } from '../../constants/fractions';
import { useNotification } from '../../contexts/NotificationContext';
import './ResearchRequests.css';

/**
 * Fraction view - research requests with slot system
 * - Tier 1 always accessible
 * - Higher tiers require 15 researched technologies from previous tier
 * - Can see current accessible tier + one tier ahead (preview)
 * - Slot cost = tier number (Tier 1 = 1 slot, Tier 2 = 2 slots, etc.)
 */
export default function ResearchRequests() {
  const { fractionId: routeFractionId } = useParams();
  const [availableTechs, setAvailableTechs] = useState([]);
  const [pendingRequests, setPendingRequests] = useState([]);
  const [fractionState, setFractionState] = useState(null);
  const [loading, setLoading] = useState(true);
  const { showSuccess, showError } = useNotification();

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
      const [state, techs, requests] = await Promise.all([
        gameStateApi.getFractionState(fractionId),
        gameStateApi.getAvailableTechnologies(fractionId),
        gameStateApi.getPendingRequests(fractionId)
      ]);

      setFractionState(state);
      setAvailableTechs(techs);
      setPendingRequests(requests);
    } catch (error) {
      console.error('Error loading data:', error);
      showError('Nie udało się załadować danych');
    } finally {
      setLoading(false);
    }
  };

  const handleRequestResearch = async (technologyId, tier) => {
    // Check if we have enough slots
    const slotsNeeded = tier;
    const availableSlots = fractionState.researchSlots - fractionState.usedResearchSlots;
    
    if (slotsNeeded > availableSlots) {
      showError(`Potrzebujesz ${slotsNeeded} slotów, a masz dostępne tylko ${availableSlots}`);
      return;
    }

    try {
      await gameStateApi.requestResearch(fractionId, technologyId);
      showSuccess('Zgłoszono do badań - oczekuje na zatwierdzenie');
      await loadData();
    } catch (error) {
      console.error('Error requesting research:', error);
      showError('Nie udało się zgłosić technologii do badań');
    }
  };

  if (loading) {
    return (
      <div className="research-requests loading">
        <div className="spinner"></div>
        <p>Ładowanie...</p>
      </div>
    );
  }

  if (!fractionState) {
    return (
      <div className="research-requests error">
        <p>Nie znaleziono danych frakcji</p>
      </div>
    );
  }

  const pendingTechIds = new Set(pendingRequests.map(r => r.technologyId));
  const availableSlots = fractionState.researchSlots - fractionState.usedResearchSlots;

  return (
    <div className="research-requests">
      {/* Header with fraction info and slots */}
      <div className="header">
        <div className="header-main">
          <h1>{FRACTION_NAMES[fractionId] || fractionId}</h1>
          <p className="subtitle">Zlecenia badań naukowych</p>
        </div>
        
        <div className="slots-panel">
          <div className="slots-info">
            <span className="slots-label">Sloty badawcze:</span>
            <div className="slots-visual">
              {Array.from({ length: fractionState.researchSlots }, (_, i) => (
                <div 
                  key={i} 
                  className={`slot ${i < fractionState.usedResearchSlots ? 'used' : 'available'}`}
                />
              ))}
            </div>
            <span className="slots-numbers">
              {availableSlots} / {fractionState.researchSlots} dostępnych
            </span>
          </div>
        </div>
      </div>

      {/* Pending requests */}
      {pendingRequests.length > 0 && (
        <div className="pending-section">
          <h2>
            <span className="icon">⏳</span>
            Oczekujące zgłoszenia ({pendingRequests.length})
          </h2>
          <div className="pending-list">
            {pendingRequests.map(request => {
              const tech = availableTechs
                .flatMap(t => t.technologies)
                .find(t => t.id === request.technologyId);
              const tier = tech?.tier || 1;
              
              return (
                <div key={request.technologyId} className="pending-item">
                  <div className="pending-info">
                    <span className="tech-name">{request.technologyName}</span>
                    <span className="tier-badge small">Tier {tier}</span>
                  </div>
                  <div className="pending-status">
                    <span className="status-badge pending">Oczekuje</span>
                    <span className="slots-cost">{tier} {tier === 1 ? 'slot' : 'sloty'}</span>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      )}

      {/* Technologies by tier */}
      <div className="tiers-section">
        {availableTechs.map(tierData => {
          const unownedTechs = tierData.technologies.filter(t => !t.isOwned);
          
          return (
            <div key={tierData.tier} className={`tier-card ${!tierData.canResearch ? 'locked' : ''}`}>
              <div className="tier-header">
                <div className="tier-title">
                  <span className="tier-badge">Tier {tierData.tier}</span>
                  {!tierData.canResearch && tierData.canView && (
                    <span className="preview-badge">Podgląd</span>
                  )}
                </div>
                <div className="tier-progress">
                  <div className="progress-bar">
                    <div 
                      className="progress-fill" 
                      style={{ width: `${(tierData.researchedCount / tierData.requiredForNextTier) * 100}%` }}
                    />
                  </div>
                  <span className="progress-text">
                    {tierData.researchedCount} / {tierData.requiredForNextTier} zbadanych
                  </span>
                </div>
                {!tierData.canResearch && (
                  <div className="lock-message">
                    🔒 Wymaga {tierData.requiredForNextTier} technologii z Tier {tierData.tier - 1}
                  </div>
                )}
              </div>

              <div className="tech-list">
                {unownedTechs.length === 0 ? (
                  <div className="all-owned-message">
                    ✓ Wszystkie technologie z tego tiera są już zbadane
                  </div>
                ) : (
                  unownedTechs.map(tech => {
                    const isPending = pendingTechIds.has(tech.id);
                    const canAfford = availableSlots >= tech.slotsCost;
                    const canRequest = tech.canResearch && !isPending && canAfford && tierData.canResearch;

                    return (
                      <div 
                        key={tech.id} 
                        className={`tech-item ${isPending ? 'pending' : ''} ${!tierData.canResearch ? 'preview' : ''}`}
                      >
                        <div className="tech-main">
                          <div className="tech-header">
                            <h3>{tech.name}</h3>
                            <div className="tech-badges">
                              {isPending && <span className="status-badge pending">Zgłoszone</span>}
                              {tech.isOwned && <span className="status-badge owned">Posiadane</span>}
                              <span className="cost-badge">{tech.slotsCost} {tech.slotsCost === 1 ? 'slot' : 'sloty'}</span>
                            </div>
                          </div>
                          <p className="tech-description">{tech.description}</p>
                          
                          {tech.requiredTechnologies?.length > 0 && (
                            <div className="requirements">
                              <span className="req-label">Wymaga:</span>
                              <div className="req-list">
                                {tech.requiredTechnologies.map(req => {
                                  const isMissing = tech.missingRequirements?.includes(req);
                                  return (
                                    <span key={req} className={`req-item ${isMissing ? 'missing' : 'met'}`}>
                                      {isMissing ? '✗' : '✓'} {req}
                                    </span>
                                  );
                                })}
                              </div>
                            </div>
                          )}
                        </div>

                        <div className="tech-actions">
                          {tierData.canResearch ? (
                            <button
                              className={`research-btn ${canRequest ? '' : 'disabled'}`}
                              onClick={() => handleRequestResearch(tech.id, tech.tier)}
                              disabled={!canRequest}
                            >
                              {isPending ? 'Zgłoszono' : 
                               !canAfford ? 'Brak slotów' :
                               tech.missingRequirements?.length > 0 ? 'Brak wymagań' :
                               'Zgłoś do badań'}
                            </button>
                          ) : (
                            <span className="locked-text">Tier zablokowany</span>
                          )}
                        </div>
                      </div>
                    );
                  })
                )}
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}
