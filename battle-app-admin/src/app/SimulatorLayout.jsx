import { Outlet } from 'react-router-dom';

/**
 * Layout dla symulatora - pełny ekran bez Sidebar i Topbar
 */
export default function SimulatorLayout() {
  return (
    <div style={{ width: '100vw', height: '100vh', overflow: 'hidden' }}>
      <Outlet />
    </div>
  );
}
