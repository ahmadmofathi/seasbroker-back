import { Navigate, Outlet } from 'react-router';
import { useAdminAuth } from '../../context/AdminAuthContext';

const ProtectedAdminRoute: React.FC = () => {
  const { isAuthenticated } = useAdminAuth();

  if (!isAuthenticated) {
    return <Navigate to="/admin/login" replace />;
  }

  return <Outlet />;
};

export default ProtectedAdminRoute;
