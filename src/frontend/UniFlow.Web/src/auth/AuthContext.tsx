import {
  createContext,
  useCallback,
  useContext,
  useMemo,
  useState,
  type ReactNode,
} from 'react';
import { useNavigate } from 'react-router-dom';
import { authApi } from '../api/services';
import type { LoginRequest, RegisterRequest } from '../api/types';
import { getErrorMessage } from '../api/client';

interface AuthContextValue {
  isAuthenticated: boolean;
  login: (request: LoginRequest) => Promise<string | null>;
  register: (request: RegisterRequest) => Promise<string | null>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const navigate = useNavigate();
  const [isAuthenticated, setIsAuthenticated] = useState(() => authApi.isAuthenticated());

  const login = useCallback(async (request: LoginRequest) => {
    const result = await authApi.login(request);
    if (!result.isSuccess) {
      return getErrorMessage(result, 'Giriş başarısız.');
    }

    setIsAuthenticated(true);
    return null;
  }, []);

  const register = useCallback(async (request: RegisterRequest) => {
    const result = await authApi.register(request);
    if (!result.isSuccess) {
      return getErrorMessage(result, 'Kayıt başarısız.');
    }

    setIsAuthenticated(true);
    return null;
  }, []);

  const logout = useCallback(() => {
    authApi.logout();
    setIsAuthenticated(false);
    navigate('/login', { replace: true });
  }, [navigate]);

  const value = useMemo(
    () => ({ isAuthenticated, login, register, logout }),
    [isAuthenticated, login, register, logout],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) {
    throw new Error('useAuth must be used within AuthProvider');
  }

  return ctx;
}
