import { Box, Heading, Text, VStack, Grid } from '@chakra-ui/react';

export default function AdminPanelPage() {
  return (
    <Box>
      <Heading size="lg" mb="4">⚙️ Panel Administratora</Heading>
      <VStack gap="4" align="stretch">
        <Box bg="white" p="6" rounded="lg" shadow="sm" borderLeft="4px solid" borderColor="purple.500">
          <Heading size="md" mb="3">Witaj, Administratorze</Heading>
          <Text color="gray.700">
            To jest specjalny panel dostępny tylko dla administratorów systemu.
          </Text>
        </Box>
        
        <Grid templateColumns="repeat(auto-fit, minmax(250px, 1fr))" gap="4">
          <Box bg="white" p="6" rounded="lg" shadow="sm">
            <Heading size="sm" mb="2">📊 Statystyki</Heading>
            <Text color="gray.600" fontSize="sm">
              Ogólne statystyki systemu i aktywności użytkowników.
            </Text>
          </Box>
          
          <Box bg="white" p="6" rounded="lg" shadow="sm">
            <Heading size="sm" mb="2">👥 Zarządzanie Użytkownikami</Heading>
            <Text color="gray.600" fontSize="sm">
              Przeglądaj i zarządzaj kontami użytkowników.
            </Text>
          </Box>
          
          <Box bg="white" p="6" rounded="lg" shadow="sm">
            <Heading size="sm" mb="2">🔧 Ustawienia Systemu</Heading>
            <Text color="gray.600" fontSize="sm">
              Konfiguracja parametrów systemu.
            </Text>
          </Box>
          
          <Box bg="white" p="6" rounded="lg" shadow="sm">
            <Heading size="sm" mb="2">📝 Logi Systemowe</Heading>
            <Text color="gray.600" fontSize="sm">
              Przeglądaj logi działania systemu.
            </Text>
          </Box>
        </Grid>
      </VStack>
    </Box>
  );
}
