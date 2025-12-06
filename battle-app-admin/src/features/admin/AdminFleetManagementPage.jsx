import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { 
  Box, Heading, VStack, HStack, Button, Text, Spinner, 
  Table, Badge, IconButton, createToaster, Toaster,
  Dialog, Portal, Tabs, NativeSelectRoot, NativeSelectField
} from '@chakra-ui/react';
import { LuPlus, LuPencil, LuTrash2, LuRocket, LuGlobe } from 'react-icons/lu';
import { 
  getFractionFleet, 
  deleteFleetShip, 
  deletePlanetarySystem
} from '../../services/api';
import FleetShipForm from './FleetShipForm';
import PlanetarySystemForm from './PlanetarySystemForm';

const toaster = createToaster({
  placement: 'top-end',
  duration: 3000,
});

const FRACTION_OPTIONS = [
  { id: 'hegemonia-titanum', name: 'Hegemonia Titanum' },
  { id: 'shimura-incorporated', name: 'Shimura Incorporated' },
  { id: 'protektorat-pogranicza', name: 'Protektorat Pogranicza' }
];

export default function AdminFleetManagementPage() {
  const queryClient = useQueryClient();
  const [selectedFraction, setSelectedFraction] = useState(FRACTION_OPTIONS[0].id);
  
  const [shipFormOpen, setShipFormOpen] = useState(false);
  const [systemFormOpen, setSystemFormOpen] = useState(false);
  const [editingShip, setEditingShip] = useState(null);
  const [editingSystem, setEditingSystem] = useState(null);

  const { data: fleetData, isLoading, error } = useQuery({
    queryKey: ['fleet', selectedFraction],
    queryFn: () => getFractionFleet(selectedFraction),
    enabled: !!selectedFraction
  });

  const deleteShipMutation = useMutation({
    mutationFn: (shipId) => deleteFleetShip(selectedFraction, shipId),
    onSuccess: () => {
      queryClient.invalidateQueries(['fleet', selectedFraction]);
      toaster.success({ title: 'Statek usunięty' });
    },
    onError: () => {
      toaster.error({ title: 'Błąd podczas usuwania statku' });
    }
  });

  const deleteSystemMutation = useMutation({
    mutationFn: (systemId) => deletePlanetarySystem(selectedFraction, systemId),
    onSuccess: () => {
      queryClient.invalidateQueries(['fleet', selectedFraction]);
      toaster.success({ title: 'Układ planetarny usunięty' });
    },
    onError: () => {
      toaster.error({ title: 'Błąd podczas usuwania układu' });
    }
  });

  const handleEditShip = (ship) => {
    setEditingShip(ship);
    setShipFormOpen(true);
  };

  const handleEditSystem = (system) => {
    setEditingSystem(system);
    setSystemFormOpen(true);
  };

  const handleCloseShipForm = () => {
    setShipFormOpen(false);
    setEditingShip(null);
  };

  const handleCloseSystemForm = () => {
    setSystemFormOpen(false);
    setEditingSystem(null);
  };

  const { planetarySystems = [], fleet = [] } = fleetData || {};

  // Sprawdź czy są układy planetarne - wymagane do tworzenia statków
  const canCreateShip = planetarySystems.length > 0;

  return (
    <>
      <Toaster toaster={toaster} />
      <Box>
        <Heading size="lg" mb="4">⚙️ Zarządzanie Flotami (Admin)</Heading>
        
        {/* Fraction Selector */}
        <Box mb="6" p="4" bg="white" rounded="lg" shadow="sm">
          <HStack>
            <Text fontWeight="bold">Wybierz frakcję:</Text>
            <NativeSelectRoot maxW="300px">
              <NativeSelectField 
                value={selectedFraction}
                onChange={(e) => setSelectedFraction(e.target.value)}
              >
                {FRACTION_OPTIONS.map(f => (
                  <option key={f.id} value={f.id}>{f.name}</option>
                ))}
              </NativeSelectField>
            </NativeSelectRoot>
          </HStack>
        </Box>

        {isLoading && <Spinner size="xl" />}
        {error && <Text color="red.500">Błąd ładowania danych floty</Text>}
        
        {!isLoading && !error && (
          <Tabs.Root defaultValue="systems" variant="enclosed">
            <Tabs.List mb="4">
              <Tabs.Trigger value="systems">
                <LuGlobe /> Układy Planetarne ({planetarySystems.length})
              </Tabs.Trigger>
              <Tabs.Trigger value="ships">
                <LuRocket /> Flota ({fleet.length})
              </Tabs.Trigger>
            </Tabs.List>

            {/* Planetary Systems Tab */}
            <Tabs.Content value="systems">
              <Box bg="white" p="6" rounded="lg" shadow="sm">
                <HStack justify="space-between" mb="4">
                  <Heading size="md">Układy Planetarne</Heading>
                  <Button 
                    colorPalette="blue" 
                    size="sm" 
                    onClick={() => setSystemFormOpen(true)}
                  >
                    <LuPlus /> Dodaj Układ
                  </Button>
                </HStack>
                
                {planetarySystems.length === 0 ? (
                  <Text color="gray.500">Brak układów planetarnych. Dodaj pierwszy układ, aby móc tworzyć statki.</Text>
                ) : (
                  <VStack gap="4" align="stretch">
                    {planetarySystems.map(system => (
                      <Box key={system.id} p="4" borderWidth="1px" borderRadius="md" bg="gray.50">
                        <HStack justify="space-between" mb="2">
                          <VStack align="start" gap="1">
                            <Heading size="sm">{system.name}</Heading>
                            <Text fontSize="sm" color="gray.600">{system.description}</Text>
                          </VStack>
                          <HStack>
                            <IconButton 
                              size="sm" 
                              variant="ghost" 
                              onClick={() => handleEditSystem(system)}
                            >
                              <LuPencil />
                            </IconButton>
                            <IconButton 
                              size="sm" 
                              variant="ghost" 
                              colorPalette="red"
                              onClick={() => deleteSystemMutation.mutate(system.id)}
                              disabled={system.stationedShips?.length > 0}
                              title={system.stationedShips?.length > 0 ? "Nie można usunąć - są przypisane statki" : "Usuń układ"}
                            >
                              <LuTrash2 />
                            </IconButton>
                          </HStack>
                        </HStack>
                        
                        {system.stationedShips?.length > 0 && (
                          <Box mt="3">
                            <Text fontSize="sm" fontWeight="bold" mb="2">
                              Stacjonujące statki ({system.stationedShips.length}):
                            </Text>
                            <HStack gap="2" flexWrap="wrap">
                              {system.stationedShips.map(ship => (
                                <Badge key={ship.id} colorPalette="blue">
                                  {ship.name} ({ship.type})
                                </Badge>
                              ))}
                            </HStack>
                          </Box>
                        )}
                      </Box>
                    ))}
                  </VStack>
                )}
              </Box>
            </Tabs.Content>

            {/* Fleet Ships Tab */}
            <Tabs.Content value="ships">
              <Box bg="white" p="6" rounded="lg" shadow="sm">
                <HStack justify="space-between" mb="4">
                  <Heading size="md">Flota</Heading>
                  <Button 
                    colorPalette="green" 
                    size="sm" 
                    onClick={() => setShipFormOpen(true)}
                    disabled={!canCreateShip}
                    title={!canCreateShip ? "Najpierw dodaj układ planetarny" : "Dodaj statek"}
                  >
                    <LuPlus /> Dodaj Statek
                  </Button>
                </HStack>

                {!canCreateShip && (
                  <Box mb="4" p="3" bg="orange.50" borderRadius="md" borderWidth="1px" borderColor="orange.200">
                    <Text color="orange.700">
                      ⚠️ Aby dodać statek, najpierw utwórz co najmniej jeden układ planetarny.
                    </Text>
                  </Box>
                )}

                {fleet.length === 0 ? (
                  <Text color="gray.500">Brak statków we flocie.</Text>
                ) : (
                  <Table.Root size="sm">
                    <Table.Header>
                      <Table.Row>
                        <Table.ColumnHeader>Nazwa</Table.ColumnHeader>
                        <Table.ColumnHeader>Typ</Table.ColumnHeader>
                        <Table.ColumnHeader>Kategoria</Table.ColumnHeader>
                        <Table.ColumnHeader>Stacjonowanie</Table.ColumnHeader>
                        <Table.ColumnHeader>HP</Table.ColumnHeader>
                        <Table.ColumnHeader>Tarcze</Table.ColumnHeader>
                        <Table.ColumnHeader>Pancerz</Table.ColumnHeader>
                        <Table.ColumnHeader>Prędkość</Table.ColumnHeader>
                        <Table.ColumnHeader>Akcje</Table.ColumnHeader>
                      </Table.Row>
                    </Table.Header>
                    <Table.Body>
                      {fleet.map(ship => (
                        <Table.Row key={ship.id}>
                          <Table.Cell fontWeight="bold">{ship.name}</Table.Cell>
                          <Table.Cell>
                            <Badge colorPalette="purple">{ship.type}</Badge>
                          </Table.Cell>
                          <Table.Cell>
                            <Badge colorPalette={
                              ship.category === 'Combat' ? 'red' : 
                              ship.category === 'Research' ? 'blue' : 'gray'
                            }>
                              {ship.category}
                            </Badge>
                          </Table.Cell>
                          <Table.Cell>
                            {ship.stationedInSystemName ? (
                              <Badge colorPalette="green">{ship.stationedInSystemName}</Badge>
                            ) : (
                              <Badge colorPalette="orange">Nieprzypisany</Badge>
                            )}
                          </Table.Cell>
                          <Table.Cell>{ship.hitPoints}</Table.Cell>
                          <Table.Cell>{ship.shields}</Table.Cell>
                          <Table.Cell>{ship.armor}</Table.Cell>
                          <Table.Cell>{ship.speed}</Table.Cell>
                          <Table.Cell>
                            <HStack gap="1">
                              <IconButton 
                                size="xs" 
                                variant="ghost"
                                onClick={() => handleEditShip(ship)}
                              >
                                <LuPencil />
                              </IconButton>
                              <IconButton 
                                size="xs" 
                                variant="ghost" 
                                colorPalette="red"
                                onClick={() => deleteShipMutation.mutate(ship.id)}
                              >
                                <LuTrash2 />
                              </IconButton>
                            </HStack>
                          </Table.Cell>
                        </Table.Row>
                      ))}
                    </Table.Body>
                  </Table.Root>
                )}
              </Box>
            </Tabs.Content>
          </Tabs.Root>
        )}
      </Box>

      {/* Ship Form Dialog */}
      <Dialog.Root open={shipFormOpen} onOpenChange={(e) => !e.open && handleCloseShipForm()}>
        <Portal>
          <Dialog.Backdrop />
          <Dialog.Positioner>
            <Dialog.Content maxW="600px">
              <Dialog.Header>
                <Dialog.Title>
                  {editingShip ? 'Edytuj Statek' : 'Dodaj Statek do Floty'}
                </Dialog.Title>
              </Dialog.Header>
              <Dialog.Body>
                <FleetShipForm 
                  fractionId={selectedFraction}
                  ship={editingShip}
                  planetarySystems={planetarySystems}
                  onClose={handleCloseShipForm}
                  requireSystem={true}
                  toaster={toaster}
                />
              </Dialog.Body>
            </Dialog.Content>
          </Dialog.Positioner>
        </Portal>
      </Dialog.Root>

      {/* System Form Dialog */}
      <Dialog.Root open={systemFormOpen} onOpenChange={(e) => !e.open && handleCloseSystemForm()}>
        <Portal>
          <Dialog.Backdrop />
          <Dialog.Positioner>
            <Dialog.Content maxW="500px">
              <Dialog.Header>
                <Dialog.Title>
                  {editingSystem ? 'Edytuj Układ Planetarny' : 'Dodaj Układ Planetarny'}
                </Dialog.Title>
              </Dialog.Header>
              <Dialog.Body>
                <PlanetarySystemForm 
                  fractionId={selectedFraction}
                  system={editingSystem}
                  onClose={handleCloseSystemForm}
                  toaster={toaster}
                />
              </Dialog.Body>
            </Dialog.Content>
          </Dialog.Positioner>
        </Portal>
      </Dialog.Root>
    </>
  );
}

