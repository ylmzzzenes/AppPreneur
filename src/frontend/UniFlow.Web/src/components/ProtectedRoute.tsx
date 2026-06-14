import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import { PageLoader } from './PageLoader';

export function ProtectedRoute() {
  const { isAuthenticated, isBootstrapping, profile } = useAuth();
  const location = useLocation();

  if (!isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location }} />;
  }

  if (isBootstrapping) {
    return <PageLoader label="Oturum doğrulanıyor..." />;
  }

  const isOnboardingRoute = location.pathname === '/onboarding';
  if (profile && !profile.isOnboardingCompleted && !isOnboardingRoute) {
    return <Navigate to="/onboarding" replace />;
  }

  if (profile?.isOnboardingCompleted && isOnboardingRoute) {
    return <Navigate to="/dashboard" replace />;
  }

  return <Outlet />;
}
