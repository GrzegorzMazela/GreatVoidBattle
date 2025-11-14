import { Outlet, Link } from 'react-router-dom';
import { Box, Flex } from '@chakra-ui/react';
import Sidebar from '../components/layout/Sidebar';
import Topbar from '../components/layout/Topbar';

export default function App() {
  return (
    <Flex h="100vh" bg="gray.50">
      <Sidebar />
      <Box flex="1" ml={{ base: 0, md: 64 }} display="flex" flexDir="column">
        <Topbar />
        <Box as="main" p={6} overflow="auto">
          <Outlet />
        </Box>
      </Box>
    </Flex>
  );
}
