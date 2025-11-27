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
    { path: '/pustka-admin-panel/hegemonia', label: '👑 Hegemonia Titanum', exact: false, requiredRole: 'Hegemonia Titanum', submenu: [
      { path: '/pustka-admin-panel/hegemonia/technologies', label: '📖 Lista Technologii', exact: false, requiredRole: 'Hegemonia Titanum' },
      { path: '/pustka-admin-panel/hegemonia/research', label: '🔬 Zgłoś Badania', exact: false, requiredRole: 'Hegemonia Titanum' }
    ]},
    { path: '/pustka-admin-panel/shimura', label: '🏢 Shimura Incorporated', exact: false, requiredRole: 'Shimura Incorporated', submenu: [
      { path: '/pustka-admin-panel/shimura/technologies', label: '📖 Lista Technologii', exact: false, requiredRole: 'Shimura Incorporated' },
      { path: '/pustka-admin-panel/shimura/research', label: '🔬 Zgłoś Badania', exact: false, requiredRole: 'Shimura Incorporated' }
    ]},
    { path: '/pustka-admin-panel/protektorat', label: '🛡️ Protektorat Pogranicza', exact: false, requiredRole: 'Protektorat Pogranicza', submenu: [
      { path: '/pustka-admin-panel/protektorat/technologies', label: '📖 Lista Technologii', exact: false, requiredRole: 'Protektorat Pogranicza' },
      { path: '/pustka-admin-panel/protektorat/research', label: '🔬 Zgłoś Badania', exact: false, requiredRole: 'Protektorat Pogranicza' }
    ]},
    { path: '/pustka-admin-panel/admin-panel', label: '⚙️ Panel Admina', exact: false, requiredRole: 'admin', submenu: [
      { path: '/pustka-admin-panel/admin-panel/technologies', label: '🔬 Zarządzanie Technologiami', exact: false, requiredRole: 'admin' },
      { path: '/pustka-admin-panel/admin-panel/turn-management', label: '🎲 Zakończ Turę', exact: false, requiredRole: 'admin' }
    ]},
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
          <div key={item.path}>
            <CLink
              as={Link}
              to={item.path}
              className={`sidebar-item ${isActive(item.path, item.exact) ? 'active' : ''}`}
            >
              {item.label}
            </CLink>
            {item.submenu && isActive(item.path, false) && (
              <VStack align="stretch" spacing="1" pl="4" mt="1">
                {item.submenu.filter(canAccessItem).map((subitem) => (
                  <CLink
                    key={subitem.path}
                    as={Link}
                    to={subitem.path}
                    className={`sidebar-item submenu ${isActive(subitem.path, subitem.exact) ? 'active' : ''}`}
                  >
                    {subitem.label}
                  </CLink>
                ))}
              </VStack>
            )}
          </div>
        ))}
      </VStack>
    </Box>
  );
}
