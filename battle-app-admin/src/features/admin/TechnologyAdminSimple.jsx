import { useState, useEffect } from 'react';
import { gameStateApi } from '../../services/gameStateApi';
import { FRACTIONS_LIST, FRACTION_NAMES } from '../../constants/fractions';
import { useNotification } from '../../contexts/NotificationContext';
import { ConfirmModal } from '../../components/modals/ConfirmModal';
import { useModal } from '../../hooks/useModal';
import './TechnologyAdmin.css';

const SOURCE_LABELS = {
  Research: '🔬 Badania',
  Trade: '💰 Handel',
  Exchange: '🔄 Wymiana',
  Other: '📦 Inne'
};

export const TechnologyAdmin = () => {
  const [technologies, setTechnologies] = useState([]);
  const [fractions] = useState(FRACTIONS_LIST);
  const [selectedFraction, setSelectedFraction] = useState('');
  const [fractionTechsMap, setFractionTechsMap] = useState(new Map()); // Map of techId -> techData
  const [loading, setLoading] = useState(false);
  const [selectedTier, setSelectedTier] = useState(1);
  
  // Add technology form state
  const [showAddForm, setShowAddForm] = useState(false);
  const [pendingTech, setPendingTech] = useState(null);
  const [pendingTechName, setPendingTechName] = useState('');
  const [techSource, setTechSource] = useState('Research');
  const [techComment, setTechComment] = useState('');
  const [sourceFractionId, setSourceFractionId] = useState('');
  const [sourceDescription, setSourceDescription] = useState('');
  const { showSuccess, showError } = useNotification();
  
  // Confirm removal modal
  const confirmModal = useModal();
  const [techToRemove, setTechToRemove] = useState(null);

  useEffect(() => {
    loadTechnologies();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    if (selectedFraction) {
      loadFractionState();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedFraction]);

  const loadTechnologies = async () => {
    try {
      const data = await gameStateApi.getAllTechnologies();
      setTechnologies(data);
      
      if (data.length > 0 && !selectedTier) {
        const firstTier = Math.min(...data.map(t => t.tier));
        setSelectedTier(firstTier);
      }
    } catch (err) {
      showError('Nie udało się załadować technologii');
      console.error(err);
    }
  };

  const loadFractionState = async () => {
    try {
      const state = await gameStateApi.getFractionState(selectedFraction);
      // Store full technology data in a Map for quick lookup
      const techMap = new Map();
      state.technologies.forEach(t => {
        techMap.set(t.technologyId, t);
      });
      setFractionTechsMap(techMap);
    } catch (err) {
      console.error('Failed to load fraction state:', err);
      setFractionTechsMap(new Map());
    }
  };

  const handleTechnologyToggle = (techId, techName, isChecked) => {
    if (!selectedFraction) {
      showError('Najpierw wybierz frakcję');
      return;
    }

    if (isChecked) {
      // Add technology - show form
      setPendingTech(techId);
      setPendingTechName(techName);
      setTechSource('Research');
      setTechComment('');
      setSourceFractionId('');
      setSourceDescription('');
      setShowAddForm(true);
    } else {
      // Remove technology - show confirmation
      setTechToRemove({ id: techId, name: techName });
      confirmModal.openModal();
    }
  };

  const handleConfirmRemove = async () => {
    if (!techToRemove) return;
    
    confirmModal.closeModal();
    await removeTechnology(techToRemove.id);
    setTechToRemove(null);
  };

  const handleCancelRemove = () => {
    confirmModal.closeModal();
    setTechToRemove(null);
  };

  const confirmAddTechnology = async () => {
    if (!pendingTech) return;
    
    setLoading(true);
    try {
      const request = {
        fractionId: selectedFraction,
        technologyId: pendingTech,
        source: techSource,
        comment: techComment || undefined,
        sourceFractionId: techSource === 'Trade' || techSource === 'Exchange' ? sourceFractionId : undefined,
        sourceDescription: techSource === 'Other' ? sourceDescription : undefined
      };
      await gameStateApi.assignTechnology(request);
      
      showSuccess(`Technologia "${pendingTechName}" została dodana`);
      
      setShowAddForm(false);
      setPendingTech(null);
      setPendingTechName('');
      await loadFractionState();
    } catch (err) {
      console.error(err);
      showError('Nie udało się dodać technologii: ' + (err.response?.data?.message || err.message));
    } finally {
      setLoading(false);
    }
  };

  const removeTechnology = async (techId) => {
    setLoading(true);
    try {
      await gameStateApi.removeTechnology(selectedFraction, techId);
      
      showSuccess('Technologia została usunięta');
      
      await loadFractionState();
    } catch (err) {
      console.error(err);
      showError('Nie udało się usunąć technologii');
    } finally {
      setLoading(false);
    }
  };

  const cancelAddForm = () => {
    setShowAddForm(false);
    setPendingTech(null);
    setPendingTechName('');
  };

  const tiers = [...new Set(technologies.map(t => t.tier))].sort((a, b) => a - b);
  const filteredTechs = technologies.filter(t => t.tier === selectedTier);

  return (
    <div style={{ padding: '24px', maxWidth: '1200px', margin: '0 auto' }}>
      <h1>Zarządzanie Technologiami</h1>

      <div style={{ marginBottom: '24px', display: 'flex', gap: '16px' }}>
        <div style={{ flex: 1 }}>
          <label style={{ display: 'block', marginBottom: '8px', fontWeight: '600' }}>Frakcja:</label>
          <select 
            value={selectedFraction} 
            onChange={(e) => setSelectedFraction(e.target.value)}
            style={{ width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #E2E8F0' }}
          >
            <option value="">-- Wybierz Frakcję --</option>
            {fractions.map(f => (
              <option key={f.id} value={f.id}>{f.name}</option>
            ))}
          </select>
        </div>

        <div style={{ flex: 1 }}>
          <label style={{ display: 'block', marginBottom: '8px', fontWeight: '600' }}>Tier:</label>
          <select 
            value={selectedTier} 
            onChange={(e) => setSelectedTier(Number(e.target.value))}
            style={{ width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #E2E8F0' }}
          >
            {tiers.map(tier => (
              <option key={tier} value={tier}>Tier {tier}</option>
            ))}
          </select>
        </div>
      </div>

      {!selectedFraction && (
        <p style={{ color: '#718096' }}>Wybierz frakcję aby zarządzać technologiami</p>
      )}

      {selectedFraction && (
        <div>
          <h2>Tier {selectedTier} - Technologie ({filteredTechs.length})</h2>
          {filteredTechs.length === 0 ? (
            <p style={{ color: '#718096' }}>Brak technologii dla Tier {selectedTier}</p>
          ) : (
            <div>
              {filteredTechs.map(tech => {
                const ownedTech = fractionTechsMap.get(tech.id);
                const isOwned = !!ownedTech;
                
                return (
                  <div
                    key={tech.id}
                    style={{ 
                      padding: '16px', 
                      border: '1px solid #E2E8F0', 
                      borderRadius: '8px', 
                      marginBottom: '12px',
                      backgroundColor: isOwned ? '#F0FFF4' : 'white',
                      borderLeft: isOwned ? '4px solid #38A169' : '1px solid #E2E8F0'
                    }}
                  >
                    <div style={{ display: 'flex', alignItems: 'start', gap: '12px' }}>
                      <input
                        type="checkbox"
                        checked={isOwned}
                        onChange={(e) => handleTechnologyToggle(tech.id, tech.name, e.target.checked)}
                        disabled={loading}
                        style={{ marginTop: '4px', width: '18px', height: '18px', cursor: 'pointer' }}
                      />
                      <div style={{ flex: 1 }}>
                        <div style={{ display: 'flex', alignItems: 'center', gap: '8px', flexWrap: 'wrap' }}>
                          <strong style={{ fontSize: '16px' }}>{tech.name}</strong>
                          {isOwned && (
                            <span style={{ 
                              backgroundColor: '#38A169', 
                              color: 'white', 
                              padding: '2px 8px', 
                              borderRadius: '4px', 
                              fontSize: '11px',
                              fontWeight: '600'
                            }}>
                              ✓ POSIADANA
                            </span>
                          )}
                        </div>
                        
                        <p style={{ fontSize: '14px', color: '#4A5568', margin: '6px 0' }}>{tech.description}</p>
                        
                        {tech.requiredTechnologies && tech.requiredTechnologies.length > 0 && (
                          <div style={{ marginTop: '8px' }}>
                            <small style={{ color: '#718096' }}>
                              📋 Wymaga: {tech.requiredTechnologies.join(', ')}
                            </small>
                          </div>
                        )}
                        
                        {/* Show source info if owned */}
                        {isOwned && ownedTech && (
                          <div style={{ 
                            marginTop: '12px', 
                            padding: '10px', 
                            backgroundColor: '#E6FFFA', 
                            borderRadius: '6px',
                            border: '1px solid #81E6D9'
                          }}>
                            <div style={{ display: 'flex', flexWrap: 'wrap', gap: '16px', fontSize: '13px' }}>
                              <div>
                                <span style={{ color: '#718096' }}>Źródło: </span>
                                <strong style={{ color: '#234E52' }}>
                                  {SOURCE_LABELS[ownedTech.source] || ownedTech.source}
                                </strong>
                              </div>
                              
                              {ownedTech.sourceFractionName && (
                                <div>
                                  <span style={{ color: '#718096' }}>Od frakcji: </span>
                                  <strong style={{ color: '#234E52' }}>
                                    {FRACTION_NAMES[ownedTech.sourceFractionName] || ownedTech.sourceFractionName}
                                  </strong>
                                </div>
                              )}
                              
                              <div>
                                <span style={{ color: '#718096' }}>Data: </span>
                                <strong style={{ color: '#234E52' }}>
                                  {new Date(ownedTech.acquiredDate).toLocaleDateString('pl-PL')}
                                </strong>
                              </div>
                            </div>
                            
                            {ownedTech.sourceDescription && (
                              <div style={{ marginTop: '8px', fontSize: '13px' }}>
                                <span style={{ color: '#718096' }}>Opis: </span>
                                <span style={{ color: '#234E52', fontStyle: 'italic' }}>
                                  {ownedTech.sourceDescription}
                                </span>
                              </div>
                            )}
                            
                            {ownedTech.comment && (
                              <div style={{ 
                                marginTop: '8px', 
                                padding: '8px', 
                                backgroundColor: '#FFFBEB', 
                                borderRadius: '4px',
                                borderLeft: '3px solid #F6AD55',
                                fontSize: '13px'
                              }}>
                                <span style={{ color: '#744210' }}>💬 </span>
                                <span style={{ color: '#744210' }}>{ownedTech.comment}</span>
                              </div>
                            )}
                          </div>
                        )}
                      </div>
                    </div>
                  </div>
                );
              })}
            </div>
          )}
        </div>
      )}

      {/* Modal do dodawania technologii */}
      {showAddForm && pendingTech && (
        <div style={{
          position: 'fixed',
          top: 0,
          left: 0,
          right: 0,
          bottom: 0,
          backgroundColor: 'rgba(0, 0, 0, 0.5)',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          zIndex: 1000
        }}>
          <div style={{ 
            backgroundColor: 'white', 
            borderRadius: '12px', 
            padding: '24px',
            maxWidth: '500px',
            width: '90%',
            maxHeight: '90vh',
            overflow: 'auto',
            boxShadow: '0 20px 50px rgba(0, 0, 0, 0.3)'
          }}>
            <h3 style={{ margin: '0 0 16px', color: '#1A202C' }}>
              Dodaj technologię: <span style={{ color: '#3182CE' }}>{pendingTechName}</span>
            </h3>
            <p style={{ margin: '0 0 16px', color: '#718096', fontSize: '14px' }}>
              Frakcja: <strong>{FRACTION_NAMES[selectedFraction]}</strong>
            </p>
            
            <div style={{ marginBottom: '16px' }}>
              <label style={{ display: 'block', marginBottom: '8px', fontWeight: '600' }}>Źródło: *</label>
              <select 
                value={techSource} 
                onChange={(e) => setTechSource(e.target.value)} 
                style={{ width: '100%', padding: '10px', borderRadius: '6px', border: '1px solid #E2E8F0', fontSize: '14px' }}
              >
                <option value="Research">🔬 Badania</option>
                <option value="Trade">💰 Handel</option>
                <option value="Exchange">🔄 Wymiana z inną frakcją</option>
                <option value="Other">📦 Inne</option>
              </select>
            </div>

            {(techSource === 'Trade' || techSource === 'Exchange') && (
              <div style={{ marginBottom: '16px' }}>
                <label style={{ display: 'block', marginBottom: '8px', fontWeight: '600' }}>Frakcja źródłowa:</label>
                <select
                  value={sourceFractionId}
                  onChange={(e) => setSourceFractionId(e.target.value)}
                  style={{ width: '100%', padding: '10px', borderRadius: '6px', border: '1px solid #E2E8F0', fontSize: '14px' }}
                >
                  <option value="">-- Wybierz Frakcję --</option>
                  {fractions.filter(f => f.id !== selectedFraction).map(f => (
                    <option key={f.id} value={f.id}>{f.name}</option>
                  ))}
                </select>
              </div>
            )}

            {techSource === 'Other' && (
              <div style={{ marginBottom: '16px' }}>
                <label style={{ display: 'block', marginBottom: '8px', fontWeight: '600' }}>Opis źródła:</label>
                <textarea
                  value={sourceDescription}
                  onChange={(e) => setSourceDescription(e.target.value)}
                  placeholder="Np. Znaleziono w ruinach, kupione od neutralnych..."
                  style={{ width: '100%', padding: '10px', borderRadius: '6px', border: '1px solid #E2E8F0', minHeight: '80px', fontSize: '14px', resize: 'vertical' }}
                />
              </div>
            )}

            <div style={{ marginBottom: '24px' }}>
              <label style={{ display: 'block', marginBottom: '8px', fontWeight: '600' }}>Komentarz (opcjonalnie):</label>
              <textarea
                value={techComment}
                onChange={(e) => setTechComment(e.target.value)}
                placeholder="Dodatkowe notatki..."
                style={{ width: '100%', padding: '10px', borderRadius: '6px', border: '1px solid #E2E8F0', minHeight: '80px', fontSize: '14px', resize: 'vertical' }}
              />
            </div>

            <div style={{ display: 'flex', gap: '12px', justifyContent: 'flex-end' }}>
              <button
                onClick={cancelAddForm}
                disabled={loading}
                style={{ 
                  padding: '10px 20px', 
                  borderRadius: '6px', 
                  border: '1px solid #E2E8F0', 
                  backgroundColor: 'white', 
                  cursor: 'pointer',
                  fontSize: '14px',
                  fontWeight: '500'
                }}
              >
                Anuluj
              </button>
              <button
                onClick={confirmAddTechnology}
                disabled={loading}
                style={{ 
                  padding: '10px 20px', 
                  borderRadius: '6px', 
                  border: 'none', 
                  backgroundColor: '#38A169', 
                  color: 'white', 
                  cursor: loading ? 'not-allowed' : 'pointer',
                  fontSize: '14px',
                  fontWeight: '600',
                  opacity: loading ? 0.7 : 1
                }}
              >
                {loading ? '⏳ Dodawanie...' : '✓ Dodaj Technologię'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Modal potwierdzenia usunięcia */}
      <ConfirmModal
        isOpen={confirmModal.isOpen}
        onClose={handleCancelRemove}
        onConfirm={handleConfirmRemove}
        title="Potwierdzenie usunięcia"
        message={`Czy na pewno chcesz usunąć technologię "${techToRemove?.name}" z frakcji ${FRACTION_NAMES[selectedFraction]}? Ta operacja jest nieodwracalna.`}
        confirmText="Usuń"
        cancelText="Anuluj"
        colorScheme="red"
      />
    </div>
  );
};
