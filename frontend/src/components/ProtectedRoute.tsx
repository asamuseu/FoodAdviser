import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import type { ReactNode } from 'react';

interface ProtectedRouteProps {
  children: ReactNode;
}

/**
 * A wrapper component that protects routes from unauthenticated access.
 * Redirects to the login page if the user is not authenticated.
 * Preserves the original URL so the user can be redirected back after login.
 */
export default function ProtectedRoute({ children }: ProtectedRouteProps) {
  const { isAuthenticated, isLoading } = useAuth();
  const location = useLocation();

  // Show nothing while checking authentication status
  if (isLoading) {
    return (
      <div className="container">
        <div className="loading-auth">
          <span className="spinner" />
          <span>Checking authentication...</span>
        </div>
      </div>
    );
  }

  // Redirect to login if not authenticated, preserving the intended destination
  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location.pathname }} replace />;
  }

  return <>{children}</>;
}
