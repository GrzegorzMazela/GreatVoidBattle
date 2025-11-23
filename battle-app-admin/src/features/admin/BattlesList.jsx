import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { listBattles, deleteBattle } from '../../services/api';
import {
  Box, Heading, Table, Button, HStack, Spinner
} from '@chakra-ui/react';
import { Link } from 'react-router-dom';
import { useState } from 'react';
import { ConfirmModal } from '../../components/modals/ConfirmModal';

export default function BattlesList() {
  const qc = useQueryClient();
  const { data, isLoading } = useQuery({ queryKey: ['battles'], queryFn: listBattles });
  const [battleToDelete, setBattleToDelete] = useState(null);

  const del = useMutation({
    mutationFn: (id) => deleteBattle(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['battles'] });
      setBattleToDelete(null);
    },
    onError: (error) => {
      console.error('Failed to delete battle:', error);
      setBattleToDelete(null);
    }
  });

  const handleDeleteClick = (battle) => {
    setBattleToDelete(battle);
  };

  const handleConfirmDelete = () => {
    if (battleToDelete) {
      del.mutate(battleToDelete.battleId);
    }
  };

  if (isLoading) return <Spinner />;

  return (
    <>
      <Box>
        <HStack justify="space-between" mb="4">
          <Heading size="md">Bitwy</Heading>
          <Button as={Link} to="/pustka-admin-panel/new" colorScheme="green">Nowa Bitwa</Button>
        </HStack>

      <Box bg="white" rounded="lg" shadow="sm" overflow="hidden">
        <Table.Root striped>
          <Table.Header>
            <Table.Row>
              <Table.ColumnHeader>Nazwa</Table.ColumnHeader>
              <Table.ColumnHeader>Tura</Table.ColumnHeader>
              <Table.ColumnHeader>Status</Table.ColumnHeader>
              <Table.ColumnHeader>Rozmiar</Table.ColumnHeader>
              <Table.ColumnHeader>Frakcje</Table.ColumnHeader>
              <Table.ColumnHeader>Akcje</Table.ColumnHeader>
            </Table.Row>
          </Table.Header>

          <Table.Body>
            {data?.map((b) => (
              <Table.Row key={b.battleId}>
                <Table.Cell>{b.name || b.battleId}</Table.Cell>
                <Table.Cell>{b.turnNumber || 0}</Table.Cell>
                <Table.Cell>{b.status || 'Nieznany'}</Table.Cell>
                <Table.Cell>{b.width}×{b.height}</Table.Cell>
                <Table.Cell>{b.fractions?.join(', ') || 'Brak frakcji'}</Table.Cell>
                <Table.Cell>
                  <HStack>
                    <Button as={Link} to={`/pustka-admin-panel/${b.battleId}`} size="sm" colorScheme="blue">
                      <span role="img" aria-label="szczegóły">👁️</span>
                    </Button>
                    <Button
                      size="sm"
                      colorScheme="red"
                      onClick={() => handleDeleteClick(b)}
                    >
                      <span role="img" aria-label="usuń">🗑️</span>
                    </Button>
                  </HStack>
                </Table.Cell>
              </Table.Row>
            ))}
          </Table.Body>
        </Table.Root>
      </Box>
    </Box>

      <ConfirmModal
        isOpen={!!battleToDelete}
        onClose={() => setBattleToDelete(null)}
        onConfirm={handleConfirmDelete}
        title="Potwierdzenie usunięcia"
        message={`Czy na pewno chcesz usunąć bitwę "${battleToDelete?.name || battleToDelete?.battleId}"? Ta operacja jest nieodwracalna.`}
        confirmText="Usuń"
        cancelText="Anuluj"
        colorScheme="red"
      />
    </>
  );
}
