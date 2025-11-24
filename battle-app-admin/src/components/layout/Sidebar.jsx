import { Box, VStack, Link as CLink } from '@chakra-ui/react';
import { Link } from 'react-router-dom';

export default function Sidebar() {
  return (
    <Box w={{ base: 0, md: 64 }} bg="gray.800" color="white" p="4" h="100vh" position="fixed">
      <VStack align="stretch" spacing="2">
        <CLink as={Link} to="/pustka-admin-panel" color="white" _hover={{ textDecor: 'none', bg: 'gray.700' }} p="2" rounded="md">
          Battles
        </CLink>
        <CLink as={Link} to="/pustka-admin-panel/new" color="white" _hover={{ textDecor: 'none', bg: 'gray.700' }} p="2" rounded="md">
          New Battle
        </CLink>
      </VStack>
    </Box>
  );
}
