import { Box, Heading, Text, VStack } from '@chakra-ui/react';

export default function HegemoniaPage() {
  return (
    <Box>
      <Heading size="lg" mb="4">👑 Hegemonia Titanum</Heading>
      <VStack gap="4" align="stretch">
        <Box bg="white" p="6" rounded="lg" shadow="sm" borderLeft="4px solid" borderColor="gold">
          <Heading size="md" mb="3">O Frakcji</Heading>
          <Text color="gray.700">
            Hegemonia Titanum - potężne imperium kontrolujące zasoby Titanu.
            Ta strona jest dostępna tylko dla członków Hegemonii Titanum oraz administratorów.
          </Text>
        </Box>
        
        <Box bg="white" p="6" rounded="lg" shadow="sm">
          <Heading size="md" mb="3">Twoje Misje</Heading>
          <Text color="gray.600">
            Lista misji specyficznych dla Hegemonii Titanum pojawi się tutaj.
          </Text>
        </Box>
      </VStack>
    </Box>
  );
}
