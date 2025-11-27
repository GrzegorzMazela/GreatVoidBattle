import { createBrowserRouter } from 'react-router-dom';
import AdminLayout from './AdminLayout';
import SimulatorLayout from './SimulatorLayout';
import BattlesList from '../features/admin/BattlesList';
import BattleForm from '../features/admin/BattleForm';
import BattleDetails from '../features/admin/BattleDetails';
import BattleAdminLogs from '../features/admin/BattleAdminLogs';
import FractionForm from '../features/admin/FractionForm';
import ShipsTable from '../features/admin/ShipsTable';
import ShipForm from '../features/admin/ShipForm';
import { BattleSimulator } from '../features/battle-simulator';
import MainPage from '../components/MainPage';
import { DiscordLogin } from '../components/auth/DiscordLogin';
import { DiscordCallback } from '../components/auth/DiscordCallback';
import { ProtectedRoute } from '../components/auth/ProtectedRoute';
import { FractionAuthRoute } from '../components/auth/FractionAuthRoute';
import AllBattlesPage from '../features/admin/AllBattlesPage';
import HegemoniaPage from '../features/admin/HegemoniaPage';
import ShimuraPage from '../features/admin/ShimuraPage';
import ProtektoratPage from '../features/admin/ProtektoratPage';
import AdminPanelPage from '../features/admin/AdminPanelPage';
import { TechnologyAdmin } from '../features/admin/TechnologyAdminSimple';
import { FractionSettingsAdmin } from '../features/admin/FractionSettingsAdmin';
import { OwnedTechnologies } from '../features/player/OwnedTechnologies';
import ResearchRequests from '../features/player/ResearchRequests';
import TurnManagementSimple from '../features/admin/TurnManagementSimple';

export const router = createBrowserRouter([
  // Strona logowania Discord
  {
    path: '/login',
    element: <DiscordLogin />,
  },
  // Callback po autoryzacji Discord
  {
    path: '/auth/discord/callback',
    element: <DiscordCallback />,
  },
  // Panel administracyjny z layoutem (Sidebar + Topbar) - wymaga Discord login
  {
    path: '/pustka-admin-panel',
    element: (
      <ProtectedRoute requirePlayer={true}>
        <AdminLayout />
      </ProtectedRoute>
    ),
    children: [
      { index: true, element: <BattlesList /> },
      { path: 'new', element: <BattleForm /> },
      { path: ':battleId', element: <BattleDetails /> },
      { path: ':battleId/admin-logs', element: <BattleAdminLogs /> },
      { path: ':battleId/fractions/new', element: <FractionForm /> },
      { path: ':battleId/fractions/:fractionId/edit', element: <FractionForm /> },
      { path: ':battleId/fractions/:fractionId/ships', element: <ShipsTable /> },
      { path: ':battleId/fractions/:fractionId/ships/new', element: <ShipForm /> },
      { path: ':battleId/fractions/:fractionId/ships/:shipId/edit', element: <ShipForm /> },
      { 
        path: 'wszystkie-bitwy', 
        element: (
          <ProtectedRoute requirePlayer={true}>
            <AllBattlesPage />
          </ProtectedRoute>
        )
      },
      { 
        path: 'hegemonia', 
        element: (
          <ProtectedRoute allowedRoles={["Hegemonia Titanum"]}>
            <HegemoniaPage />
          </ProtectedRoute>
        )
      },
      { 
        path: 'hegemonia/technologies', 
        element: (
          <ProtectedRoute allowedRoles={["Hegemonia Titanum"]}>
            <OwnedTechnologies />
          </ProtectedRoute>
        )
      },
      { 
        path: 'hegemonia/research', 
        element: (
          <ProtectedRoute allowedRoles={["Hegemonia Titanum"]}>
            <ResearchRequests />
          </ProtectedRoute>
        )
      },
      { 
        path: 'shimura', 
        element: (
          <ProtectedRoute allowedRoles={["Shimura Incorporated"]}>
            <ShimuraPage />
          </ProtectedRoute>
        )
      },
      { 
        path: 'shimura/technologies', 
        element: (
          <ProtectedRoute allowedRoles={["Shimura Incorporated"]}>
            <OwnedTechnologies />
          </ProtectedRoute>
        )
      },
      { 
        path: 'shimura/research', 
        element: (
          <ProtectedRoute allowedRoles={["Shimura Incorporated"]}>
            <ResearchRequests />
          </ProtectedRoute>
        )
      },
      { 
        path: 'protektorat', 
        element: (
          <ProtectedRoute allowedRoles={["Protektorat Pogranicza"]}>
            <ProtektoratPage />
          </ProtectedRoute>
        )
      },
      { 
        path: 'protektorat/technologies', 
        element: (
          <ProtectedRoute allowedRoles={["Protektorat Pogranicza"]}>
            <OwnedTechnologies />
          </ProtectedRoute>
        )
      },
      { 
        path: 'protektorat/research', 
        element: (
          <ProtectedRoute allowedRoles={["Protektorat Pogranicza"]}>
            <ResearchRequests />
          </ProtectedRoute>
        )
      },
      { 
        path: 'admin-panel', 
        element: (
          <ProtectedRoute requireAdmin={true}>
            <AdminPanelPage />
          </ProtectedRoute>
        )
      },
      { 
        path: 'admin-panel/technologies', 
        element: (
          <ProtectedRoute requireAdmin={true}>
            <TechnologyAdmin />
          </ProtectedRoute>
        )
      },
      { 
        path: 'admin-panel/fraction-settings', 
        element: (
          <ProtectedRoute requireAdmin={true}>
            <FractionSettingsAdmin />
          </ProtectedRoute>
        )
      },
      { 
        path: 'admin-panel/turn-management', 
        element: (
          <ProtectedRoute requireAdmin={true}>
            <TurnManagementSimple />
          </ProtectedRoute>
        )
      },
    ],
  },
  // Symulator - pełny ekran bez layoutu
  // Używa FractionAuthRoute - wymaga tylko auth key (token), NIE wymaga logowania Discord
  {
    path: '/battles',
    element: <SimulatorLayout />,
    children: [
      { 
        path: ':battleId/simulator', 
        element: (
          <FractionAuthRoute>
            <BattleSimulator />
          </FractionAuthRoute>
        )
      },
    ],
  },
  // Strona główna
  {
    path: '/',
    element: <MainPage />,
  },
]);
