import React, { createContext, useContext, useEffect, useState } from 'react';
import { BFF_BASE_URL } from '../bffConfig';

const AuthContext = createContext({
  isLoading: true,
  isAuthenticated: false,
  claims: [],
});

export function AuthProvider({ children }) {
  const [state, setState] = useState({
    isLoading: true,
    isAuthenticated: false,
    claims: [],
  });

  useEffect(() => {
    let cancelled = false;

    async function load() {
      try {
        const response = await fetch(`${BFF_BASE_URL}/api/me`, {
          credentials: 'include',
        });

        if (!response.ok) {
          if (!cancelled) {
            setState({
              isLoading: false,
              isAuthenticated: false,
              claims: [],
            });
          }
          return;
        }

        const data = await response.json();
        if (!cancelled) {
          setState({
            isLoading: false,
            isAuthenticated: true,
            claims: Array.isArray(data.claims) ? data.claims : [],
          });
        }
      } catch {
        if (!cancelled) {
          setState({
            isLoading: false,
            isAuthenticated: false,
            claims: [],
          });
        }
      }
    }

    load();

    return () => {
      cancelled = true;
    };
  }, []);

  return <AuthContext.Provider value={state}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  return useContext(AuthContext);
}
