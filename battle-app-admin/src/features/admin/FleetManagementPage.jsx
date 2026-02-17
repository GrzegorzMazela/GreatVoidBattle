import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { 
  Box, Heading, VStack, HStack, Button, Text, Spinner, 
  Table, Badge, IconButton,
  Dialog, Portal
} from '@chakra-ui/react';
import { LuMapPin, LuRocket, LuGlobe, LuInfo } from 'react-icons/lu';
import { getDiscordSession } from '../../services/discordAuthApi';
import { 
  getFractionFleet, 
  assignShipToSystem 
} from '../../services/api';
import { useNotification } from '../../contexts/NotificationContext';
import { getShipTypeNamePl } from '../../types/dto';

const FRACTION_IDS = {
  'Hegemonia Titanum': 'hegemonia-titanum',
  'Shimura Incorporated': 'shimura-incorporated',
  'Protektorat Pogranicza': 'protektorat-pogranicza'
};

export default function FleetManagementPage() {
  const queryClient = useQueryClient();
  const { showSuccess, showError } = useNotification();
  const session = getDiscordSession();
  const fractionRoles = session?.user?.fractionRoles || [];
  
  // Determine fraction ID based on user role
  const matchingRole = fractionRoles.find(r => FRACTION_IDS[r]);
  const fractionId = matchingRole ? FRACTION_IDS[matchingRole] : null;

  const [assignDialogOpen, setAssignDialogOpen] = useState(false);
  const [assigningShip, setAssigningShip] = useState(null);
  const [systemDetailsOpen, setSystemDetailsOpen] = useState(false);
  const [selectedSystem, setSelectedSystem] = useState(null);

  const { data: fleetData, isLoading, error } = useQuery({
    queryKey: ['fleet', fractionId],
    queryFn: () => getFractionFleet(fractionId),
    enabled: !!fractionId
  });

  const assignShipMutation = useMutation({
    mutationFn: ({ shipId, systemId }) => assignShipToSystem(fractionId, shipId, systemId),
    onSuccess: () => {
      queryClient.invalidateQueries(['fleet', fractionId]);
      showSuccess('Statek przypisany do układu');
      setAssignDialogOpen(false);
      setAssigningShip(null);
    },
    onError: () => {
      showError('Błąd podczas przypisywania statku');
    }
  });

  const handleAssignShip = (ship) => {
    setAssigningShip(ship);
    setAssignDialogOpen(true);
  };

  const handleShowSystemDetails = (system) => {
    setSelectedSystem(system);
    setSystemDetailsOpen(true);
  };

  if (!fractionId) {
    return (
      <Box p="6">
        <Text color="red.500">Nie masz przypisanej frakcji. Skontaktuj się z administratorem.</Text>
      </Box>
    );
  }

  if (isLoading) return <Spinner size="xl" />;
  if (error) return <Text color="red.500">Błąd ładowania danych floty</Text>;

  const { planetarySystems = [], fleet = [], unassignedShips = [] } = fleetData || {};

  return (
    <>
      <Box>
        <Heading size="lg" mb="6">🚀 Flota Frakcji</Heading>
        
        <VStack gap="6" align="stretch">
          {/* Planetary Systems Section - Read Only */}
          <Box bg="white" p="6" rounded="lg" shadow="sm">
            <HStack mb="4">
              <LuGlobe size={24} />
              <Heading size="md">Układy Planetarne ({planetarySystems.length})</Heading>
            </HStack>
            
            {planetarySystems.length === 0 ? (
              <Text color="gray.500">Brak układów planetarnych. Skontaktuj się z administratorem.</Text>
            ) : (
              <VStack gap="4" align="stretch">
                {planetarySystems.map(system => (
                  <Box key={system.id} p="4" borderWidth="1px" borderRadius="md" bg="gray.50">
                    <HStack justify="space-between" mb="2">
                      <VStack align="start" gap="1">
                        <Heading size="sm">{system.name}</Heading>
                        <Text fontSize="sm" color="gray.600">{system.description}</Text>
                      </VStack>
                      <IconButton 
                        size="sm" 
                        variant="ghost" 
                        onClick={() => handleShowSystemDetails(system)}
                        title="Szczegóły układu"
                      >
                        <LuInfo />
                      </IconButton>
                    </HStack>
                    
                    {system.stationedShips?.length > 0 && (
                      <Box mt="3">
                        <Text fontSize="sm" fontWeight="bold" mb="2">
                          Stacjonujące statki ({system.stationedShips.length}):
                        </Text>
                        <HStack gap="2" flexWrap="wrap">
                          {system.stationedShips.map(ship => (
                            <Badge key={ship.id} colorPalette="blue">
                              {ship.name} ({getShipTypeNamePl(ship.type)})
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

          {/* Fleet Ships Section - Read Only with Assignment */}
          <Box bg="white" p="6" rounded="lg" shadow="sm">
            <HStack mb="4">
              <LuRocket size={24} />
              <Heading size="md">Flota ({fleet.length} statków)</Heading>
            </HStack>

            {fleet.length === 0 ? (
              <Text color="gray.500">Brak statków we flocie. Skontaktuj się z administratorem.</Text>
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
                        <Badge colorPalette="purple">{getShipTypeNamePl(ship.type)}</Badge>
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
                        <IconButton 
                          size="xs" 
                          variant="ghost"
                          onClick={() => handleAssignShip(ship)}
                          title="Przypisz do układu"
                        >
                          <LuMapPin />
                        </IconButton>
                      </Table.Cell>
                    </Table.Row>
                  ))}
                </Table.Body>
              </Table.Root>
            )}
          </Box>

          {/* Unassigned Ships Warning */}
          {unassignedShips.length > 0 && (
            <Box bg="orange.50" p="4" rounded="lg" borderWidth="1px" borderColor="orange.200">
              <Heading size="sm" mb="2" color="orange.700">
                ⚠️ Statki nieprzypisane do żadnego układu ({unassignedShips.length})
              </Heading>
              <Text fontSize="sm" color="orange.600" mb="2">
                Przypisz te statki do układów planetarnych używając przycisku 📍
              </Text>
              <HStack gap="2" flexWrap="wrap">
                {unassignedShips.map(ship => (
                  <Badge key={ship.id} colorPalette="orange" cursor="pointer" onClick={() => handleAssignShip(ship)}>
                    {ship.name} ({getShipTypeNamePl(ship.type)})
                  </Badge>
                ))}
              </HStack>
            </Box>
          )}
        </VStack>
      </Box>

      {/* System Details Dialog */}
      <Dialog.Root open={systemDetailsOpen} onOpenChange={(e) => !e.open && setSystemDetailsOpen(false)}>
        <Portal>
          <Dialog.Backdrop />
          <Dialog.Positioner>
            <Dialog.Content maxW="500px">
              <Dialog.Header>
                <Dialog.Title>🌍 {selectedSystem?.name}</Dialog.Title>
              </Dialog.Header>
              <Dialog.Body>
                <VStack gap="4" align="stretch">
                  <Box>
                    <Text fontWeight="bold" mb="1">Opis:</Text>
                    <Text color="gray.600">{selectedSystem?.description || 'Brak opisu'}</Text>
                  </Box>
                  
                  <Box>
                    <Text fontWeight="bold" mb="2">Stacjonujące statki:</Text>
                    {selectedSystem?.stationedShips?.length > 0 ? (
                      <VStack align="stretch" gap="2">
                        {selectedSystem.stationedShips.map(ship => (
                          <HStack key={ship.id} p="2" bg="gray.50" borderRadius="md" justify="space-between">
                            <Text fontWeight="medium">{ship.name}</Text>
                            <Badge colorPalette="purple">{getShipTypeNamePl(ship.type)}</Badge>
                          </HStack>
                        ))}
                      </VStack>
                    ) : (
                      <Text color="gray.500">Brak stacjonujących statków</Text>
                    )}
                  </Box>
                  
                  <Button variant="outline" onClick={() => setSystemDetailsOpen(false)}>
                    Zamknij
                  </Button>
                </VStack>
              </Dialog.Body>
            </Dialog.Content>
          </Dialog.Positioner>
        </Portal>
      </Dialog.Root>

      {/* Assign Ship Dialog */}
      <Dialog.Root open={assignDialogOpen} onOpenChange={(e) => !e.open && setAssignDialogOpen(false)}>
        <Portal>
          <Dialog.Backdrop />
          <Dialog.Positioner>
            <Dialog.Content maxW="400px">
              <Dialog.Header>
                <Dialog.Title>Przypisz statek do układu</Dialog.Title>
              </Dialog.Header>
              <Dialog.Body>
                <VStack gap="3" align="stretch">
                  <Text>Wybierz układ planetarny dla statku <strong>{assigningShip?.name}</strong>:</Text>
                  {planetarySystems.map(system => (
                    <Button 
                      key={system.id}
                      variant={assigningShip?.stationedInSystemId === system.id ? "solid" : "outline"}
                      onClick={() => assignShipMutation.mutate({ shipId: assigningShip?.id, systemId: system.id })}
                    >
                      🌍 {system.name}
                    </Button>
                  ))}
                </VStack>
              </Dialog.Body>
            </Dialog.Content>
          </Dialog.Positioner>
        </Portal>
      </Dialog.Root>
    </>
  );
}

