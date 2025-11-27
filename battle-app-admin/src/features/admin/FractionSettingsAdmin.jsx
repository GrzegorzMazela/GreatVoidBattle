import { useState, useEffect } from 'react';
import { gameStateApi } from '../../services/gameStateApi';
import { FRACTION_NAMES } from '../../constants/fractions';
import { useNotification } from '../../contexts/NotificationContext';
import './TechnologyAdmin.css';

/**
 * Admin panel for managing fraction settings (research slots, etc.)
 */
export const FractionSettingsAdmin = () => {
  const [fractionStates, setFractionStates] = useState([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState({});
  const { showSuccess, showError } = useNotification();

  useEffect(() => {
    loadFractionStates();
  }, []);

  const loadFractionStates = async () => {
    try {
      setLoading(true);
      const states = await gameStateApi.getAllFractionStates();
      setFractionStates(states);
    } catch (err) {
      console.error('Failed to load fraction states:', err);
      showError('Nie udało się załadować stanów frakcji');
    } finally {
      setLoading(false);
    }
  };

  const handleSlotsChange = async (fractionId, newSlots) => {
    if (newSlots < 0) return;
    
    setSaving(prev => ({ ...prev, [fractionId]: true }));
    try {
      await gameStateApi.setResearchSlots(fractionId, newSlots);
      showSuccess('Sloty badawcze zostały zaktualizowane');
      await loadFractionStates();
    } catch (err) {
      console.error('Failed to update slots:', err);
      showError('Nie udało się zaktualizować slotów');
    } finally {
      setSaving(prev => ({ ...prev, [fractionId]: false }));
    }
  };

  if (loading) {
    return <div style={{ padding: '24px' }}>Ładowanie...</div>;
  }

  return (
    <div style={{ padding: '24px', maxWidth: '1200px', margin: '0 auto' }}>
      <h1>Ustawienia Frakcji</h1>
      <p style={{ color: '#718096', marginBottom: '24px' }}>
        Zarządzaj slotami badawczymi i innymi ustawieniami frakcji.
      </p>

      <div style={{ display: 'grid', gap: '24px' }}>
        {fractionStates.map(state => (
          <div
            key={state.fractionId}
            style={{
              backgroundColor: 'white',
              borderRadius: '12px',
              border: '1px solid #E2E8F0',
              padding: '24px',
              boxShadow: '0 1px 3px rgba(0,0,0,0.1)'
            }}
          >
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'start', marginBottom: '16px' }}>
              <div>
                <h2 style={{ margin: 0, fontSize: '20px' }}>
                  {FRACTION_NAMES[state.fractionId] || state.fractionId}
                </h2>
                <p style={{ color: '#718096', fontSize: '14px', margin: '4px 0 0' }}>
                  {state.technologies?.length || 0} technologii posiadanych
                </p>
              </div>
            </div>

            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: '16px' }}>
              {/* Research Slots */}
              <div style={{ 
                backgroundColor: '#F7FAFC', 
                padding: '16px', 
                borderRadius: '8px',
                border: '1px solid #E2E8F0'
              }}>
                <label style={{ display: 'block', fontWeight: '600', marginBottom: '8px' }}>
                  Sloty Badawcze
                </label>
                <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
                  <button
                    onClick={() => handleSlotsChange(state.fractionId, state.researchSlots - 1)}
                    disabled={saving[state.fractionId] || state.researchSlots <= 0}
                    style={{
                      width: '36px',
                      height: '36px',
                      borderRadius: '8px',
                      border: '1px solid #E2E8F0',
                      backgroundColor: 'white',
                      fontSize: '18px',
                      cursor: 'pointer'
                    }}
                  >
                    -
                  </button>
                  <span style={{ 
                    fontSize: '24px', 
                    fontWeight: 'bold',
                    minWidth: '40px',
                    textAlign: 'center'
                  }}>
                    {state.researchSlots}
                  </span>
                  <button
                    onClick={() => handleSlotsChange(state.fractionId, state.researchSlots + 1)}
                    disabled={saving[state.fractionId]}
                    style={{
                      width: '36px',
                      height: '36px',
                      borderRadius: '8px',
                      border: '1px solid #E2E8F0',
                      backgroundColor: 'white',
                      fontSize: '18px',
                      cursor: 'pointer'
                    }}
                  >
                    +
                  </button>
                </div>
                <p style={{ fontSize: '12px', color: '#718096', marginTop: '8px' }}>
                  Używane: {state.usedResearchSlots || 0} / {state.researchSlots}
                </p>
              </div>

              {/* Tier Progress */}
              <div style={{ 
                backgroundColor: '#F7FAFC', 
                padding: '16px', 
                borderRadius: '8px',
                border: '1px solid #E2E8F0'
              }}>
                <label style={{ display: 'block', fontWeight: '600', marginBottom: '8px' }}>
                  Postęp Badań
                </label>
                <div style={{ display: 'flex', flexDirection: 'column', gap: '8px' }}>
                  {(state.tierProgress || []).map(tier => (
                    <div key={tier.tier} style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                      <span style={{ 
                        fontWeight: '600', 
                        minWidth: '50px',
                        color: tier.canResearch ? '#38A169' : '#718096'
                      }}>
                        Tier {tier.tier}:
                      </span>
                      <div style={{ 
                        flex: 1, 
                        height: '8px', 
                        backgroundColor: '#E2E8F0', 
                        borderRadius: '4px',
                        overflow: 'hidden'
                      }}>
                        <div style={{
                          width: `${(tier.researchedCount / tier.requiredForNextTier) * 100}%`,
                          height: '100%',
                          backgroundColor: tier.canResearch ? '#38A169' : '#A0AEC0',
                          transition: 'width 0.3s'
                        }} />
                      </div>
                      <span style={{ fontSize: '12px', color: '#718096', minWidth: '40px' }}>
                        {tier.researchedCount}/{tier.requiredForNextTier}
                      </span>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

export default FractionSettingsAdmin;

