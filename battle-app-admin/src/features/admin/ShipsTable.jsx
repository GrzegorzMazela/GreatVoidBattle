import { useParams, Link } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { listShips, deleteShip, getBattle } from '../../services/api';
import { Box, Heading, Button, Table, HStack, Spinner, createToaster, Toaster, Text, VStack, Badge, Breadcrumb } from '@chakra-ui/react';

const toaster = createToaster({
  placement: 'top-end',
  duration: 3000,
});

const statusColors = {
  'Draft': 'gray',
  'InProgress': 'blue',
  'Finished': 'green'
};

export default function ShipsTable() {
  const { battleId, fractionId } = useParams();
  const qc = useQueryClient();
  
  const { data: ships, isLoading: shipsLoading } = useQuery({ 
    queryKey: ['ships', battleId, fractionId], 
    queryFn: () => listShips(battleId, fractionId) 
  });

  const { data: battle, isLoading: battleLoading } = useQuery({
    queryKey: ['battle', battleId],
    queryFn: () => getBattle(battleId)
  });

  const del = useMutation({
    mutationFn: (shipId) => deleteShip(battleId, fractionId, shipId),
    onSuccess: () => { 
      qc.invalidateQueries(['ships', battleId, fractionId]); 
      qc.invalidateQueries(['battle', battleId]);
      toaster.create({ title: 'Ship deleted', type: 'success' }); 
    }
  });

  if (shipsLoading || battleLoading) return <Spinner />;

  const fraction = battle?.fractions?.find(f => f.fractionId === fractionId);

  console.log('Battle data:', battle);
  console.log('Fraction ID:', fractionId);
  console.log('Found fraction:', fraction);

  return (
    <>
      <Toaster toaster={toaster} />
      <Box>
        {/* Breadcrumb Navigation */}
        <Breadcrumb.Root mb="4" fontSize="sm">
          <Breadcrumb.List>
            <Breadcrumb.Item>
              <Breadcrumb.Link asChild>
                <Link to="/admin">Battles</Link>
              </Breadcrumb.Link>
            </Breadcrumb.Item>
            <Breadcrumb.Separator />
            <Breadcrumb.Item>
              <Breadcrumb.Link asChild>
                <Link to={`/admin/${battleId}`}>{battle?.name || 'Battle'}</Link>
              </Breadcrumb.Link>
            </Breadcrumb.Item>
            <Breadcrumb.Separator />
            <Breadcrumb.Item>
              <Breadcrumb.CurrentLink>{fraction?.name || 'Fraction'} Ships</Breadcrumb.CurrentLink>
            </Breadcrumb.Item>
          </Breadcrumb.List>
        </Breadcrumb.Root>

        <HStack justify="space-between" mb="4">
          <VStack align="start" gap="1">
            <Heading size="md">Ships</Heading>
            {fraction && <Text fontSize="sm" color="gray.600">Fraction: {fraction.name}</Text>}
            {battle && (
              <Badge colorScheme={statusColors[battle.status] || 'gray'}>
                Battle Status: {battle.status}
              </Badge>
            )}
          </VStack>
          <HStack>
            <Button as={Link} to={`/admin/${battleId}/fractions/${fractionId}/ships/new`} colorScheme="green">
              Add Ship
            </Button>
            <Button as={Link} to={`/admin/${battleId}`} variant="outline">
              Back to Battle
            </Button>
          </HStack>
        </HStack>
        <Box bg="white" rounded="lg" shadow="sm" overflow="hidden">
          <Table.Root>
            <Table.Header>
              <Table.Row>
                <Table.ColumnHeader>Name</Table.ColumnHeader>
                <Table.ColumnHeader>Type</Table.ColumnHeader>
                <Table.ColumnHeader>Position</Table.ColumnHeader>
                <Table.ColumnHeader>Modules</Table.ColumnHeader>
                <Table.ColumnHeader>Actions</Table.ColumnHeader>
              </Table.Row>
            </Table.Header>
            <Table.Body>
              {ships?.map(s => (
                <Table.Row key={s.shipId}>
                  <Table.Cell>{s.name}</Table.Cell>
                  <Table.Cell>{s.type}</Table.Cell>
                  <Table.Cell>({s.x}, {s.y})</Table.Cell>
                  <Table.Cell>
                    <VStack align="start" gap="1">
                      {s.modules?.map((module, idx) => (
                        <Box key={idx} fontSize="xs">
                          <Text fontWeight="bold">Module {idx + 1}:</Text>
                          <Text color="gray.600">
                            {module.weaponTypes?.join(', ') || 'No weapons'}
                          </Text>
                        </Box>
                      ))}
                    </VStack>
                  </Table.Cell>
                  <Table.Cell>
                    <HStack>
                      <Button 
                        size="sm" 
                        colorScheme="blue" 
                        variant="outline" 
                        as={Link} 
                        to={`/admin/${battleId}/fractions/${fractionId}/ships/${s.shipId}/edit`}
                      >
                        Edit
                      </Button>
                      <Button size="sm" colorScheme="red" variant="outline" onClick={() => del.mutate(s.shipId)}>
                        Delete
                      </Button>
                    </HStack>
                  </Table.Cell>
                </Table.Row>
              ))}
            </Table.Body>
          </Table.Root>
        </Box>
      </Box>
    </>
  );
}
