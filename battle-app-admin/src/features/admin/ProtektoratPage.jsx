import { Box, Heading, Text, VStack } from '@chakra-ui/react';

export default function ProtektoratPage() {
  return (
    <Box>
      <Heading size="lg" mb="4">🛡️ Protektorat Pogranicza</Heading>
      <VStack gap="4" align="stretch">
        <Box bg="white" p="6" rounded="lg" shadow="sm" borderLeft="4px solid" borderColor="green.500">
          <Heading size="md" mb="3">O Protektoracie</Heading>
          <Text color="gray.700">
            Protektorat Pogranicza - obrońcy kolonii na krańcach znanej przestrzeni.
            Ta strona jest dostępna tylko dla członków Protektoratu oraz administratorów.
          </Text>
        </Box>
        
        <Box bg="white" p="6" rounded="lg" shadow="sm">
          <Heading size="md" mb="3">Linie Obronne</Heading>
          <Text color="gray.600">
            Mapa linii obronnych i statusy posterunków granicznych Protektoratu.
          </Text>
        </Box>
      </VStack>
    </Box>
  );
}
