import { createContext, useContext, useState, useCallback } from 'react';

const AuthContext = createContext(null);

function getUserFromToken() {
  const token = localStorage.getItem('token');
  if (!token) return null;
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    return {
      id: parseInt(payload.sub, 10),
      email: payload.email,
      name: payload.name,
      token
    };
  } catch {
    return null;
  }
}

export function AuthProvider({ children }) {
  const [user, setUser] = useState(() => getUserFromToken());

  const login = useCallback((token) => {
    localStorage.setItem('token', token);
    setUser(getUserFromToken());
  }, []);

  const logout = useCallback(() => {
    localStorage.removeItem('token');
    setUser(null);
  }, []);

  const isAuthenticated = !!user;

  return (
    <AuthContext.Provider value={{ user, isAuthenticated, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export const useAuth = () => {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within AuthProvider');
  return ctx;
};

