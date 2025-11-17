import {
  DialogRoot,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogBody,
} from '@chakra-ui/react';
import { Box, Text, VStack, Spinner } from '@chakra-ui/react';
import './TurnWaitingModal.css';

/**
 * Modal wyświetlający informację o oczekiwaniu na innych graczy
 */
export const TurnWaitingModal = ({ isOpen, waitingPlayers }) => {
  return (
    <DialogRoot 
      open={isOpen} 
      size="lg" 
      closeOnInteractOutside={false} 
      closeOnEscape={false}
      blockScrollOnMount={false}
      preserveScrollBarGap
    >
      <DialogContent style={{ backgroundColor: '#ffffff', color: '#000000' }}>
        <DialogHeader style={{ borderBottom: '1px solid #e2e8f0' }}>
          <DialogTitle style={{ color: '#000000' }}>Oczekiwanie na innych graczy</DialogTitle>
        </DialogHeader>
        <DialogBody style={{ paddingTop: '1.5rem', paddingBottom: '1.5rem' }}>
          <VStack gap={4} align="stretch">
            <Box textAlign="center">
              <Spinner size="xl" style={{ color: '#3182ce', marginBottom: '1rem' }} />
              <Text fontSize="lg" fontWeight="bold" style={{ color: '#000000' }}>
                Twoja tura została zakończona
              </Text>
              <Text style={{ color: '#4a5568', marginTop: '0.5rem' }}>
                Czekamy aż wszyscy gracze zakończą swoje rozkazy...
              </Text>
            </Box>

            {waitingPlayers && waitingPlayers.length > 0 && (
              <Box style={{ backgroundColor: '#f7fafc', padding: '1rem', borderRadius: '0.375rem', border: '1px solid #e2e8f0' }}>
                <Text fontWeight="bold" style={{ marginBottom: '0.5rem', color: '#000000' }}>
                  Oczekiwanie na graczy ({waitingPlayers.length}):
                </Text>
                <VStack align="stretch" gap={2}>
                  {waitingPlayers.map((player) => (
                    <Box
                      key={player.fractionId}
                      style={{
                        padding: '0.5rem',
                        backgroundColor: '#ffffff',
                        borderRadius: '0.375rem',
                        border: '1px solid #e2e8f0',
                        display: 'flex',
                        alignItems: 'center',
                        gap: '0.5rem'
                      }}
                    >
                      <Box
                        style={{
                          width: '12px',
                          height: '12px',
                          borderRadius: '9999px',
                          backgroundColor: '#ed8936'
                        }}
                        className="pulse-animation"
                      />
                      <Text style={{ color: '#000000' }}>
                        <strong>{player.playerName || player.fractionName}</strong>
                      </Text>
                    </Box>
                  ))}
                </VStack>
              </Box>
            )}
          </VStack>
        </DialogBody>
      </DialogContent>
    </DialogRoot>
  );
};
