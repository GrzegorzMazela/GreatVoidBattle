import { useEffect, useState } from 'react';
import PropTypes from 'prop-types';
import {
  DialogRoot,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogBody,
  DialogFooter,
  DialogActionTrigger,
} from '@chakra-ui/react';
import { Box, Text, VStack, HStack, Grid } from '@chakra-ui/react';
import './TurnLogsModal.css';

/**
 * Modal wyświetlający logi z przebiegu tury
 */
export const TurnLogsModal = ({ isOpen, onClose, logs, turnNumber, battleId, fractionId, authToken, maxTurn }) => {
  const [selectedTurn, setSelectedTurn] = useState(turnNumber);
  const [currentLogs, setCurrentLogs] = useState(logs);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    setSelectedTurn(turnNumber);
    setCurrentLogs(logs);
  }, [logs, turnNumber]);

  const handleTurnChange = async (newTurn) => {
    if (newTurn === selectedTurn || newTurn < 1 || newTurn > maxTurn) return;
    
    setLoading(true);
    try {
      const { getTurnLogs } = await import('../../../services/api');
      const logsData = await getTurnLogs(battleId, fractionId, newTurn, authToken);
      setCurrentLogs(logsData.logs || []);
      setSelectedTurn(newTurn);
    } catch (error) {
      console.error('Error loading turn logs:', error);
    } finally {
      setLoading(false);
    }
  };

  const groupedLogs = currentLogs?.reduce((acc, log) => {
    if (!acc[log.type]) {
      acc[log.type] = [];
    }
    acc[log.type].push(log);
    return acc;
  }, {}) || {};

  const stats = {
    moves: groupedLogs.ShipMove?.length || 0,
    laserHits: groupedLogs.LaserHit?.length || 0,
    laserMisses: groupedLogs.LaserMiss?.length || 0,
    missileHits: groupedLogs.MissileHit?.length || 0,
    missileMisses: groupedLogs.MissileMiss?.length || 0,
    damageReceived: groupedLogs.DamageReceived?.length || 0,
    shipsDestroyed: groupedLogs.ShipDestroyed?.length || 0,
  };

  return (
    <DialogRoot 
      open={isOpen} 
      onOpenChange={({ open }) => !open && onClose()}
      size="6xl"
      closeOnInteractOutside={true}
      closeOnEscape={true}
      blockScrollOnMount={false}
      preserveScrollBarGap
    >
      <DialogContent style={{ backgroundColor: '#1a1a2e', color: '#eee', maxHeight: '90vh', maxWidth: '95vw' }}>
        <DialogHeader style={{ 
          borderBottom: '2px solid #16213e', 
          paddingBottom: '1rem',
          background: 'linear-gradient(135deg, #0f3460 0%, #16213e 100%)'
        }}>
          <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', width: '100%' }}>
            <DialogTitle style={{ color: '#eee', fontSize: '1.8rem', fontWeight: 'bold', display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
              📋 Raport z Tury {selectedTurn}
            </DialogTitle>
            
            {/* Nawigacja tur */}
            <div className="turn-navigation">
              <button 
                className="turn-nav-btn"
                onClick={() => handleTurnChange(selectedTurn - 1)}
                disabled={selectedTurn <= 1 || loading}
                title="Poprzednia tura"
              >
                ◀
              </button>
              <span className="turn-indicator">Tura {selectedTurn} / {maxTurn}</span>
              <button 
                className="turn-nav-btn"
                onClick={() => handleTurnChange(selectedTurn + 1)}
                disabled={selectedTurn >= maxTurn || loading}
                title="Następna tura"
              >
                ▶
              </button>
            </div>
          </div>
        </DialogHeader>

        <DialogBody style={{ paddingTop: '1.5rem', paddingBottom: '1.5rem', overflowY: 'auto' }}>
          {loading ? (
            <Box textAlign="center" padding="3rem">
              <div className="loading-spinner"></div>
              <Text fontSize="lg" color="#aaa" marginTop="1rem">
                Ładowanie logów...
              </Text>
            </Box>
          ) : !currentLogs || currentLogs.length === 0 ? (
            <Box textAlign="center" padding="3rem">
              <Text fontSize="xl" color="#aaa">
                Brak logów z tej tury
              </Text>
            </Box>
          ) : (
            <VStack gap={4} align="stretch">
              {/* Statystyki */}
              <Box className="stats-grid">
                <div className="stat-card stat-move">
                  <div className="stat-icon">🚶</div>
                  <div className="stat-content">
                    <div className="stat-value">{stats.moves}</div>
                    <div className="stat-label">Ruchów</div>
                  </div>
                </div>
                <div className="stat-card stat-hit">
                  <div className="stat-icon">⚡</div>
                  <div className="stat-content">
                    <div className="stat-value">{stats.laserHits}/{stats.laserHits + stats.laserMisses}</div>
                    <div className="stat-label">Lasery</div>
                  </div>
                </div>
                <div className="stat-card stat-hit">
                  <div className="stat-icon">🚀</div>
                  <div className="stat-content">
                    <div className="stat-value">{stats.missileHits}/{stats.missileHits + stats.missileMisses}</div>
                    <div className="stat-label">Rakiety</div>
                  </div>
                </div>
                <div className="stat-card stat-damage">
                  <div className="stat-icon">🩸</div>
                  <div className="stat-content">
                    <div className="stat-value">{stats.damageReceived}</div>
                    <div className="stat-label">Obrażeń</div>
                  </div>
                </div>
                <div className="stat-card stat-destroyed">
                  <div className="stat-icon">💥</div>
                  <div className="stat-content">
                    <div className="stat-value">{stats.shipsDestroyed}</div>
                    <div className="stat-label">Straconych</div>
                  </div>
                </div>
              </Box>

              {/* Główna siatka logów */}
              <VStack gap={4} align="stretch">
                {/* Sekcja: Ruchy statków */}
                {groupedLogs.ShipMove && groupedLogs.ShipMove.length > 0 && (
                  <Box className="log-section">
                    <Text className="log-section-title">🚶 Ruchy Statków</Text>
                    <div className="log-table">
                      <div className="log-table-header">
                        <div className="col-ship">Statek</div>
                        <div className="col-action">Akcja</div>
                      </div>
                      {groupedLogs.ShipMove.map((log, index) => (
                        <div key={index} className="log-table-row log-move">
                          <div className="col-ship">{log.shipName}</div>
                          <div className="col-action">{log.message}</div>
                        </div>
                      ))}
                    </div>
                  </Box>
                )}

                {/* Sekcja: Ataki laserowe */}
                {(groupedLogs.LaserHit || groupedLogs.LaserMiss) && 
                 (groupedLogs.LaserHit?.length > 0 || groupedLogs.LaserMiss?.length > 0) && (
                  <Box className="log-section">
                    <Text className="log-section-title">⚡ Ataki Laserowe</Text>
                    <div className="log-table">
                      <div className="log-table-header">
                        <div className="col-icon">Status</div>
                        <div className="col-attacker">Atakujący</div>
                        <div className="col-target">Cel</div>
                        <div className="col-result">Wynik</div>
                      </div>
                      {groupedLogs.LaserHit?.map((log, index) => {
                        const parts = log.message.match(/(.+?) trafia laserem w (.+?) \((.+?)\)/);
                        return (
                          <div key={`laser-hit-${index}`} className="log-table-row log-hit">
                            <div className="col-icon">✓</div>
                            <div className="col-attacker">{parts?.[1] || log.shipName}</div>
                            <div className="col-target">{parts?.[2] || log.targetShipName}</div>
                            <div className="col-result">{parts?.[3] || 'Trafienie'}</div>
                          </div>
                        );
                      })}
                      {groupedLogs.LaserMiss?.map((log, index) => {
                        const parts = log.message.match(/(.+?) chybia laserem w (.+?) \(celność: (.+?)\)/);
                        return (
                          <div key={`laser-miss-${index}`} className="log-table-row log-miss">
                            <div className="col-icon">✗</div>
                            <div className="col-attacker">{parts?.[1] || log.shipName}</div>
                            <div className="col-target">{parts?.[2] || log.targetShipName}</div>
                            <div className="col-result">Chybienie ({parts?.[3] || 'N/A'})</div>
                          </div>
                        );
                      })}
                    </div>
                  </Box>
                )}

                {/* Sekcja: Ataki rakietowe */}
                {(groupedLogs.MissileHit || groupedLogs.MissileMiss) && 
                 (groupedLogs.MissileHit?.length > 0 || groupedLogs.MissileMiss?.length > 0) && (
                  <Box className="log-section">
                    <Text className="log-section-title">🚀 Ataki Rakietowe</Text>
                    <div className="log-table">
                      <div className="log-table-header">
                        <div className="col-icon">Status</div>
                        <div className="col-attacker">Atakujący</div>
                        <div className="col-target">Cel</div>
                        <div className="col-result">Wynik</div>
                      </div>
                      {groupedLogs.MissileHit?.map((log, index) => {
                        const parts = log.message.match(/(.+?) trafia rakietą w (.+?) \((.+?)\)/);
                        return (
                          <div key={`missile-hit-${index}`} className="log-table-row log-hit">
                            <div className="col-icon">✓</div>
                            <div className="col-attacker">{parts?.[1] || log.shipName}</div>
                            <div className="col-target">{parts?.[2] || log.targetShipName}</div>
                            <div className="col-result">{parts?.[3] || 'Trafienie'}</div>
                          </div>
                        );
                      })}
                      {groupedLogs.MissileMiss?.map((log, index) => {
                        const parts = log.message.match(/(.+?) chybia rakietą w (.+?) \(celność: (.+?)\)/);
                        return (
                          <div key={`missile-miss-${index}`} className="log-table-row log-miss">
                            <div className="col-icon">✗</div>
                            <div className="col-attacker">{parts?.[1] || log.shipName}</div>
                            <div className="col-target">{parts?.[2] || log.targetShipName}</div>
                            <div className="col-result">Chybienie ({parts?.[3] || 'N/A'})</div>
                          </div>
                        );
                      })}
                    </div>
                  </Box>
                )}

                {/* Sekcja: Otrzymane obrażenia */}
                {groupedLogs.DamageReceived && groupedLogs.DamageReceived.length > 0 && (
                  <Box className="log-section">
                    <Text className="log-section-title">🩸 Otrzymane Obrażenia</Text>
                    <div className="log-table">
                      <div className="log-table-header">
                        <div className="col-ship">Statek</div>
                        <div className="col-dmg">Obrażenia</div>
                        <div className="col-stats">Stan Statku</div>
                      </div>
                      {groupedLogs.DamageReceived.map((log, index) => {
                        // Próbuj parsować różne formaty wiadomości
                        let damage = '?';
                        
                        // Format: "otrzymał obrażenia od ... (X dmg)"
                        const damageMatch1 = log.message.match(/\((\d+)\s+dmg\)/);
                        if (damageMatch1) {
                          damage = damageMatch1[1];
                        } else {
                          // Format alternatywny: "otrzymuje X obrażeń"
                          const damageMatch2 = log.message.match(/otrzymuje (\d+) obrażeń/);
                          if (damageMatch2) {
                            damage = damageMatch2[1];
                          } else if (log.details?.damage) {
                            // Pobierz z details jeśli dostępne
                            damage = log.details.damage;
                          }
                        }
                        
                        return (
                          <div key={index} className="log-table-row log-damage">
                            <div className="col-ship">{log.shipName}</div>
                            <div className="col-dmg">-{damage} HP</div>
                            <div className="col-stats">
                              {log.details && (
                                <>
                                  <span className="stat-hp">HP: {log.details.remainingHP}</span>
                                  <span className="stat-shield">🛡️ {log.details.remainingShields}</span>
                                  <span className="stat-armor">🔰 {log.details.remainingArmor}</span>
                                </>
                              )}
                            </div>
                          </div>
                        );
                      })}
                    </div>
                  </Box>
                )}

                {/* Sekcja: Zniszczone statki */}
                {groupedLogs.ShipDestroyed && groupedLogs.ShipDestroyed.length > 0 && (
                  <Box className="log-section">
                    <Text className="log-section-title">💥 Zniszczone Statki</Text>
                    <div className="log-table">
                      <div className="log-table-header">
                        <div className="col-icon">💥</div>
                        <div className="col-ship">Statek</div>
                        <div className="col-action">Opis</div>
                      </div>
                      {groupedLogs.ShipDestroyed.map((log, index) => (
                        <div key={index} className="log-table-row log-destroyed">
                          <div className="col-icon">💥</div>
                          <div className="col-ship">{log.shipName}</div>
                          <div className="col-action">{log.message}</div>
                        </div>
                      ))}
                    </div>
                  </Box>
                )}
              </VStack>
            </VStack>
          )}
        </DialogBody>

        <DialogFooter style={{ borderTop: '2px solid #16213e', paddingTop: '1rem', background: '#0f3460' }}>
          <DialogActionTrigger asChild>
            <button 
              className="btn-close-logs"
              onClick={onClose}
            >
              Zamknij
            </button>
          </DialogActionTrigger>
        </DialogFooter>
      </DialogContent>
    </DialogRoot>
  );
};

TurnLogsModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  onClose: PropTypes.func.isRequired,
  logs: PropTypes.arrayOf(PropTypes.shape({
    type: PropTypes.string.isRequired,
    fractionId: PropTypes.string.isRequired,
    fractionName: PropTypes.string.isRequired,
    targetFractionId: PropTypes.string,
    targetFractionName: PropTypes.string,
    shipId: PropTypes.string.isRequired,
    shipName: PropTypes.string.isRequired,
    targetShipId: PropTypes.string,
    targetShipName: PropTypes.string,
    message: PropTypes.string.isRequired,
    details: PropTypes.object,
  })),
  turnNumber: PropTypes.number.isRequired,
  battleId: PropTypes.string.isRequired,
  fractionId: PropTypes.string.isRequired,
  authToken: PropTypes.string.isRequired,
  maxTurn: PropTypes.number.isRequired,
};
