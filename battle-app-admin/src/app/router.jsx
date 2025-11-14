import { createBrowserRouter } from 'react-router-dom';
import App from './App';
import BattlesList from '../features/admin/BattlesList';
import BattleForm from '../features/admin/BattleForm';
import BattleDetails from '../features/admin/BattleDetails';
import FractionForm from '../features/admin/FractionForm';
import ShipsTable from '../features/admin/ShipsTable';
import ShipForm from '../features/admin/ShipForm';
import { BattleSimulator } from '../features/battle-simulator';

export const router = createBrowserRouter([
  {
    path: '/',
    element: <App />,
    children: [
      { index: true, element: <BattlesList /> },
      { path: 'admin', element: <BattlesList /> },
      { path: 'admin/new', element: <BattleForm /> },
      { path: 'admin/:battleId', element: <BattleDetails /> },
      { path: 'admin/:battleId/fractions/new', element: <FractionForm /> },
      { path: 'admin/:battleId/fractions/:fractionId/ships', element: <ShipsTable /> },
      { path: 'admin/:battleId/fractions/:fractionId/ships/new', element: <ShipForm /> },
      { path: 'admin/:battleId/fractions/:fractionId/ships/:shipId/edit', element: <ShipForm /> },
      { path: 'battles/:battleId/simulator', element: <BattleSimulator /> },
    ],
  },
]);
