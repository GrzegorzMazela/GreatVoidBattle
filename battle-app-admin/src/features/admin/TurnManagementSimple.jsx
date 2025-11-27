import { useState, useEffect } from 'react';
import { gameStateApi } from '../../services/gameStateApi';
import './TurnManagement.css';

const FRACTION_NAMES = {
  hegemonia_titanum: 'Hegemonia Titanum',
  shimura_incorporated: 'Shimura Incorporated',
  protektorat_pogranicza: 'Protektorat Pogranicza'
};

export default function TurnManagement() {
  const [pendingRequests, setPendingRequests] = useState([]);
  const [resolutions, setResolutions] = useState({});
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [successMessage, setSuccessMessage] = useState('');
  const [errorMessage, setErrorMessage] = useState('');

  const loadPendingRequests = async () => {
    try {
      setLoading(true);
      const data = await gameStateApi.getAllPendingRequests();
      setPendingRequests(data);
      
      const initialResolutions = {};
      data.forEach(fraction => {
        fraction.pendingRequests.forEach(request => {
          const key = `${fraction.fractionId}_${request.technologyId}`;
          initialResolutions[key] = {
            fractionId: fraction.fractionId,
            technologyId: request.technologyId,
            approved: true,
            comment: ''
          };
        });
      });
      setResolutions(initialResolutions);
    } catch (error) {
      console.error('Error loading pending requests:', error);
      setErrorMessage('Nie udało się załadować zgłoszeń');
      setTimeout(() => setErrorMessage(''), 3000);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadPendingRequests();
  }, []);

  const updateResolution = (fractionId, technologyId, field, value) => {
    const key = `${fractionId}_${technologyId}`;
    setResolutions(prev => ({
      ...prev,
      [key]: {
        ...prev[key],
        [field]: value
      }
    }));
  };

  const handleEndTurn = async () => {
    try {
      setSubmitting(true);
      const resolutionList = Object.values(resolutions);
      await gameStateApi.endTurn(resolutionList);
      
      setSuccessMessage('Tura zakończona - wszystkie decyzje zostały zapisane');
      setTimeout(() => setSuccessMessage(''), 5000);

      await loadPendingRequests();
    } catch (error) {
      console.error('Error ending turn:', error);
      setErrorMessage('Nie udało się zakończyć tury');
      setTimeout(() => setErrorMessage(''), 3000);
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) {
    return (
      <div style={{ padding: '24px', display: 'flex', justifyContent: 'center' }}>
        <div>Ładowanie...</div>
      </div>
    );
  }

  const totalRequests = pendingRequests.reduce((sum, f) => sum + f.pendingRequests.length, 0);

  return (
    <div style={{ padding: '24px', maxWidth: '1400px', margin: '0 auto' }}>
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

      <div style={{ marginBottom: '16px' }}>
        <h1>Zarządzanie turą</h1>
        <p style={{ color: '#718096', marginTop: '8px' }}>
          Zgłoszenia do zatwierdzenia: {totalRequests}
        </p>
      </div>

      {totalRequests === 0 ? (
        <div style={{ padding: '12px', backgroundColor: '#EBF8FF', borderRadius: '8px', color: '#2C5282' }}>
          ℹ Brak oczekujących zgłoszeń do badań
        </div>
      ) : (
        <div>
          {pendingRequests.map(fraction => (
            <div key={fraction.fractionId} style={{ marginBottom: '24px', border: '1px solid #E2E8F0', borderRadius: '8px', padding: '16px', backgroundColor: 'white' }}>
              <h2 style={{ marginBottom: '16px' }}>
                {FRACTION_NAMES[fraction.fractionId]}
                <span style={{ marginLeft: '12px', padding: '4px 8px', backgroundColor: '#3182CE', color: 'white', borderRadius: '4px', fontSize: '14px' }}>
                  {fraction.pendingRequests.length}
                </span>
              </h2>

              {fraction.pendingRequests.map(request => {
                const key = `${fraction.fractionId}_${request.technologyId}`;
                const resolution = resolutions[key];

                return (
                  <div
                    key={request.technologyId}
                    style={{
                      padding: '16px',
                      border: '1px solid #E2E8F0',
                      borderRadius: '8px',
                      marginBottom: '12px',
                      backgroundColor: resolution?.approved ? '#F0FFF4' : '#FFF5F5'
                    }}
                  >
                    <div style={{ marginBottom: '12px' }}>
                      <strong style={{ fontSize: '18px' }}>{request.technologyName}</strong>
                      <div style={{ fontSize: '14px', color: '#718096', marginTop: '4px' }}>
                        Zgłoszono: {new Date(request.requestedAt).toLocaleString('pl-PL')}
                      </div>
                    </div>

                    <div style={{ borderTop: '1px solid #E2E8F0', margin: '12px 0' }} />

                    <div style={{ marginBottom: '12px' }}>
                      <strong style={{ display: 'block', marginBottom: '8px' }}>Decyzja:</strong>
                      <div style={{ display: 'flex', gap: '16px' }}>
                        <label style={{ display: 'flex', alignItems: 'center', cursor: 'pointer' }}>
                          <input
                            type="radio"
                            name={`decision_${key}`}
                            checked={resolution?.approved === true}
                            onChange={() => updateResolution(fraction.fractionId, request.technologyId, 'approved', true)}
                            style={{ marginRight: '8px' }}
                          />
                          <span style={{ color: '#38A169' }}>Zatwierdź</span>
                        </label>
                        <label style={{ display: 'flex', alignItems: 'center', cursor: 'pointer' }}>
                          <input
                            type="radio"
                            name={`decision_${key}`}
                            checked={resolution?.approved === false}
                            onChange={() => updateResolution(fraction.fractionId, request.technologyId, 'approved', false)}
                            style={{ marginRight: '8px' }}
                          />
                          <span style={{ color: '#E53E3E' }}>Odrzuć</span>
                        </label>
                      </div>
                    </div>

                    <div>
                      <strong style={{ display: 'block', marginBottom: '8px' }}>Komentarz (opcjonalnie):</strong>
                      <textarea
                        placeholder="Dodaj komentarz dla graczy..."
                        value={resolution?.comment || ''}
                        onChange={(e) => updateResolution(fraction.fractionId, request.technologyId, 'comment', e.target.value)}
                        style={{
                          width: '100%',
                          padding: '8px',
                          borderRadius: '4px',
                          border: '1px solid #E2E8F0',
                          minHeight: '60px',
                          fontFamily: 'inherit'
                        }}
                      />
                    </div>
                  </div>
                );
              })}
            </div>
          ))}

          <div style={{ padding: '16px', backgroundColor: '#FFFAF0', borderRadius: '8px' }}>
            <div style={{ padding: '12px', marginBottom: '16px', backgroundColor: '#FED7AA', borderRadius: '8px', color: '#7C2D12' }}>
              ⚠ Po kliknięciu "Zakończ turę" wszystkie decyzje zostaną zapisane i technologie zostaną przypisane do frakcji. Ta akcja jest nieodwracalna!
            </div>

            <button
              onClick={handleEndTurn}
              disabled={submitting}
              style={{
                width: '100%',
                padding: '12px',
                fontSize: '16px',
                fontWeight: 'bold',
                borderRadius: '4px',
                border: 'none',
                backgroundColor: submitting ? '#CBD5E0' : '#3182CE',
                color: 'white',
                cursor: submitting ? 'not-allowed' : 'pointer'
              }}
            >
              {submitting ? 'Kończenie tury...' : 'Zakończ turę i zastosuj decyzje'}
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
