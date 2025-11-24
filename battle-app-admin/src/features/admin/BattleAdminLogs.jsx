import { useParams, Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { getBattleAdmin, getAdminTurnLogs } from '../../services/api';
import { useState } from 'react';
import {
  Box, Heading, VStack, HStack, Button, Spinner, Text, Badge
} from '@chakra-ui/react';
import { Table } from '@chakra-ui/react';

export default function BattleAdminLogs() {
  const { battleId } = useParams();
  const [selectedTurn, setSelectedTurn] = useState(1);
  
  const { data: battle, isLoading: isBattleLoading } = useQuery({
    queryKey: ['battle', battleId, 'admin'],
    queryFn: () => getBattleAdmin(battleId)
  });

  const { data: turnLogs, isLoading: isLogsLoading } = useQuery({
    queryKey: ['battle', battleId, 'admin-logs', selectedTurn],
    queryFn: () => getAdminTurnLogs(battleId, selectedTurn),
    enabled: !!battle && battle.turnNumber > 0
  });

  if (isBattleLoading) return <Spinner />;
  if (!battle) return <Text>Battle not found</Text>;

  const maxTurn = battle.turnNumber || 1;
  const turns = Array.from({ length: maxTurn }, (_, i) => i + 1);

  const getLogTypeColor = (type) => {
    const colors = {
      'ShipMove': 'blue',
      'LaserHit': 'red',
      'LaserMiss': 'gray',
      'MissileFired': 'orange',
      'MissileHit': 'red',
      'MissileMiss': 'gray',
      'ShipDestroyed': 'purple',
      'TurnStart': 'green',
      'TurnEnd': 'green'
    };
    return colors[type] || 'gray';
  };

  const getLogIcon = (type) => {
    const icons = {
      'MissileFired': '🚀',
      'MissileHit': '💥',
      'MissileMiss': '🌫️',
      'LaserHit': '⚡',
      'LaserMiss': '✨',
      'ShipMove': '🔄',
      'ShipDestroyed': '💀',
      'DamageReceived': '🛡️'
    };
    return icons[type] || '📋';
  };

  const getMissileInfo = (log) => {
    if (!log.adminLog) return null;
    
    if (log.type === 'MissileFired') {
      // Parse: [Admin] Missile {guid} fired at turn {n}, initialAccuracy={n}, distance={n}
      const match = log.adminLog.match(/Missile ([\w-]+) fired at turn (\d+), initialAccuracy=(\d+), distance=(\d+)/);
      if (!match) return null;
      
      return (
        <Box p={4} bg="orange.50" rounded="lg" borderLeft="4px solid" borderColor="orange.500" shadow="sm">
          <HStack mb={3}>
            <Text fontSize="2xl">🚀</Text>
            <Text fontWeight="bold" fontSize="lg" color="orange.700">Wystrzelenie Rakiety</Text>
          </HStack>
          <VStack align="stretch" gap={2} fontSize="sm">
            <HStack justify="space-between" p={2} bg="white" rounded="md">
              <Text color="gray.600" fontWeight="medium">ID Rakiety:</Text>
              <Text fontFamily="mono" fontSize="xs" bg="gray.100" px={2} py={1} rounded="md">{match[1]}</Text>
            </HStack>
            <HStack justify="space-between" p={2} bg="white" rounded="md">
              <Text color="gray.600" fontWeight="medium">Tura wystrzelenia:</Text>
              <Badge colorScheme="orange" fontSize="md" px={3} py={1}>Tura {match[2]}</Badge>
            </HStack>
            <HStack justify="space-between" p={2} bg="white" rounded="md">
              <Text color="gray.600" fontWeight="medium">Początkowa dokładność:</Text>
              <Badge colorScheme="blue" fontSize="md" px={3} py={1}>{match[3]}%</Badge>
            </HStack>
            <HStack justify="space-between" p={2} bg="white" rounded="md">
              <Text color="gray.600" fontWeight="medium">Dystans do celu:</Text>
              <Badge colorScheme="cyan" fontSize="md" px={3} py={1}>{match[4]} pól</Badge>
            </HStack>
          </VStack>
        </Box>
      );
    }

    if (log.type === 'MissileHit' || log.type === 'MissileMiss') {
      // Parse: [Admin] Missile {guid}: firedAt={n}, hitAt={n}, travel={n} turns, initAcc={n}, finalAcc={n}, rolled={n}, hit={bool}, dmg={n}
      const match = log.adminLog.match(/Missile ([\w-]+): firedAt=(\d+), hitAt=(\d+), travel=(\d+) turns, initAcc=(\d+), finalAcc=(\d+), rolled=(\d+), hit=(True|False), dmg=(\d+)/);
      if (!match) return null;
      
      const isHit = match[8] === 'True';
      const hitChance = parseInt(match[6]);
      const rolled = parseInt(match[7]);
      
      return (
        <Box 
          p={4} 
          bg={isHit ? "red.50" : "gray.100"} 
          rounded="lg" 
          borderLeft="4px solid" 
          borderColor={isHit ? "red.500" : "gray.400"}
          shadow="sm"
        >
          <HStack mb={3}>
            <Text fontSize="2xl">{isHit ? '💥' : '🌫️'}</Text>
            <Text fontWeight="bold" fontSize="lg" color={isHit ? "red.700" : "gray.700"}>
              {isHit ? 'Trafienie Rakiety' : 'Chybienie Rakiety'}
            </Text>
          </HStack>
          
          <VStack align="stretch" gap={2} fontSize="sm">
            <HStack justify="space-between" p={2} bg="white" rounded="md">
              <Text color="gray.600" fontWeight="medium">ID Rakiety:</Text>
              <Text fontFamily="mono" fontSize="xs" bg="gray.100" px={2} py={1} rounded="md">{match[1]}</Text>
            </HStack>
            
            <Box p={3} bg="white" rounded="md" borderLeft="3px solid" borderColor="blue.300">
              <Text fontSize="xs" fontWeight="bold" color="blue.700" mb={2}>🕐 Oś czasu</Text>
              <VStack align="stretch" gap={1}>
                <HStack justify="space-between">
                  <Text color="gray.600">Wystrzelona:</Text>
                  <Badge colorScheme="orange">Tura {match[2]}</Badge>
                </HStack>
                <HStack justify="space-between">
                  <Text color="gray.600">Trafiła:</Text>
                  <Badge colorScheme="green">Tura {match[3]}</Badge>
                </HStack>
                <HStack justify="space-between">
                  <Text color="gray.600">Czas lotu:</Text>
                  <Badge colorScheme="purple">{match[4]} tur</Badge>
                </HStack>
              </VStack>
            </Box>
            
            <Box p={3} bg="white" rounded="md" borderLeft="3px solid" borderColor="cyan.300">
              <Text fontSize="xs" fontWeight="bold" color="cyan.700" mb={2}>🎯 Dokładność</Text>
              <VStack align="stretch" gap={1}>
                <HStack justify="space-between">
                  <Text color="gray.600">Początkowa:</Text>
                  <Badge colorScheme="blue">{match[5]}%</Badge>
                </HStack>
                <HStack justify="space-between">
                  <Text color="gray.600">Końcowa:</Text>
                  <Badge colorScheme="purple">{match[6]}%</Badge>
                </HStack>
              </VStack>
            </Box>
            
            <Box p={3} bg={rolled <= hitChance ? "green.50" : "red.50"} rounded="md" borderLeft="3px solid" borderColor={rolled <= hitChance ? "green.400" : "red.400"}>
              <Text fontSize="xs" fontWeight="bold" color={rolled <= hitChance ? "green.700" : "red.700"} mb={2}>
                🎲 Test trafienia
              </Text>
              <VStack align="stretch" gap={2}>
                <HStack justify="space-between">
                  <Text color="gray.700" fontWeight="medium">Wylosowano:</Text>
                  <Badge colorScheme="yellow" fontSize="lg" px={3} py={1}>{rolled}</Badge>
                </HStack>
                <HStack justify="space-between">
                  <Text color="gray.700" fontWeight="medium">Potrzebne ≤</Text>
                  <Badge colorScheme="cyan" fontSize="lg" px={3} py={1}>{hitChance}</Badge>
                </HStack>
                <Box textAlign="center" mt={1}>
                  <Badge 
                    colorScheme={isHit ? "green" : "red"} 
                    fontSize="lg" 
                    px={4} 
                    py={2}
                    fontWeight="bold"
                  >
                    {isHit ? '✓ TRAFIENIE' : '✗ CHYBIENIE'}
                  </Badge>
                </Box>
              </VStack>
            </Box>
            
            {isHit && (
              <HStack justify="space-between" p={3} bg="red.100" rounded="md" borderLeft="3px solid" borderColor="red.500">
                <Text color="red.800" fontWeight="bold">💔 Obrażenia:</Text>
                <Badge colorScheme="red" fontSize="lg" px={3} py={1}>{match[9]} HP</Badge>
              </HStack>
            )}
          </VStack>
        </Box>
      );
    }

    return null;
  };

  return (
    <Box>
      <HStack justify="space-between" mb="4">
        <Heading size="md">Admin Logs: {battle.name}</Heading>
        <Button as={Link} to={`/pustka-admin-panel/${battleId}`} variant="outline">
          Back to Battle
        </Button>
      </HStack>

      {battle.status === 'Preparation' ? (
        <Box bg="yellow.50" p={4} rounded="lg" borderLeft="4px solid" borderColor="yellow.400">
          <Text color="yellow.800">
            Bitwa jest w fazie przygotowań. Logi będą dostępne po rozpoczęciu bitwy.
          </Text>
        </Box>
      ) : (
        <VStack align="stretch" gap={4}>
          {/* Turn selector */}
          <Box bg="white" p={4} rounded="lg" shadow="md" borderTop="4px solid" borderColor="blue.500">
            <HStack justify="space-between" mb={3}>
              <Box>
                <Text fontWeight="bold" fontSize="lg" color="gray.800">Wybierz turę do analizy</Text>
                <Text fontSize="sm" color="gray.600">Przeglądaj szczegółowe logi każdej tury</Text>
              </Box>
              <Badge colorScheme="blue" fontSize="md" px={3} py={1}>
                {maxTurn} {maxTurn === 1 ? 'tura' : maxTurn < 5 ? 'tury' : 'tur'}
              </Badge>
            </HStack>
            <Box 
              display="grid" 
              gridTemplateColumns="repeat(auto-fill, minmax(100px, 1fr))" 
              gap={2}
            >
              {turns.map((turn) => (
                <Button
                  key={turn}
                  size="md"
                  colorScheme={selectedTurn === turn ? 'blue' : 'gray'}
                  onClick={() => setSelectedTurn(turn)}
                  variant={selectedTurn === turn ? 'solid' : 'outline'}
                  fontWeight={selectedTurn === turn ? 'bold' : 'normal'}
                  _hover={{
                    transform: 'translateY(-2px)',
                    shadow: 'md'
                  }}
                  transition="all 0.2s"
                >
                  Tura {turn}
                </Button>
              ))}
            </Box>
          </Box>

          {/* Logs display */}
          {isLogsLoading ? (
            <Spinner />
          ) : turnLogs && turnLogs.length > 0 ? (
            <Box bg="white" rounded="lg" shadow="sm" overflow="hidden">
              <Box p={4} bg="blue.50" borderBottom="1px solid" borderColor="blue.200">
                <HStack justify="space-between">
                  <Box>
                    <Heading size="sm" color="blue.800">Logi Tury {selectedTurn}</Heading>
                    <Text fontSize="sm" color="blue.600" mt={1}>
                      Wyświetlone {turnLogs.length} {turnLogs.length === 1 ? 'zdarzenie' : turnLogs.length < 5 ? 'zdarzenia' : 'zdarzeń'}
                    </Text>
                  </Box>
                </HStack>
              </Box>
              <VStack align="stretch" gap={0}>
                {turnLogs.map((log, index) => {
                  const missileInfo = getMissileInfo(log);
                  const logIcon = getLogIcon(log.type);
                  
                  return (
                    <Box 
                      key={index} 
                      p={4} 
                      borderBottom={index < turnLogs.length - 1 ? "1px solid" : "none"}
                      borderColor="gray.200"
                      _hover={{ bg: "gray.50" }}
                      transition="background 0.2s"
                    >
                      <HStack justify="space-between" mb={3} align="start">
                        <HStack gap={3} flex={1}>
                          <Text fontSize="2xl">{logIcon}</Text>
                          <Box>
                            <HStack gap={2} mb={1}>
                              <Badge colorScheme={getLogTypeColor(log.type)} fontSize="sm" px={2} py={1}>
                                {log.type}
                              </Badge>
                              {log.fractionName && (
                                <Badge colorScheme="purple" fontSize="sm" px={2} py={1}>
                                  {log.fractionName}
                                </Badge>
                              )}
                              {log.shipName && (
                                <Text fontSize="sm" fontWeight="medium" color="gray.700">
                                  {log.shipName}
                                </Text>
                              )}
                            </HStack>
                            <Text color="gray.700" fontSize="md">
                              {log.message}
                            </Text>
                          </Box>
                        </HStack>
                        <Badge colorScheme="gray" fontSize="xs">#{index + 1}</Badge>
                      </HStack>

                      {missileInfo && (
                        <Box mt={3}>
                          {missileInfo}
                        </Box>
                      )}
                    </Box>
                  );
                })}
              </VStack>
            </Box>
          ) : (
            <Box bg="white" p={4} rounded="lg" shadow="sm">
              <Text color="gray.500">Brak logów dla tury {selectedTurn}</Text>
            </Box>
          )}
        </VStack>
      )}
    </Box>
  );
}
