import { createContext, useContext, useState, useCallback, useEffect, type ReactNode } from 'react';
import type { User, AuthTokens } from '@/types';
import { getProfile } from '@/api/settings';

interface AuthContextType {
  user: User | null;
  tokens: AuthTokens | null;
  isAuthenticated: boolean;
  login: (tokens: AuthTokens, user: User) => void;
  logout: () => void;
  updateTokens: (tokens: AuthTokens) => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(() => {
    const stored = localStorage.getItem('user');
    return stored ? JSON.parse(stored) : null;
  });

  const [tokens, setTokens] = useState<AuthTokens | null>(() => {
    const stored = localStorage.getItem('tokens');
    return stored ? JSON.parse(stored) : null;
  });

  const login = useCallback((newTokens: AuthTokens, newUser: User) => {
    setTokens(newTokens);
    setUser(newUser);
    localStorage.setItem('tokens', JSON.stringify(newTokens));
    localStorage.setItem('user', JSON.stringify(newUser));
  }, []);

  const logout = useCallback(() => {
    setTokens(null);
    setUser(null);
    localStorage.removeItem('tokens');
    localStorage.removeItem('user');
  }, []);

  useEffect(() => {
    if (tokens?.accessToken && !user) {
      getProfile()
        .then((profile) => {
          setUser(profile);
          localStorage.setItem('user', JSON.stringify(profile));
        })
        .catch(() => {
          // Token is invalid — force logout
          setTokens(null);
          setUser(null);
          localStorage.removeItem('tokens');
          localStorage.removeItem('user');
        });
    }
  }, [tokens, user]);

  const updateTokens = useCallback((newTokens: AuthTokens) => {
    setTokens(newTokens);
    localStorage.setItem('tokens', JSON.stringify(newTokens));
  }, []);

  return (
    <AuthContext.Provider
      value={{
        user,
        tokens,
        isAuthenticated: !!tokens?.accessToken,
        login,
        logout,
        updateTokens,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) throw new Error('useAuth must be used within AuthProvider');
  return context;
}
