import { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import {
  Box,
  Heading,
  VStack,
  HStack,
  Text,
  Button,
  Card,
  CardBody,
  Badge,
  Spinner
} from '@chakra-ui/react';
import { gameStateApi } from '../../services/gameStateApi';
import './ResearchRequests.css';

const FRACTION_NAMES = {
  hegemonia_titanum: 'Hegemonia Titanum',
  shimura_incorporated: 'Shimura Incorporated',
  protektorat_pogranicza: 'Protektorat Pogranicza'
};

export default function ResearchRequests() {
  const { fractionId } = useParams();
  const [availableTechs, setAvailableTechs] = useState([]);
  const [pendingRequests, setPendingRequests] = useState([]);
  const [fractionState, setFractionState] = useState(null);
  const [loading, setLoading] = useState(true);
  const [successMessage, setSuccessMessage] = useState('');
  const [errorMessage, setErrorMessage] = useState('');

  useEffect(() => {
    loadData();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [fractionId]);

  const loadData = async () => {
    try {
      setLoading(true);
      const [state, techs, requests] = await Promise.all([
        gameStateApi.getFractionState(fractionId),
        gameStateApi.getAvailableTechnologies(fractionId),
        gameStateApi.getPendingRequests(fractionId)
      ]);

      setFractionState(state);
      setAvailableTechs(techs);
      setPendingRequests(requests);
    } catch (error) {
      console.error('Error loading data:', error);
      setErrorMessage('Nie udało się załadować danych');
      setTimeout(() => setErrorMessage(''), 3000);
    } finally {
      setLoading(false);
    }
  };

  const handleRequestResearch = async (technologyId) => {
    try {
      await gameStateApi.requestResearch(fractionId, technologyId);
      setSuccessMessage('Zgłoszono do badań - oczekuje na zatwierdzenie admina');
      setTimeout(() => setSuccessMessage(''), 3000);
      await loadData();
    } catch (error) {
      console.error('Error requesting research:', error);
      setErrorMessage('Nie udało się zgłosić technologii do badań');
      setTimeout(() => setErrorMessage(''), 3000);
    }
  };

  if (loading) {
    return (
      <Box p={6} display="flex" justifyContent="center">
        <Spinner size="xl" />
      </Box>
    );
  }

  const pendingTechIds = new Set(pendingRequests.map(r => r.technologyId));

  return (
    <Box p={6} className="research-requests">
      <VStack spacing={6} align="stretch">
        {successMessage && (
          <Box p={3} bg="green.100" borderRadius="md" color="green.800">
            ✓ {successMessage}
          </Box>
        )}

        {errorMessage && (
          <Box p={3} bg="red.100" borderRadius="md" color="red.800">
            ✗ {errorMessage}
          </Box>
        )}

        <Box>
          <Heading size="lg">{FRACTION_NAMES[fractionId]}</Heading>
          <Text color="gray.600" mt={2}>
            Tier {fractionState?.currentTier} | Zbadane technologie: {fractionState?.researchedTechnologiesInCurrentTier}/15
          </Text>
        </Box>

        {pendingRequests.length > 0 && (
          <Card bg="blue.50">
            <CardBody>
              <Heading size="md" mb={3}>Oczekujące zgłoszenia ({pendingRequests.length})</Heading>
              <VStack spacing={2} align="stretch">
                {pendingRequests.map(request => (
                  <HStack key={request.technologyId} justify="space-between">
                    <Text fontWeight="bold">{request.technologyName}</Text>
                    <Badge colorScheme="yellow">Oczekuje</Badge>
                  </HStack>
                ))}
              </VStack>
            </CardBody>
          </Card>
        )}

        {availableTechs.map(tierData => (
          <Card key={tierData.tier}>
            <CardBody>
              <Heading size="md" mb={4}>Tier {tierData.tier}</Heading>
              
              <VStack spacing={3} align="stretch">
                {tierData.technologies
                  .filter(tech => !tech.isOwned)
                  .map(tech => {
                    const isPending = pendingTechIds.has(tech.id);
                    const canRequest = tech.canResearch && !isPending;

                    return (
                      <Box
                        key={tech.id}
                        p={4}
                        border="1px solid"
                        borderColor="gray.200"
                        borderRadius="md"
                        bg={isPending ? 'blue.50' : canRequest ? 'white' : 'gray.50'}
                      >
                        <HStack justify="space-between" align="start">
                          <Box flex={1}>
                            <HStack mb={2}>
                              <Text fontWeight="bold">{tech.name}</Text>
                              {isPending && (
                                <Badge colorScheme="yellow">Zgłoszono</Badge>
                              )}
                              {tech.isOwned && (
                                <Badge colorScheme="green">Posiadana</Badge>
                              )}
                            </HStack>
                            <Text fontSize="sm" color="gray.600" mb={2}>
                              {tech.description}
                            </Text>
                            {tech.requiredTechnologies?.length > 0 && (
                              <Text fontSize="xs" color="gray.500">
                                Wymaga: {tech.requiredTechnologies.length} technologii
                              </Text>
                            )}
                            {tech.missingRequirements?.length > 0 && (
                              <Text fontSize="xs" color="red.500">
                                Brakuje: {tech.missingRequirements.length} technologii
                              </Text>
                            )}
                          </Box>
                          <Button
                            colorScheme="blue"
                            size="sm"
                            isDisabled={!canRequest}
                            onClick={() => handleRequestResearch(tech.id)}
                          >
                            {isPending ? 'Zgłoszono' : 'Zgłoś do badań'}
                          </Button>
                        </HStack>
                      </Box>
                    );
                  })}
              </VStack>

              {tierData.technologies.filter(t => !t.isOwned).length === 0 && (
                <Box p={3} bg="blue.100" borderRadius="md" color="blue.800">
                  ℹ Wszystkie technologie z tego tiera są już zbadane
                </Box>
              )}
            </CardBody>
          </Card>
        ))}
      </VStack>
    </Box>
  );
}
