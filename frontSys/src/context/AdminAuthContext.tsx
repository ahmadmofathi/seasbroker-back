import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import {
  clearSuperuserToken,
  getCollectionToken,
  restoreSuperuserSession,
  superuserLogin,
  superuserRefresh,
} from '../api/auth';
import { isAdminAuthenticated } from '../api/adminClient';
import pb from '../utils/pocketbase';

interface AdminAuthContextValue {
  isAuthenticated: boolean;
  login: (identity: string, password: string) => Promise<void>;
  logout: () => void;
  refreshSession: () => Promise<void>;
}

const AdminAuthContext = createContext<AdminAuthContextValue | null>(null);

export const AdminAuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [isAuthenticated, setIsAuthenticated] = useState(() => {
    restoreSuperuserSession();
    return isAdminAuthenticated();
  });

  useEffect(() => {
    restoreSuperuserSession();
    setIsAuthenticated(isAdminAuthenticated());

    return pb.authStore.onChange(() => {
      setIsAuthenticated(!!getCollectionToken());
    });
  }, []);

  const login = useCallback(async (identity: string, password: string) => {
    await superuserLogin(identity, password);
    setIsAuthenticated(!!getCollectionToken());
  }, []);

  const logout = useCallback(() => {
    clearSuperuserToken();
    setIsAuthenticated(false);
  }, []);

  const refreshSession = useCallback(async () => {
    await superuserRefresh();
    setIsAuthenticated(!!getCollectionToken());
  }, []);

  const value = useMemo(
    () => ({
      isAuthenticated,
      login,
      logout,
      refreshSession,
    }),
    [isAuthenticated, login, logout, refreshSession],
  );

  return <AdminAuthContext.Provider value={value}>{children}</AdminAuthContext.Provider>;
};

export function useAdminAuth(): AdminAuthContextValue {
  const ctx = useContext(AdminAuthContext);
  if (!ctx) {
    throw new Error('useAdminAuth must be used within AdminAuthProvider');
  }
  return ctx;
}
