import { useState, useMemo } from 'react';
import {
  Box, Heading, VStack, HStack, Text, Button, Badge, Tabs,
  Input, Dialog, Portal, Table, IconButton
} from '@chakra-ui/react';
import { LuRocket, LuGlobe, LuInfo, LuSearch, LuStar, LuMapPin } from 'react-icons/lu';
import { StarMap } from './StarMap';
import { STAR_SYSTEMS, EXPLORATION_STATUS, getKnownSystemsForFaction } from '../data/starSystemsData';
import { useNotification } from '../../../contexts/NotificationContext';
import { getDiscordSession } from '../../../services/discordAuthApi';
import './FractionExplorationPanel.css';

const FRACTION_IDS = {
  'Hegemonia Titanum': { id: 'hegemonia-titanum', key: 'hegemonia' },
  'Shimura Incorporated': { id: 'shimura-incorporated', key: 'shimura' },
  'Protektorat Pogranicza': { id: 'protektorat-pogranicza', key: 'protektorat' }
};

/**
 * Panel eksploracji dla frakcji - widok gracza
 */
export const FractionExplorationPanel = () => {
  const { showSuccess, showError, showInfo } = useNotification();
  const session = getDiscordSession();
  const fractionRoles = session?.user?.fractionRoles || [];
  
  // Określ frakcję użytkownika
  const matchingRole = fractionRoles.find(r => FRACTION_IDS[r]);
  const fractionData = matchingRole ? FRACTION_IDS[matchingRole] : null;
  const fractionId = fractionData?.id;
  const fractionKey = fractionData?.key;

  // Stan
  const [selectedSystem, setSelectedSystem] = useState(null);
  const [activeTab, setActiveTab] = useState('map');
  const [showExploreDialog, setShowExploreDialog] = useState(false);
  
  // Mock danych - w prawdziwej aplikacji z API
  const [explorationData, setExplorationData] = useState({});
  const [researchSlots, setResearchSlots] = useState(2);
  const [usedSlots, setUsedSlots] = useState(0);
  const [pendingExplorations, setPendingExplorations] = useState([]);
  const [explorationHistory, setExplorationHistory] = useState([]);

  // Znane układy dla frakcji
  const knownSystems = useMemo(() => {
    if (!fractionKey) return [];
    
    return STAR_SYSTEMS.filter(system => {
      // Układy znane wszystkim
      if (system.knownToAll) return true;
      // Układy własnej frakcji
      if (system.faction === fractionKey) return true;
      // Układy specjalnie odkryte dla frakcji
      if (system.knownTo && system.knownTo.includes(fractionKey)) return true;
      // Układy odkryte przez eksplorację
      const status = explorationData[system.id]?.status;
      return status === EXPLORATION_STATUS.DISCOVERED || status === EXPLORATION_STATUS.EXPLORED;
    });
  }, [fractionKey, explorationData]);

  // Układy własnej frakcji
  const ownSystems = useMemo(() => {
    if (!fractionKey) return [];
    return STAR_SYSTEMS.filter(s => s.faction === fractionKey);
  }, [fractionKey]);

  // Układy do zbadania (nieznane ale nie wrogie)
  const unexploredSystems = useMemo(() => {
    if (!fractionKey) return [];
    return STAR_SYSTEMS.filter(s => {
      if (s.knownToAll) return false;
      if (s.faction === fractionKey) return false;
      if (s.knownTo && s.knownTo.includes(fractionKey)) return false;
      const status = explorationData[s.id]?.status;
      return !status;
    });
  }, [fractionKey, explorationData]);

  // Sprawdź czy układ jest znany
  const isSystemKnown = (system) => {
    if (!system || !fractionKey) return false;
    if (system.knownToAll) return true;
    if (system.faction === fractionKey) return true;
    if (system.knownTo && system.knownTo.includes(fractionKey)) return true;
    const status = explorationData[system.id]?.status;
    return status === EXPLORATION_STATUS.DISCOVERED || status === EXPLORATION_STATUS.EXPLORED;
  };

  // Sprawdź czy układ jest w trakcie eksploracji
  const isSystemPending = (system) => {
    if (!system) return false;
    return pendingExplorations.some(e => e.systemId === system.id);
  };

  // Obsługa wyboru układu
  const handleSystemSelect = (system) => {
    setSelectedSystem(system);
  };

  // Wyślij ekspedycję
  const handleSendExpedition = () => {
    if (!selectedSystem) {
      showError('Wybierz układ do eksploracji');
      return;
    }
    
    if (usedSlots >= researchSlots) {
      showError('Wykorzystałeś wszystkie sloty badawcze na tę turę');
      return;
    }

    // Dodaj do oczekujących
    setPendingExplorations(prev => [...prev, {
      systemId: selectedSystem.id,
      systemName: selectedSystem.name,
      sentAt: new Date().toISOString()
    }]);

    setUsedSlots(prev => prev + 1);
    setShowExploreDialog(false);
    showSuccess(`Wysłano ekspedycję do układu ${selectedSystem.name}`);
  };

  // Anuluj ekspedycję
  const handleCancelExpedition = (systemId) => {
    setPendingExplorations(prev => prev.filter(e => e.systemId !== systemId));
    setUsedSlots(prev => Math.max(0, prev - 1));
    showInfo('Anulowano ekspedycję');
  };

  // Jeśli użytkownik nie ma frakcji
  if (!fractionId) {
    return (
      <Box p="8" textAlign="center">
        <LuGlobe size={64} style={{ margin: '0 auto', opacity: 0.3 }} />
        <Heading size="md" mt="4" color="gray.500">
          Brak przypisanej frakcji
        </Heading>
        <Text color="gray.400" mt="2">
          Skontaktuj się z administratorem, aby uzyskać dostęp do eksploracji.
        </Text>
      </Box>
    );
  }

  return (
    <Box className="fraction-exploration-panel">
      <VStack gap="6" align="stretch">
        {/* Nagłówek */}
        <Box className="exploration-header" p="6" borderRadius="xl">
          <HStack justify="space-between" flexWrap="wrap" gap="4">
            <Box>
              <Heading size="lg" color="white">🌌 Eksploracja Kosmosu</Heading>
              <Text color="whiteAlpha.800">
                Odkrywaj nowe układy i zdobywaj zasoby dla swojej frakcji
              </Text>
            </Box>
            
            <HStack gap="4">
              <Box className="slot-indicator" p="4" borderRadius="lg">
                <HStack gap="3">
                  <LuRocket size={24} />
                  <Box>
                    <Text fontWeight="bold" fontSize="lg">
                      {researchSlots - usedSlots} / {researchSlots}
                    </Text>
                    <Text fontSize="xs" opacity="0.8">Sloty badawcze</Text>
                  </Box>
                </HStack>
                <Box 
                  mt="2" 
                  h="6px" 
                  bg="whiteAlpha.300" 
                  borderRadius="full" 
                  overflow="hidden"
                >
                  <Box 
                    h="100%" 
                    w={`${(usedSlots / researchSlots) * 100}%`}
                    bg="blue.400" 
                    borderRadius="full" 
                    transition="width 0.3s"
                  />
                </Box>
              </Box>
              
              <Box className="systems-indicator" p="4" borderRadius="lg">
                <Text fontWeight="bold" fontSize="2xl">{knownSystems.length}</Text>
                <Text fontSize="xs" opacity="0.8">Znanych układów</Text>
              </Box>
            </HStack>
          </HStack>
        </Box>

        {/* Taby */}
        <Tabs.Root value={activeTab} onValueChange={(e) => setActiveTab(e.value)}>
          <Tabs.List>
            <Tabs.Trigger value="map">
              <LuGlobe /> Mapa Galaktyki
            </Tabs.Trigger>
            <Tabs.Trigger value="own">
              <LuStar /> Twoje układy ({ownSystems.length})
            </Tabs.Trigger>
            <Tabs.Trigger value="pending">
              <LuRocket /> Ekspedycje ({pendingExplorations.length})
            </Tabs.Trigger>
            <Tabs.Trigger value="history">
              <LuInfo /> Historia odkryć
            </Tabs.Trigger>
          </Tabs.List>

          {/* Mapa */}
          <Tabs.Content value="map">
            <HStack align="start" gap="6" flexDirection={{ base: 'column', xl: 'row' }}>
              {/* Mapa */}
              <Box flex="2" minW="600px">
                <StarMap
                  viewMode="fraction"
                  fractionId={fractionKey}
                  explorationData={explorationData}
                  onSystemSelect={handleSystemSelect}
                  selectedSystem={selectedSystem}
                  highlightedSystems={pendingExplorations.map(e => e.systemId)}
                />
                
                {/* Legenda */}
                <HStack mt="4" gap="4" flexWrap="wrap" justify="center">
                  <HStack gap="2">
                    <Box w="12px" h="12px" borderRadius="full" bg="#4ade80" />
                    <Text fontSize="sm" color="gray.600">Zbadany</Text>
                  </HStack>
                  <HStack gap="2">
                    <Box w="12px" h="12px" borderRadius="full" bg="#fbbf24" />
                    <Text fontSize="sm" color="gray.600">Odkryty</Text>
                  </HStack>
                  <HStack gap="2">
                    <Box w="12px" h="12px" borderRadius="full" bg="#888" opacity="0.5" />
                    <Text fontSize="sm" color="gray.600">Nieznany</Text>
                  </HStack>
                </HStack>
              </Box>

              {/* Panel szczegółów */}
              <Box flex="1" minW="350px">
                {selectedSystem ? (
                  <Box className="system-details" p="6" borderRadius="xl">
                    <VStack align="stretch" gap="4">
                      <HStack justify="space-between">
                        <Heading size="md">{selectedSystem.name}</Heading>
                        {selectedSystem.faction && (
                          <Badge 
                            colorScheme={
                              selectedSystem.faction === 'hegemonia' ? 'red' :
                              selectedSystem.faction === 'shimura' ? 'cyan' :
                              selectedSystem.faction === 'protektorat' ? 'green' : 'gray'
                            }
                          >
                            {selectedSystem.faction === 'hegemonia' ? 'Hegemonia' :
                             selectedSystem.faction === 'shimura' ? 'Shimura' :
                             selectedSystem.faction === 'protektorat' ? 'Protektorat' :
                             selectedSystem.faction}
                          </Badge>
                        )}
                      </HStack>

                      {selectedSystem.isCapital && (
                        <Badge colorScheme="yellow" size="lg">⭐ Stolica</Badge>
                      )}

                      {/* Status eksploracji */}
                      {isSystemPending(selectedSystem) ? (
                        <Box p="3" bg="purple.50" borderRadius="md" borderLeft="4px solid" borderColor="purple.400">
                          <HStack justify="space-between" align="start">
                            <Box>
                              <Text fontWeight="bold" color="purple.700">
                                🚀 Ekspedycja w drodze
                              </Text>
                              <Text fontSize="sm" color="gray.500" mt="1">
                                Oczekuje na wyniki od administracji
                              </Text>
                            </Box>
                            <Button 
                              size="sm" 
                              variant="outline" 
                              colorScheme="red"
                              onClick={() => handleCancelExpedition(selectedSystem.id)}
                            >
                              Anuluj
                            </Button>
                          </HStack>
                        </Box>
                      ) : explorationData[selectedSystem.id] ? (
                        <Box p="3" bg="green.50" borderRadius="md" borderLeft="4px solid" borderColor="green.400">
                          <Text fontWeight="bold" color="green.700">
                            {explorationData[selectedSystem.id].status === 'explored' ? '✓ Zbadany' : '👁 Odkryty'}
                          </Text>
                          {explorationData[selectedSystem.id].message && (
                            <Text fontSize="sm" color="gray.600" mt="2">
                              {explorationData[selectedSystem.id].message}
                            </Text>
                          )}
                        </Box>
                      ) : selectedSystem.faction === fractionKey ? (
                        <Box p="3" bg="blue.50" borderRadius="md" borderLeft="4px solid" borderColor="blue.400">
                          <Text fontWeight="bold" color="blue.700">
                            🏠 Twój układ
                          </Text>
                        </Box>
                      ) : isSystemKnown(selectedSystem) ? (
                        <Box p="3" bg="cyan.50" borderRadius="md" borderLeft="4px solid" borderColor="cyan.400">
                          <Text fontWeight="bold" color="cyan.700">
                            👁 Znany układ
                          </Text>
                          {selectedSystem.faction && (
                            <Text fontSize="sm" color="gray.500" mt="1">
                              Należy do: {selectedSystem.faction === 'hegemonia' ? 'Hegemonia' :
                                         selectedSystem.faction === 'shimura' ? 'Shimura' :
                                         selectedSystem.faction === 'protektorat' ? 'Protektorat' :
                                         selectedSystem.faction}
                            </Text>
                          )}
                        </Box>
                      ) : (
                        <Box p="3" bg="gray.50" borderRadius="md" borderLeft="4px solid" borderColor="gray.300">
                          <Text fontWeight="bold" color="gray.600">
                            ❓ Niezbadany układ
                          </Text>
                          <Text fontSize="sm" color="gray.500" mt="1">
                            Wyślij ekspedycję, aby dowiedzieć się więcej
                          </Text>
                        </Box>
                      )}

                      {/* Opis (jeśli zbadany) */}
                      {selectedSystem.description && isSystemKnown(selectedSystem) && (
                        <Box>
                          <Text fontWeight="bold" mb="1">Informacje</Text>
                          <Text fontSize="sm" color="gray.600">
                            {selectedSystem.description}
                          </Text>
                        </Box>
                      )}

                      {/* Przycisk eksploracji - dla niezbadanych układów które nie są w pending */}
                      {!isSystemKnown(selectedSystem) && !isSystemPending(selectedSystem) && selectedSystem.faction !== fractionKey && (
                        <Button
                          colorScheme="purple"
                          onClick={() => setShowExploreDialog(true)}
                          disabled={usedSlots >= researchSlots}
                        >
                          <LuRocket /> Wyślij ekspedycję
                        </Button>
                      )}

                      {usedSlots >= researchSlots && !isSystemKnown(selectedSystem) && !isSystemPending(selectedSystem) && (
                        <Text fontSize="sm" color="orange.500" textAlign="center">
                          ⚠️ Wykorzystano wszystkie sloty badawcze
                        </Text>
                      )}
                    </VStack>
                  </Box>
                ) : (
                  <Box className="system-details empty" p="8" borderRadius="xl" textAlign="center">
                    <LuMapPin size={48} style={{ margin: '0 auto', opacity: 0.3 }} />
                    <Text color="gray.500" mt="4">
                      Wybierz układ na mapie, aby zobaczyć szczegóły
                    </Text>
                  </Box>
                )}
              </Box>
            </HStack>
          </Tabs.Content>

          {/* Twoje układy */}
          <Tabs.Content value="own">
            <Box className="systems-grid">
              {ownSystems.length === 0 ? (
                <Box textAlign="center" py="8" color="gray.500">
                  <Text>Brak układów pod kontrolą frakcji</Text>
                </Box>
              ) : (
                <VStack gap="3" align="stretch">
                  {ownSystems.map(system => (
                    <Box 
                      key={system.id}
                      p="4"
                      bg="white"
                      borderRadius="lg"
                      shadow="sm"
                      cursor="pointer"
                      onClick={() => {
                        setSelectedSystem(system);
                        setActiveTab('map');
                      }}
                      _hover={{ shadow: 'md', transform: 'translateX(4px)' }}
                      transition="all 0.2s"
                    >
                      <HStack justify="space-between">
                        <Box>
                          <Text fontWeight="bold">{system.name}</Text>
                          <Text fontSize="sm" color="gray.500">
                            Współrzędne: ({system.x}, {system.y})
                          </Text>
                        </Box>
                        {system.isCapital && (
                          <Badge colorScheme="yellow">⭐ Stolica</Badge>
                        )}
                      </HStack>
                    </Box>
                  ))}
                </VStack>
              )}
            </Box>
          </Tabs.Content>

          {/* Oczekujące ekspedycje */}
          <Tabs.Content value="pending">
            <Box bg="white" p="6" borderRadius="lg" shadow="sm">
              <Heading size="md" mb="4">🚀 Wysłane ekspedycje</Heading>
              
              {pendingExplorations.length === 0 ? (
                <Box textAlign="center" py="8" color="gray.500">
                  <LuRocket size={48} style={{ margin: '0 auto', opacity: 0.3 }} />
                  <Text mt="4">Brak wysłanych ekspedycji</Text>
                  <Text fontSize="sm" mt="2">
                    Wybierz układ na mapie i wyślij ekspedycję badawczą
                  </Text>
                </Box>
              ) : (
                <VStack gap="3" align="stretch">
                  {pendingExplorations.map((exp, idx) => (
                    <Box 
                      key={idx}
                      p="4"
                      bg="purple.50"
                      borderRadius="lg"
                      borderLeft="4px solid"
                      borderColor="purple.400"
                    >
                      <HStack justify="space-between">
                        <Box>
                          <Text fontWeight="bold">{exp.systemName}</Text>
                          <Text fontSize="sm" color="gray.500">
                            Wysłano: {new Date(exp.sentAt).toLocaleString('pl-PL')}
                          </Text>
                        </Box>
                        <HStack gap="2">
                          <Badge colorScheme="purple">Oczekuje</Badge>
                          <Button 
                            size="sm" 
                            variant="outline" 
                            colorScheme="red"
                            onClick={() => handleCancelExpedition(exp.systemId)}
                          >
                            Anuluj
                          </Button>
                        </HStack>
                      </HStack>
                    </Box>
                  ))}
                </VStack>
              )}
            </Box>
          </Tabs.Content>

          {/* Historia */}
          <Tabs.Content value="history">
            <Box bg="white" p="6" borderRadius="lg" shadow="sm">
              <Heading size="md" mb="4">📜 Historia odkryć</Heading>
              
              {explorationHistory.length === 0 ? (
                <Box textAlign="center" py="8" color="gray.500">
                  <Text>Brak historii odkryć</Text>
                </Box>
              ) : (
                <Table.Root>
                  <Table.Header>
                    <Table.Row>
                      <Table.ColumnHeader>Układ</Table.ColumnHeader>
                      <Table.ColumnHeader>Status</Table.ColumnHeader>
                      <Table.ColumnHeader>Data</Table.ColumnHeader>
                      <Table.ColumnHeader>Opis</Table.ColumnHeader>
                    </Table.Row>
                  </Table.Header>
                  <Table.Body>
                    {explorationHistory.map((item, idx) => (
                      <Table.Row key={idx}>
                        <Table.Cell fontWeight="bold">{item.systemName}</Table.Cell>
                        <Table.Cell>
                          <Badge colorScheme={item.status === 'explored' ? 'green' : 'yellow'}>
                            {item.status === 'explored' ? 'Zbadany' : 'Odkryty'}
                          </Badge>
                        </Table.Cell>
                        <Table.Cell>{new Date(item.date).toLocaleDateString('pl-PL')}</Table.Cell>
                        <Table.Cell>{item.description || '-'}</Table.Cell>
                      </Table.Row>
                    ))}
                  </Table.Body>
                </Table.Root>
              )}
            </Box>
          </Tabs.Content>
        </Tabs.Root>
      </VStack>

      {/* Dialog potwierdzenia eksploracji */}
      <Dialog.Root open={showExploreDialog} onOpenChange={(e) => setShowExploreDialog(e.open)}>
        <Portal>
          <Dialog.Backdrop />
          <Dialog.Positioner>
            <Dialog.Content maxW="400px">
              <Dialog.Header>
                <Dialog.Title>🚀 Wyślij ekspedycję</Dialog.Title>
              </Dialog.Header>
              <Dialog.Body>
                <VStack gap="4" align="stretch">
                  <Text>
                    Czy na pewno chcesz wysłać ekspedycję do układu{' '}
                    <strong>{selectedSystem?.name}</strong>?
                  </Text>
                  
                  <Box p="3" bg="blue.50" borderRadius="md">
                    <Text fontSize="sm" color="blue.700">
                      Wykorzystasz 1 z {researchSlots - usedSlots} dostępnych slotów badawczych.
                    </Text>
                  </Box>
                  
                  <Text fontSize="sm" color="gray.500">
                    Wyniki eksploracji zostaną przekazane przez administrację na początku następnej tury.
                  </Text>
                </VStack>
              </Dialog.Body>
              <Dialog.Footer>
                <HStack>
                  <Button variant="outline" onClick={() => setShowExploreDialog(false)}>
                    Anuluj
                  </Button>
                  <Button colorScheme="purple" onClick={handleSendExpedition}>
                    <LuRocket /> Wyślij ekspedycję
                  </Button>
                </HStack>
              </Dialog.Footer>
            </Dialog.Content>
          </Dialog.Positioner>
        </Portal>
      </Dialog.Root>
    </Box>
  );
};

export default FractionExplorationPanel;

