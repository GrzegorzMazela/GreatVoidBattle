import { useState, useEffect } from 'react';
import { gameStateApi } from '../../services/gameStateApi';
import './TechnologyAdmin.css';

export const TechnologyAdmin = () => {
  const [technologies, setTechnologies] = useState([]);
  const [fractions] = useState([
    { id: 'hegemonia_titanum', name: 'Hegemonia Titanum' },
    { id: 'shimura_incorporated', name: 'Shimura Incorporated' },
    { id: 'protektorat_pogranicza', name: 'Protektorat Pogranicza' }
  ]);
  const [selectedFraction, setSelectedFraction] = useState('');
  const [fractionTechs, setFractionTechs] = useState(new Set());
  const [loading, setLoading] = useState(false);
  const [selectedTier, setSelectedTier] = useState(1);
  
  // Add technology form state
  const [showAddForm, setShowAddForm] = useState(false);
  const [pendingTech, setPendingTech] = useState(null);
  const [techSource, setTechSource] = useState('Research');
  const [techComment, setTechComment] = useState('');
  const [sourceFractionId, setSourceFractionId] = useState('');
  const [sourceDescription, setSourceDescription] = useState('');
  const [successMessage, setSuccessMessage] = useState('');
  const [errorMessage, setErrorMessage] = useState('');

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
      setErrorMessage('Nie udało się załadować technologii');
      setTimeout(() => setErrorMessage(''), 3000);
      console.error(err);
    }
  };

  const loadFractionState = async () => {
    try {
      const state = await gameStateApi.getFractionState(selectedFraction);
      const techIds = new Set(state.technologies.map(t => t.technologyId));
      setFractionTechs(techIds);
    } catch (err) {
      console.error('Failed to load fraction state:', err);
      setFractionTechs(new Set());
    }
  };

  const handleTechnologyToggle = async (techId, isChecked) => {
    if (!selectedFraction) {
      setErrorMessage('Najpierw wybierz frakcję');
      setTimeout(() => setErrorMessage(''), 3000);
      return;
    }

    if (isChecked) {
      setPendingTech(techId);
      setTechSource('Research');
      setTechComment('');
      setSourceFractionId('');
      setSourceDescription('');
      setShowAddForm(true);
    } else {
      await removeTechnology(techId);
    }
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
      
      setSuccessMessage('Technologia została dodana');
      setTimeout(() => setSuccessMessage(''), 3000);
      
      setShowAddForm(false);
      setPendingTech(null);
      await loadFractionState();
    } catch (err) {
      console.error(err);
      setErrorMessage('Nie udało się dodać technologii');
      setTimeout(() => setErrorMessage(''), 3000);
    } finally {
      setLoading(false);
    }
  };

  const removeTechnology = async (techId) => {
    setLoading(true);
    try {
      await gameStateApi.removeTechnology(selectedFraction, techId);
      
      setSuccessMessage('Technologia została usunięta');
      setTimeout(() => setSuccessMessage(''), 3000);
      
      await loadFractionState();
    } catch (err) {
      console.error(err);
      setErrorMessage('Nie udało się usunąć technologii');
      setTimeout(() => setErrorMessage(''), 3000);
    } finally {
      setLoading(false);
    }
  };

  const tiers = [...new Set(technologies.map(t => t.tier))].sort((a, b) => a - b);
  const filteredTechs = technologies.filter(t => t.tier === selectedTier);

  return (
    <div style={{ padding: '24px', maxWidth: '1200px', margin: '0 auto' }}>
      <h1>Zarządzanie Technologiami</h1>

      {successMessage && (
        <div style={{ padding: '12px', marginBottom: '16px', backgroundColor: '#C6F6D5', borderRadius: '8px', color: '#22543D' }}>
          ✓ {successMessage}
        </div>
      )}

      {errorMessage && (
        <div style={{ padding: '12px', marginBottom: '16px', backgroundColor: '#FED7D7', borderRadius: '8px', color: '#742A2A' }}>
          ✗ {errorMessage}
        </div>
      )}

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
              {filteredTechs.map(tech => (
                <div
                  key={tech.id}
                  style={{ padding: '12px', border: '1px solid #E2E8F0', borderRadius: '8px', marginBottom: '8px' }}
                >
                  <label style={{ display: 'flex', alignItems: 'start', cursor: 'pointer' }}>
                    <input
                      type="checkbox"
                      checked={fractionTechs.has(tech.id)}
                      onChange={(e) => handleTechnologyToggle(tech.id, e.target.checked)}
                      disabled={loading}
                      style={{ marginRight: '12px', marginTop: '4px' }}
                    />
                    <div>
                      <strong>{tech.name}</strong>
                      <p style={{ fontSize: '14px', color: '#4A5568', margin: '4px 0' }}>{tech.description}</p>
                      {tech.requiredTechnologies && tech.requiredTechnologies.length > 0 && (
                        <small style={{ color: '#718096' }}>
                          Wymaga: {tech.requiredTechnologies.join(', ')}
                        </small>
                      )}
                    </div>
                  </label>
                </div>
              ))}
            </div>
          )}
        </div>
      )}

      {showAddForm && pendingTech && (
        <div style={{ marginTop: '24px', padding: '24px', backgroundColor: '#EBF8FF', borderRadius: '8px', border: '2px solid #90CDF4' }}>
          <h3>Dodaj Technologię</h3>
          <div style={{ marginBottom: '16px' }}>
            <label style={{ display: 'block', marginBottom: '8px', fontWeight: '600' }}>Źródło: *</label>
            <select value={techSource} onChange={(e) => setTechSource(e.target.value)} style={{ width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #E2E8F0' }}>
              <option value="Research">Badania</option>
              <option value="Trade">Handel</option>
              <option value="Exchange">Wymiana z inną frakcją</option>
              <option value="Other">Inne</option>
            </select>
          </div>

          {(techSource === 'Trade' || techSource === 'Exchange') && (
            <div style={{ marginBottom: '16px' }}>
              <label style={{ display: 'block', marginBottom: '8px', fontWeight: '600' }}>Frakcja źródłowa:</label>
              <select
                value={sourceFractionId}
                onChange={(e) => setSourceFractionId(e.target.value)}
                style={{ width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #E2E8F0' }}
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
                style={{ width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #E2E8F0', minHeight: '80px' }}
              />
            </div>
          )}

          <div style={{ marginBottom: '16px' }}>
            <label style={{ display: 'block', marginBottom: '8px', fontWeight: '600' }}>Komentarz (opcjonalnie):</label>
            <textarea
              value={techComment}
              onChange={(e) => setTechComment(e.target.value)}
              placeholder="Dodatkowe notatki..."
              style={{ width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #E2E8F0', minHeight: '80px' }}
            />
          </div>

          <div style={{ display: 'flex', gap: '12px' }}>
            <button
              onClick={() => {
                setShowAddForm(false);
                setPendingTech(null);
              }}
              style={{ padding: '8px 16px', borderRadius: '4px', border: '1px solid #E2E8F0', backgroundColor: 'white', cursor: 'pointer' }}
            >
              Anuluj
            </button>
            <button
              onClick={confirmAddTechnology}
              disabled={loading}
              style={{ padding: '8px 16px', borderRadius: '4px', border: 'none', backgroundColor: '#3182CE', color: 'white', cursor: loading ? 'not-allowed' : 'pointer' }}
            >
              {loading ? 'Dodawanie...' : 'Dodaj Technologię'}
            </button>
          </div>
        </div>
      )}
    </div>
  );
};
