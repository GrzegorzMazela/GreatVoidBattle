import { Box, VStack, Link as CLink } from '@chakra-ui/react';
import { Link, useLocation } from 'react-router-dom';
import { getDiscordSession } from '../../services/discordAuthApi';
import logo from '../../assets/logo.png';
import './Sidebar.css';

export default function Sidebar() {
  const location = useLocation();
  const session = getDiscordSession();
  const isAdmin = session?.user?.isAdmin;
  const fractionRoles = session?.user?.fractionRoles || [];
  
  const menuItems = [
    { path: '/pustka-admin-panel', label: '⚔️ Bitwy', exact: true, requiredRole: null },
    { path: '/pustka-admin-panel/wszystkie-bitwy', label: '🌐 Wszystkie Bitwy', exact: false, requiredRole: 'player' },
    { path: '/pustka-admin-panel/hegemonia', label: '👑 Hegemonia Titanum', exact: false, requiredRole: 'Hegemonia Titanum' },
    { path: '/pustka-admin-panel/shimura', label: '🏢 Shimura Incorporated', exact: false, requiredRole: 'Shimura Incorporated' },
    { path: '/pustka-admin-panel/protektorat', label: '🛡️ Protektorat Pogranicza', exact: false, requiredRole: 'Protektorat Pogranicza' },
    { path: '/pustka-admin-panel/admin-panel', label: '⚙️ Panel Admina', exact: false, requiredRole: 'admin' },
  ];

  const canAccessItem = (item) => {
    if (!item.requiredRole) return true;
    if (isAdmin) return true;
    if (item.requiredRole === 'player') return true;
    if (item.requiredRole === 'admin') return false;
    return fractionRoles.includes(item.requiredRole);
  };

  const filteredMenuItems = menuItems.filter(canAccessItem);

  const isActive = (path, exact) => {
    if (exact) {
      return location.pathname === path;
    }
    return location.pathname.startsWith(path);
  };

  return (
    <Box className="sidebar">
      <div className="sidebar-header">
        <img src={logo} alt="Logo" className="sidebar-logo" />
        <h2 className="sidebar-title">Wielka Pustka</h2>
      </div>
      
      <VStack align="stretch" spacing="2" className="sidebar-menu">
        {filteredMenuItems.map((item) => (
          <CLink
            key={item.path}
            as={Link}
            to={item.path}
            className={`sidebar-item ${isActive(item.path, item.exact) ? 'active' : ''}`}
          >
            {item.label}
          </CLink>
        ))}
      </VStack>
    </Box>
  );
}
