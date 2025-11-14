import { useParams, Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { getBattle } from '../../services/api';
import {
  Box, Heading, VStack, HStack, Button, Spinner, Text
} from '@chakra-ui/react';
import { Table } from '@chakra-ui/react';

export default function BattleDetails() {
  const { battleId } = useParams();
  const { data: battle, isLoading } = useQuery({
    queryKey: ['battle', battleId],
    queryFn: () => getBattle(battleId)
  });

  if (isLoading) return <Spinner />;
  if (!battle) return <Text>Battle not found</Text>;

  return (
    <Box>
      <HStack justify="space-between" mb="4">
        <Heading size="md">{battle.name}</Heading>
        <HStack>
          <Button 
            as={Link} 
            to={`/battles/${battleId}/simulator`} 
            colorScheme="green"
            size="lg"
          >
            🎮 Uruchom Symulator
          </Button>
          <Button as={Link} to={`/admin/${battleId}/fractions/new`} colorScheme="purple">
            Add Fraction
          </Button>
          <Button as={Link} to="/admin" variant="outline">
            Back to Battles
          </Button>
        </HStack>
      </HStack>

      <VStack align="stretch" spacing="4">
        <Box bg="white" rounded="lg" shadow="sm" p="4">
          <Text><strong>Status:</strong> {battle.status}</Text>
        </Box>

        <Box>
          <Heading size="sm" mb="3">Fractions</Heading>
          {battle.fractions && battle.fractions.length > 0 ? (
            <Box bg="white" rounded="lg" shadow="sm" overflow="hidden">
              <Table.Root striped>
                <Table.Header>
                  <Table.Row>
                    <Table.ColumnHeader>Fraction Name</Table.ColumnHeader>
                    <Table.ColumnHeader>Ships Count</Table.ColumnHeader>
                    <Table.ColumnHeader>Status</Table.ColumnHeader>
                    <Table.ColumnHeader>Actions</Table.ColumnHeader>
                  </Table.Row>
                </Table.Header>
                <Table.Body>
                  {battle.fractions.map((fraction) => (
                    <Table.Row key={fraction.fractionId}>
                      <Table.Cell>{fraction.fractionName}</Table.Cell>
                      <Table.Cell>{fraction.ships?.length || 0}</Table.Cell>
                      <Table.Cell>{fraction.isDefeated ? 'Defeated' : 'Active'}</Table.Cell>
                      <Table.Cell>
                        <Button
                          as={Link}
                          to={`/admin/${battleId}/fractions/${fraction.fractionId}/ships`}
                          size="sm"
                          colorScheme="blue"
                        >
                          Manage Ships
                        </Button>
                      </Table.Cell>
                    </Table.Row>
                  ))}
                </Table.Body>
              </Table.Root>
            </Box>
          ) : (
            <Box bg="white" rounded="lg" shadow="sm" p="4">
              <Text color="gray.500">No fractions added yet. Add a fraction to start.</Text>
            </Box>
          )}
        </Box>
      </VStack>
    </Box>
  );
}
