import { useState, useMemo } from 'react';
import {
  Box, Heading, VStack, HStack, Text, Button, Badge, Tabs,
  Input, Textarea, NativeSelectRoot, NativeSelectField,
  Dialog, Portal, Table, IconButton, Checkbox
} from '@chakra-ui/react';
import { LuSend, LuEye, LuPencil, LuTrash2, LuPlus, LuFilter, LuGlobe, LuRocket, LuInfo } from 'react-icons/lu';
import { StarMap } from './StarMap';
import { STAR_SYSTEMS, FACTION_NAMES, SYSTEM_TYPES, EXPLORATION_STATUS } from '../data/starSystemsData';
import { useNotification } from '../../../contexts/NotificationContext';
import './AdminExplorationPanel.css';

const FRACTION_OPTIONS = [
  { id: 'hegemonia-titanum', name: 'Hegemonia Titanum', key: 'hegemonia' },
  { id: 'shimura-incorporated', name: 'Shimura Incorporated', key: 'shimura' },
  { id: 'protektorat-pogranicza', name: 'Protektorat Pogranicza', key: 'protektorat' }
];

/**
 * Panel administracyjny eksploracji - zarządzanie układami i eksploracją frakcji
 */
export const AdminExplorationPanel = () => {
  const { showSuccess, showError } = useNotification();
  
  // Stan
  const [selectedSystem, setSelectedSystem] = useState(null);
  const [filterFaction, setFilterFaction] = useState('all');
  const [activeTab, setActiveTab] = useState('map');
  
  // Dane eksploracji (mock - w prawdziwej aplikacji z API)
  const [explorationData, setExplorationData] = useState({});
  const [systemNotes, setSystemNotes] = useState({});
  const [fractionNotes, setFractionNotes] = useState({}); // Per frakcja per układ
  const [researchSlots, setResearchSlots] = useState({
    'hegemonia-titanum': 3,
    'shimura-incorporated': 2,
    'protektorat-pogranicza': 2
  });
  // Mock pending explorations - w prawdziwej aplikacji z API/bazy danych
  // Te dane będą współdzielone między panelami frakcji i admina
  const [pendingExplorations, setPendingExplorations] = useState([
    // Przykładowe dane do demonstracji
    { fractionId: 'hegemonia-titanum', fractionName: 'Hegemonia Titanum', systemId: 'boreasion', systemName: 'Boreasion', sentAt: '2025-12-09T10:30:00' },
    { fractionId: 'shimura-incorporated', fractionName: 'Shimura Incorporated', systemId: 'doranor', systemName: 'Doranor', sentAt: '2025-12-09T11:15:00' },
    { fractionId: 'protektorat-pogranicza', fractionName: 'Protektorat Pogranicza', systemId: 'korvon', systemName: 'Korvon', sentAt: '2025-12-09T09:45:00' },
  ]);

  // Dialog wysyłania informacji
  const [sendInfoDialog, setSendInfoDialog] = useState({ open: false, system: null });
  const [sendInfoForm, setSendInfoForm] = useState({
    fractionId: '',
    message: '',
    revealStatus: 'discovered'
  });

  // Dialog edycji układu
  const [editSystemDialog, setEditSystemDialog] = useState({ open: false, system: null });
  const [editSystemForm, setEditSystemForm] = useState({
    type: 'empty',
    resources: '',
    mission: '',
    adminNotes: ''
  });

  // Statystyki
  const stats = useMemo(() => {
    const total = STAR_SYSTEMS.length;
    const byFaction = {
      hegemonia: STAR_SYSTEMS.filter(s => s.faction === 'hegemonia').length,
      shimura: STAR_SYSTEMS.filter(s => s.faction === 'shimura').length,
      protektorat: STAR_SYSTEMS.filter(s => s.faction === 'protektorat').length,
      neutral: STAR_SYSTEMS.filter(s => !s.faction).length
    };
    return { total, byFaction };
  }, []);

  // Obsługa wyboru układu
  const handleSystemSelect = (system) => {
    setSelectedSystem(system);
    // Załaduj dane edycji
    const notes = systemNotes[system.id] || {};
    setEditSystemForm({
      type: notes.type || 'empty',
      resources: notes.resources || '',
      mission: notes.mission || '',
      adminNotes: notes.adminNotes || ''
    });
  };

  // Zapisz notatki układu
  const handleSaveSystemNotes = () => {
    if (!selectedSystem) return;
    
    setSystemNotes(prev => ({
      ...prev,
      [selectedSystem.id]: editSystemForm
    }));
    showSuccess('Zapisano informacje o układzie');
  };

  // Wyślij informację do frakcji
  const handleSendInfo = () => {
    if (!sendInfoForm.fractionId || !sendInfoDialog.system) {
      showError('Wybierz frakcję');
      return;
    }

    const key = `${sendInfoDialog.system.id}-${sendInfoForm.fractionId}`;
    setFractionNotes(prev => ({
      ...prev,
      [key]: {
        message: sendInfoForm.message,
        status: sendInfoForm.revealStatus,
        sentAt: new Date().toISOString()
      }
    }));

    // Aktualizuj status eksploracji
    setExplorationData(prev => ({
      ...prev,
      [`${sendInfoForm.fractionId}-${sendInfoDialog.system.id}`]: {
        status: sendInfoForm.revealStatus,
        updatedAt: new Date().toISOString()
      }
    }));

    // Usuń z pending explorations jeśli była tam eksploracja tego układu przez tę frakcję
    setPendingExplorations(prev => 
      prev.filter(e => !(e.systemId === sendInfoDialog.system.id && e.fractionId === sendInfoForm.fractionId))
    );

    showSuccess(`Wysłano informację o ${sendInfoDialog.system.name} do frakcji`);
    setSendInfoDialog({ open: false, system: null });
    setSendInfoForm({ fractionId: '', message: '', revealStatus: 'discovered' });
  };

  // Aktualizuj sloty badawcze
  const handleUpdateSlots = (fractionId, slots) => {
    setResearchSlots(prev => ({
      ...prev,
      [fractionId]: parseInt(slots) || 0
    }));
  };

  return (
    <Box className="admin-exploration-panel">
      <VStack gap="6" align="stretch">
        {/* Nagłówek */}
        <HStack justify="space-between" flexWrap="wrap" gap="4">
          <Box>
            <Heading size="lg">🌌 Eksploracja Galaktyki</Heading>
            <Text color="gray.500">Zarządzaj odkryciami i eksploracją frakcji</Text>
          </Box>
          
          <HStack gap="4">
            <Box textAlign="center" p="3" bg="purple.50" borderRadius="lg">
              <Text fontWeight="bold" fontSize="2xl" color="purple.600">{stats.total}</Text>
              <Text fontSize="xs" color="gray.500">Układów</Text>
            </Box>
            <Box textAlign="center" p="3" bg="red.50" borderRadius="lg">
              <Text fontWeight="bold" fontSize="xl" color="red.600">{stats.byFaction.hegemonia}</Text>
              <Text fontSize="xs" color="gray.500">Hegemonia</Text>
            </Box>
            <Box textAlign="center" p="3" bg="cyan.50" borderRadius="lg">
              <Text fontWeight="bold" fontSize="xl" color="cyan.600">{stats.byFaction.shimura}</Text>
              <Text fontSize="xs" color="gray.500">Shimura</Text>
            </Box>
            <Box textAlign="center" p="3" bg="green.50" borderRadius="lg">
              <Text fontWeight="bold" fontSize="xl" color="green.600">{stats.byFaction.protektorat}</Text>
              <Text fontSize="xs" color="gray.500">Protektorat</Text>
            </Box>
          </HStack>
        </HStack>

        {/* Taby */}
        <Tabs.Root value={activeTab} onValueChange={(e) => setActiveTab(e.value)}>
          <Tabs.List>
            <Tabs.Trigger value="map">
              <LuGlobe /> Mapa Galaktyki
            </Tabs.Trigger>
            <Tabs.Trigger value="slots">
              <LuRocket /> Sloty Badawcze
            </Tabs.Trigger>
            <Tabs.Trigger value="pending">
              <LuInfo /> Oczekujące ({pendingExplorations.length})
            </Tabs.Trigger>
          </Tabs.List>

          {/* Mapa */}
          <Tabs.Content value="map">
            <HStack align="start" gap="4" flexWrap="wrap">
              {/* Filtr */}
              <Box mb="4" p="4" bg="white" borderRadius="lg" shadow="sm" minW="200px">
                <Text fontWeight="bold" mb="2">Filtruj frakcję</Text>
                <NativeSelectRoot>
                  <NativeSelectField
                    value={filterFaction}
                    onChange={(e) => setFilterFaction(e.target.value)}
                  >
                    <option value="all">Wszystkie układy</option>
                    <option value="hegemonia">Hegemonia Titanum</option>
                    <option value="shimura">Shimura Incorporated</option>
                    <option value="protektorat">Protektorat Pogranicza</option>
                    <option value="neutral">Neutralne</option>
                  </NativeSelectField>
                </NativeSelectRoot>
              </Box>
            </HStack>

            <HStack align="start" gap="6" flexDirection={{ base: 'column', xl: 'row' }}>
              {/* Mapa */}
              <Box flex="2" minW="600px">
                <StarMap
                  viewMode="admin"
                  explorationData={explorationData}
                  onSystemSelect={handleSystemSelect}
                  selectedSystem={selectedSystem}
                  filterFaction={filterFaction === 'all' ? null : filterFaction}
                  highlightedSystems={pendingExplorations.map(e => e.systemId)}
                />
              </Box>

              {/* Panel szczegółów */}
              <Box flex="1" minW="350px">
                {selectedSystem ? (
                  <Box bg="white" p="6" borderRadius="lg" shadow="md">
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
                            {FACTION_NAMES[selectedSystem.faction] || selectedSystem.faction}
                          </Badge>
                        )}
                      </HStack>

                      {selectedSystem.isCapital && (
                        <Badge colorScheme="yellow" size="lg">⭐ Stolica Frakcji</Badge>
                      )}

                      <Text fontSize="sm" color="gray.500">
                        Współrzędne: ({selectedSystem.x}, {selectedSystem.y})
                      </Text>

                      {/* Aktywne eksploracje tego układu */}
                      {pendingExplorations.filter(e => e.systemId === selectedSystem.id).length > 0 && (
                        <Box p="3" bg="purple.50" borderRadius="md" borderLeft="4px solid" borderColor="purple.400">
                          <Text fontWeight="bold" color="purple.700" mb="2">
                            🚀 Aktywne eksploracje
                          </Text>
                          <VStack gap="1" align="stretch">
                            {pendingExplorations.filter(e => e.systemId === selectedSystem.id).map((exp, idx) => (
                              <HStack key={idx} justify="space-between">
                                <Badge colorScheme={
                                  exp.fractionId === 'hegemonia-titanum' ? 'red' :
                                  exp.fractionId === 'shimura-incorporated' ? 'cyan' : 'green'
                                }>
                                  {exp.fractionName}
                                </Badge>
                                <Text fontSize="xs" color="gray.500">
                                  {new Date(exp.sentAt).toLocaleDateString('pl-PL')}
                                </Text>
                              </HStack>
                            ))}
                          </VStack>
                        </Box>
                      )}

                      {/* Typ układu */}
                      <Box>
                        <Text fontWeight="bold" mb="1">Typ układu</Text>
                        <NativeSelectRoot size="sm">
                          <NativeSelectField
                            value={editSystemForm.type}
                            onChange={(e) => setEditSystemForm(prev => ({ ...prev, type: e.target.value }))}
                          >
                            <option value="empty">Pusty</option>
                            <option value="resources">Zasoby</option>
                            <option value="mission">Misja PBF</option>
                            <option value="anomaly">Anomalia</option>
                            <option value="hostile">Wrogie siły</option>
                            <option value="artifact">Artefakt</option>
                          </NativeSelectField>
                        </NativeSelectRoot>
                      </Box>

                      {/* Zasoby */}
                      {editSystemForm.type === 'resources' && (
                        <Box>
                          <Text fontWeight="bold" mb="1">Zasoby</Text>
                          <Textarea
                            size="sm"
                            value={editSystemForm.resources}
                            onChange={(e) => setEditSystemForm(prev => ({ ...prev, resources: e.target.value }))}
                            placeholder="Opisz dostępne zasoby..."
                            rows={2}
                          />
                        </Box>
                      )}

                      {/* Misja */}
                      {editSystemForm.type === 'mission' && (
                        <Box>
                          <Text fontWeight="bold" mb="1">Opis misji</Text>
                          <Textarea
                            size="sm"
                            value={editSystemForm.mission}
                            onChange={(e) => setEditSystemForm(prev => ({ ...prev, mission: e.target.value }))}
                            placeholder="Opisz misję PBF..."
                            rows={3}
                          />
                        </Box>
                      )}

                      {/* Notatki admina */}
                      <Box>
                        <Text fontWeight="bold" mb="1">Notatki (tylko dla adminów)</Text>
                        <Textarea
                          size="sm"
                          value={editSystemForm.adminNotes}
                          onChange={(e) => setEditSystemForm(prev => ({ ...prev, adminNotes: e.target.value }))}
                          placeholder="Prywatne notatki..."
                          rows={2}
                        />
                      </Box>

                      <HStack>
                        <Button colorScheme="blue" onClick={handleSaveSystemNotes} flex="1">
                          Zapisz
                        </Button>
                        <Button 
                          colorScheme="green" 
                          onClick={() => setSendInfoDialog({ open: true, system: selectedSystem })}
                          flex="1"
                        >
                          <LuSend /> Wyślij do frakcji
                        </Button>
                      </HStack>

                      {/* Status eksploracji per frakcja */}
                      <Box mt="4" pt="4" borderTop="1px solid" borderColor="gray.200">
                        <Text fontWeight="bold" mb="2">Status odkrycia per frakcja</Text>
                        <VStack gap="2" align="stretch">
                          {FRACTION_OPTIONS.map(f => {
                            const key = `${selectedSystem.id}-${f.id}`;
                            const note = fractionNotes[key];
                            return (
                              <HStack key={f.id} justify="space-between" p="2" bg="gray.50" borderRadius="md">
                                <Text fontSize="sm">{f.name}</Text>
                                {note ? (
                                  <Badge colorScheme={note.status === 'explored' ? 'green' : 'yellow'}>
                                    {note.status === 'explored' ? 'Zbadany' : 'Odkryty'}
                                  </Badge>
                                ) : (
                                  <Badge colorScheme="gray">Nieznany</Badge>
                                )}
                              </HStack>
                            );
                          })}
                        </VStack>
                      </Box>
                    </VStack>
                  </Box>
                ) : (
                  <Box bg="gray.50" p="8" borderRadius="lg" textAlign="center">
                    <LuGlobe size={48} style={{ margin: '0 auto', opacity: 0.3 }} />
                    <Text color="gray.500" mt="4">
                      Wybierz układ na mapie, aby zobaczyć szczegóły
                    </Text>
                  </Box>
                )}
              </Box>
            </HStack>
          </Tabs.Content>

          {/* Sloty badawcze */}
          <Tabs.Content value="slots">
            <Box bg="white" p="6" borderRadius="lg" shadow="sm">
              <Heading size="md" mb="4">🔬 Sloty Badawcze Frakcji</Heading>
              <Text color="gray.500" mb="6">
                Ustaw ile ekspedycji może wysłać każda frakcja na turę.
              </Text>

              <VStack gap="4" align="stretch" maxW="500px">
                {FRACTION_OPTIONS.map(f => (
                  <HStack key={f.id} justify="space-between" p="4" bg="gray.50" borderRadius="lg">
                    <Box>
                      <Text fontWeight="bold">{f.name}</Text>
                      <Text fontSize="sm" color="gray.500">
                        Aktywne sloty badawcze
                      </Text>
                    </Box>
                    <HStack>
                      <Button
                        size="sm"
                        variant="outline"
                        onClick={() => handleUpdateSlots(f.id, Math.max(0, researchSlots[f.id] - 1))}
                      >
                        -
                      </Button>
                      <Text fontWeight="bold" fontSize="xl" minW="40px" textAlign="center">
                        {researchSlots[f.id]}
                      </Text>
                      <Button
                        size="sm"
                        variant="outline"
                        onClick={() => handleUpdateSlots(f.id, researchSlots[f.id] + 1)}
                      >
                        +
                      </Button>
                    </HStack>
                  </HStack>
                ))}
              </VStack>
            </Box>
          </Tabs.Content>

          {/* Oczekujące eksploracje */}
          <Tabs.Content value="pending">
            <Box bg="white" p="6" borderRadius="lg" shadow="sm">
              <Heading size="md" mb="4">⏳ Oczekujące eksploracje</Heading>
              
              {pendingExplorations.length === 0 ? (
                <Box textAlign="center" py="8" color="gray.500">
                  <Text>Brak oczekujących eksploracji</Text>
                  <Text fontSize="sm" mt="2">
                    Frakcje mogą wysyłać ekspedycje ze swoich paneli
                  </Text>
                </Box>
              ) : (
                <Table.Root>
                  <Table.Header>
                    <Table.Row>
                      <Table.ColumnHeader>Frakcja</Table.ColumnHeader>
                      <Table.ColumnHeader>Układ</Table.ColumnHeader>
                      <Table.ColumnHeader>Data wysłania</Table.ColumnHeader>
                      <Table.ColumnHeader>Akcje</Table.ColumnHeader>
                    </Table.Row>
                  </Table.Header>
                  <Table.Body>
                    {pendingExplorations.map((exp, idx) => {
                      const system = STAR_SYSTEMS.find(s => s.id === exp.systemId);
                      return (
                        <Table.Row key={idx}>
                          <Table.Cell>
                            <Badge colorScheme={
                              exp.fractionId === 'hegemonia-titanum' ? 'red' :
                              exp.fractionId === 'shimura-incorporated' ? 'cyan' : 'green'
                            }>
                              {exp.fractionName}
                            </Badge>
                          </Table.Cell>
                          <Table.Cell fontWeight="bold">{exp.systemName}</Table.Cell>
                          <Table.Cell fontSize="sm" color="gray.500">
                            {new Date(exp.sentAt).toLocaleString('pl-PL')}
                          </Table.Cell>
                          <Table.Cell>
                            <HStack gap="2">
                              <Button 
                                size="sm" 
                                colorScheme="green"
                                onClick={() => {
                                  if (system) {
                                    setSendInfoDialog({ open: true, system });
                                    setSendInfoForm(prev => ({ ...prev, fractionId: exp.fractionId }));
                                  }
                                }}
                              >
                                <LuSend /> Odpowiedz
                              </Button>
                              <Button 
                                size="sm" 
                                variant="outline"
                                colorScheme="red"
                                onClick={() => {
                                  setPendingExplorations(prev => prev.filter((_, i) => i !== idx));
                                  showSuccess('Usunięto eksplorację');
                                }}
                              >
                                <LuTrash2 />
                              </Button>
                            </HStack>
                          </Table.Cell>
                        </Table.Row>
                      );
                    })}
                  </Table.Body>
                </Table.Root>
              )}
            </Box>
          </Tabs.Content>
        </Tabs.Root>
      </VStack>

      {/* Dialog wysyłania informacji */}
      <Dialog.Root open={sendInfoDialog.open} onOpenChange={(e) => !e.open && setSendInfoDialog({ open: false, system: null })}>
        <Portal>
          <Dialog.Backdrop />
          <Dialog.Positioner>
            <Dialog.Content maxW="500px">
              <Dialog.Header>
                <Dialog.Title>
                  📨 Wyślij informację o {sendInfoDialog.system?.name}
                </Dialog.Title>
              </Dialog.Header>
              <Dialog.Body>
                <VStack gap="4" align="stretch">
                  <Box>
                    <Text fontWeight="bold" mb="1">Wybierz frakcję</Text>
                    <NativeSelectRoot>
                      <NativeSelectField
                        value={sendInfoForm.fractionId}
                        onChange={(e) => setSendInfoForm(prev => ({ ...prev, fractionId: e.target.value }))}
                      >
                        <option value="">-- Wybierz --</option>
                        {FRACTION_OPTIONS.map(f => (
                          <option key={f.id} value={f.id}>{f.name}</option>
                        ))}
                      </NativeSelectField>
                    </NativeSelectRoot>
                  </Box>

                  <Box>
                    <Text fontWeight="bold" mb="1">Poziom odkrycia</Text>
                    <NativeSelectRoot>
                      <NativeSelectField
                        value={sendInfoForm.revealStatus}
                        onChange={(e) => setSendInfoForm(prev => ({ ...prev, revealStatus: e.target.value }))}
                      >
                        <option value="discovered">Odkryty (tylko nazwa)</option>
                        <option value="explored">Zbadany (pełne informacje)</option>
                      </NativeSelectField>
                    </NativeSelectRoot>
                  </Box>

                  <Box>
                    <Text fontWeight="bold" mb="1">Wiadomość dla frakcji</Text>
                    <Textarea
                      value={sendInfoForm.message}
                      onChange={(e) => setSendInfoForm(prev => ({ ...prev, message: e.target.value }))}
                      placeholder="Co frakcja dowiedziała się o tym układzie..."
                      rows={4}
                    />
                  </Box>
                </VStack>
              </Dialog.Body>
              <Dialog.Footer>
                <HStack>
                  <Button variant="outline" onClick={() => setSendInfoDialog({ open: false, system: null })}>
                    Anuluj
                  </Button>
                  <Button colorScheme="green" onClick={handleSendInfo}>
                    <LuSend /> Wyślij
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

export default AdminExplorationPanel;

