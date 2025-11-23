import { Box, Heading, Text } from '@chakra-ui/react';

export default function AllBattlesPage() {
  return (
    <Box>
      <Heading size="lg" mb="4">🌐 Wszystkie Bitwy</Heading>
      <Box bg="white" p="6" rounded="lg" shadow="sm">
        <Text mb="4">Ta strona jest dostępna dla wszystkich zalogowanych graczy.</Text>
        <Text color="gray.600">
          Tutaj będzie lista wszystkich aktywnych bitew, w których mogą uczestniczyć gracze.
        </Text>
      </Box>
    </Box>
  );
}
