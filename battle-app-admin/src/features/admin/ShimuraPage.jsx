import { Box, Heading, Text, VStack } from '@chakra-ui/react';

export default function ShimuraPage() {
  return (
    <Box>
      <Heading size="lg" mb="4">🏢 Shimura Incorporated</Heading>
      <VStack gap="4" align="stretch">
        <Box bg="white" p="6" rounded="lg" shadow="sm" borderLeft="4px solid" borderColor="blue.500">
          <Heading size="md" mb="3">O Korporacji</Heading>
          <Text color="gray.700">
            Shimura Incorporated - najbardziej zaawansowana technologicznie korporacja w galaktyce.
            Ta strona jest dostępna tylko dla pracowników Shimura Inc. oraz administratorów.
          </Text>
        </Box>
        
        <Box bg="white" p="6" rounded="lg" shadow="sm">
          <Heading size="md" mb="3">Projekty Badawcze</Heading>
          <Text color="gray.600">
            Informacje o aktualnych projektach badawczych Shimura Incorporated.
          </Text>
        </Box>
      </VStack>
    </Box>
  );
}
