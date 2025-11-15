import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { listBattles, deleteBattle } from '../../services/api';
import {
  Box, Heading, Table, Button, HStack, Spinner, createToaster, Toaster
} from '@chakra-ui/react';
import { Link } from 'react-router-dom';

const toaster = createToaster({
  placement: 'top-end',
  duration: 3000,
});

export default function BattlesList() {
  const qc = useQueryClient();
  const { data, isLoading } = useQuery({ queryKey: ['battles'], queryFn: listBattles });

  const del = useMutation({
    mutationFn: (id) => deleteBattle(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['battles'] });
      toaster.create({ title: 'Battle deleted', type: 'success' });
    }
  });

  if (isLoading) return <Spinner />;

  return (
    <>
      <Toaster toaster={toaster} />
      <Box>
        <HStack justify="space-between" mb="4">
          <Heading size="md">Battles</Heading>
          <Button as={Link} to="/pustka-admin-panel/new" colorScheme="green">New</Button>
        </HStack>

      <Box bg="white" rounded="lg" shadow="sm" overflow="hidden">
        <Table.Root striped>
          <Table.Header>
            <Table.Row>
              <Table.ColumnHeader>Name</Table.ColumnHeader>
              <Table.ColumnHeader>Turn</Table.ColumnHeader>
              <Table.ColumnHeader>Status</Table.ColumnHeader>
              <Table.ColumnHeader>Size</Table.ColumnHeader>
              <Table.ColumnHeader>Fractions</Table.ColumnHeader>
              <Table.ColumnHeader>Actions</Table.ColumnHeader>
            </Table.Row>
          </Table.Header>

          <Table.Body>
            {data?.map((b) => (
              <Table.Row key={b.battleId}>
                <Table.Cell>{b.name || b.battleId}</Table.Cell>
                <Table.Cell>{b.turnNumber || 0}</Table.Cell>
                <Table.Cell>{b.status || 'Unknown'}</Table.Cell>
                <Table.Cell>{b.width}×{b.height}</Table.Cell>
                <Table.Cell>{b.fractions?.join(', ') || 'No fractions'}</Table.Cell>
                <Table.Cell>
                  <HStack>
                    <Button as={Link} to={`/pustka-admin-panel/${b.battleId}`} size="sm" colorScheme="blue">Details</Button>
                    <Button
                      size="sm"
                      colorScheme="red"
                      variant="outline"
                      onClick={() => del.mutate(b.battleId)}
                    >
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
