import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react';
import { useNavigate } from 'react-router-dom';
import { setUnauthorizedHandler } from '../api/client';
import { authApi, getErrorMessage, usersApi } from '../api/services';
import type { LoginRequest, RegisterRequest, UserProfile } from '../api/types';

interface AuthContextValue {
  isAuthenticated: boolean;
  isBootstrapping: boolean;
  profile: UserProfile | null;
  login: (request: LoginRequest) => Promise<string | null>;
  register: (request: RegisterRequest) => Promise<string | null>;
  logout: () => void;
  refreshProfile: () => Promise<void>;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const navigate = useNavigate();
  const [isAuthenticated, setIsAuthenticated] = useState(() => authApi.isAuthenticated());
  const [isBootstrapping, setIsBootstrapping] = useState(() => authApi.isAuthenticated());
  const [profile, setProfile] = useState<UserProfile | null>(null);

  const logout = useCallback(() => {
    authApi.logout();
    setIsAuthenticated(false);
    setProfile(null);
    navigate('/login', { replace: true });
  }, [navigate]);

  const refreshProfile = useCallback(async () => {
    if (!authApi.isAuthenticated()) {
      setProfile(null);
      setIsBootstrapping(false);
      return;
    }
    const result = await usersApi.me();
    if (!result.isSuccess) {
      if (result.error?.code === 'UNAUTHORIZED') {
        logout();
        return;
      }
      setProfile(null);
      setIsBootstrapping(false);
      return;
    }
    setProfile(result.data ?? null);
    setIsBootstrapping(false);
  }, [logout]);

  useEffect(() => {
    setUnauthorizedHandler(() => logout());
    if (authApi.isAuthenticated()) {
      void refreshProfile();
    } else {
      setIsBootstrapping(false);
    }
  }, [logout, refreshProfile]);

  const navigateAfterAuth = useCallback(
    async (data: UserProfile | null | undefined) => {
      setIsAuthenticated(true);
      setProfile(data ?? null);
      if (data?.isOnboardingCompleted) {
        navigate('/dashboard', { replace: true });
      } else {
        navigate('/onboarding', { replace: true });
      }
    },
    [navigate],
  );

  const login = useCallback(
    async (request: LoginRequest) => {
      const result = await authApi.login(request);
      if (!result.isSuccess) return getErrorMessage(result, 'Giriş başarısız.');
      const profileResult = await usersApi.me();
      if (profileResult.error?.code === 'UNAUTHORIZED') {
        logout();
        return 'Oturum doğrulanamadı.';
      }
      await navigateAfterAuth(profileResult.data);
      return null;
    },
    [logout, navigateAfterAuth],
  );

  const register = useCallback(
    async (request: RegisterRequest) => {
      const result = await authApi.register(request);
      if (!result.isSuccess) return getErrorMessage(result, 'Kayıt başarısız.');
      const profileResult = await usersApi.me();
      await navigateAfterAuth(profileResult.data);
      return null;
    },
    [navigateAfterAuth],
  );

  const value = useMemo(
    () => ({
      isAuthenticated,
      isBootstrapping,
      profile,
      login,
      register,
      logout,
      refreshProfile,
    }),
    [isAuthenticated, isBootstrapping, profile, login, register, logout, refreshProfile],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within AuthProvider');
  return ctx;
}
