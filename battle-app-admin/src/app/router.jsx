import { createBrowserRouter } from 'react-router-dom';
import AdminLayout from './AdminLayout';
import SimulatorLayout from './SimulatorLayout';
import BattlesList from '../features/admin/BattlesList';
import BattleForm from '../features/admin/BattleForm';
import BattleDetails from '../features/admin/BattleDetails';
import FractionForm from '../features/admin/FractionForm';
import ShipsTable from '../features/admin/ShipsTable';
import ShipForm from '../features/admin/ShipForm';
import { BattleSimulator } from '../features/battle-simulator';
import RequireAuth from '../components/RequireAuth';
import MainPage from '../components/MainPage';

export const router = createBrowserRouter([
  // Panel administracyjny z layoutem (Sidebar + Topbar)
  {
    path: '/pustka-admin-panel',
    element: <AdminLayout />,
    children: [
      { index: true, element: <BattlesList /> },
      { path: 'new', element: <BattleForm /> },
      { path: ':battleId', element: <BattleDetails /> },
      { path: ':battleId/fractions/new', element: <FractionForm /> },
      { path: ':battleId/fractions/:fractionId/edit', element: <FractionForm /> },
      { path: ':battleId/fractions/:fractionId/ships', element: <ShipsTable /> },
      { path: ':battleId/fractions/:fractionId/ships/new', element: <ShipForm /> },
      { path: ':battleId/fractions/:fractionId/ships/:shipId/edit', element: <ShipForm /> },
    ],
  },
  // Symulator - pełny ekran bez layoutu
  {
    path: '/battles',
    element: <SimulatorLayout />,
    children: [
      { 
        path: ':battleId/simulator', 
        element: (
          <RequireAuth>
            <BattleSimulator />
          </RequireAuth>
        )
      },
    ],
  },
  // Przekierowanie ze starego URL
  {
    path: '/',
    element: <MainPage />,
  },
]);
