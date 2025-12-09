import { useParams, Link } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { getBattleAdmin, startBattle } from '../../services/api';
import { useState } from 'react';
import {
  Box, Heading, VStack, HStack, Button, Spinner, Text
} from '@chakra-ui/react';
import { Table } from '@chakra-ui/react';
import { useModal } from '../../hooks/useModal';
import { ConfirmModal } from '../../components/modals/ConfirmModal';
import { useNotification } from '../../contexts/NotificationContext';

export default function BattleDetails() {
  const { battleId } = useParams();
  const queryClient = useQueryClient();
  const [copiedToken, setCopiedToken] = useState(null);
  
  const confirmModal = useModal();
  const { showSuccess, showError } = useNotification();
  
  const { data: battle, isLoading } = useQuery({
    queryKey: ['battle', battleId, 'admin'],
    queryFn: () => getBattleAdmin(battleId)
  });

  const startMutation = useMutation({
    mutationFn: () => startBattle(battleId),
    onSuccess: () => {
      queryClient.invalidateQueries(['battle', battleId]);
    }
  });

  const handleStartBattle = () => {
    confirmModal.openModal({
      message: 'Czy na pewno chcesz rozpocząć bitwę? Po rozpoczęciu nie będzie można dodawać jednostek.',
      onConfirm: () => startMutation.mutate()
    });
  };

  const copyPlayerLink = async (fraction) => {
    if (!fraction.authToken) {
      showError('Auth token nie jest dostępny dla tej frakcji.');
      return;
    }
    const playerUrl = `${window.location.origin}/battles/${battleId}/simulator?token=${fraction.authToken}&fractionId=${fraction.fractionId}`;
    
    try {
      // Próbuj użyć nowoczesnego API
      if (navigator.clipboard && navigator.clipboard.writeText) {
        await navigator.clipboard.writeText(playerUrl);
      } else {
        // Fallback dla starszych przeglądarek lub HTTP
        const textArea = document.createElement('textarea');
        textArea.value = playerUrl;
        textArea.style.position = 'fixed';
        textArea.style.left = '-999999px';
        document.body.appendChild(textArea);
        textArea.select();
        document.execCommand('copy');
        document.body.removeChild(textArea);
      }
      setCopiedToken(fraction.fractionId);
      setTimeout(() => setCopiedToken(null), 2000);
      showSuccess('Link skopiowany do schowka!');
    } catch (error) {
      console.error('Failed to copy:', error);
      showError('Nie udało się skopiować linku. Spróbuj ręcznie: ' + playerUrl);
    }
  };

  if (isLoading) return <Spinner />;
  if (!battle) return <Text>Battle not found</Text>;

  return (
    <Box>
      <HStack justify="space-between" mb="4">
        <Heading size="md">{battle.name}</Heading>
        <HStack>
          {battle.status === 'Preparation' && (
            <>
              <Button 
                onClick={handleStartBattle} 
                colorScheme="orange"
                isLoading={startMutation.isPending}
              >
                ▶️ Start Battle
              </Button>
              <Button 
                as={Link} 
                to={`/pustka-admin-panel/${battleId}/fractions/new`} 
                colorScheme="purple"
              >
                Add Fraction
              </Button>
            </>
          )}
          {battle.status === 'InProgress' && (
            <Button 
              as={Link} 
              to={`/pustka-admin-panel/${battleId}/admin-logs`} 
              colorScheme="cyan"
            >
              📊 Admin Logs
            </Button>
          )}
          <Button as={Link} to="/pustka-admin-panel" variant="outline">
            Back to List
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
                    <Table.ColumnHeader>Color</Table.ColumnHeader>
                    <Table.ColumnHeader>Fraction Name</Table.ColumnHeader>
                    <Table.ColumnHeader>Player Name</Table.ColumnHeader>
                    <Table.ColumnHeader>Player Link</Table.ColumnHeader>
                    <Table.ColumnHeader>Ships Count</Table.ColumnHeader>
                    <Table.ColumnHeader>Status</Table.ColumnHeader>
                    <Table.ColumnHeader>Actions</Table.ColumnHeader>
                  </Table.Row>
                </Table.Header>
                <Table.Body>
                  {battle.fractions.map((fraction) => (
                    <Table.Row key={fraction.fractionId}>
                      <Table.Cell>
                        <Box
                          width="30px"
                          height="30px"
                          borderRadius="md"
                          bg={fraction.fractionColor || '#999'}
                          border="1px solid"
                          borderColor="gray.300"
                        />
                      </Table.Cell>
                      <Table.Cell>{fraction.fractionName}</Table.Cell>
                      <Table.Cell>{fraction.playerName || '-'}</Table.Cell>
                      <Table.Cell>
                        {fraction.authToken ? (
                          <Button
                            size="sm"
                            colorScheme={copiedToken === fraction.fractionId ? "green" : "gray"}
                            onClick={() => copyPlayerLink(fraction)}
                          >
                            {copiedToken === fraction.fractionId ? '✓ Copied!' : '📋 Copy Link'}
                          </Button>
                        ) : (
                          <Text fontSize="sm" color="gray.500">Token niedostępny</Text>
                        )}
                      </Table.Cell>
                      <Table.Cell>{fraction.ships?.length || 0}</Table.Cell>
                      <Table.Cell>{fraction.isDefeated ? 'Defeated' : 'Active'}</Table.Cell>
                      <Table.Cell>
                        <HStack>
                          <Button
                            as={Link}
                            to={`/pustka-admin-panel/${battleId}/fractions/${fraction.fractionId}/edit`}
                            size="sm"
                            colorScheme="teal"
                            isDisabled={battle.status !== 'Preparation'}
                          >
                            Edit
                          </Button>
                          <Button
                            as={Link}
                            to={`/pustka-admin-panel/${battleId}/fractions/${fraction.fractionId}/ships`}
                            size="sm"
                            colorScheme="blue"
                          >
                            Manage Ships
                          </Button>
                        </HStack>
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

      <ConfirmModal
        isOpen={confirmModal.isOpen}
        onClose={confirmModal.closeModal}
        onConfirm={confirmModal.modalData.onConfirm || (() => {})}
        title="Potwierdzenie"
        message={confirmModal.modalData.message}
        confirmText="Rozpocznij"
        cancelText="Anuluj"
        colorScheme="orange"
      />
    </Box>
  );
}
