import { Outlet } from 'react-router-dom';
import { Box, Flex } from '@chakra-ui/react';
import Sidebar from '../components/layout/Sidebar';
import Topbar from '../components/layout/Topbar';
import './AdminLayout.css';

/**
 * Layout dla panelu administracyjnego z Sidebar i Topbar
 */
export default function AdminLayout() {
  return (
    <Flex h="100vh" bg="#f5f7fa">
      <Sidebar />
      <Box flex="1" ml={{ base: 0, md: '280px' }} display="flex" flexDir="column">
        <Topbar />
        <Box as="main" className="admin-content">
          <Outlet />
        </Box>
      </Box>
    </Flex>
  );
}
