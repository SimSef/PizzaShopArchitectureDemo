import React, { createContext, useContext, useEffect, useState } from 'react';
import { BFF_BASE_URL } from '../bffConfig';

const AuthContext = createContext({
  isLoading: true,
  isAuthenticated: false,
  claims: [],
  isAdmin: false,
  isCustomer: false,
});

export function AuthProvider({ children }) {
  const [state, setState] = useState({
    isLoading: true,
    isAuthenticated: false,
    claims: [],
    isAdmin: false,
    isCustomer: false,
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
          const claims = Array.isArray(data.claims) ? data.claims : [];

          // Debug: inspect raw claims coming from the BFF.
          // This helps verify what Keycloak roles look like in the SPA.
          // eslint-disable-next-line no-console
          console.log('[Auth] claims from /api/me', claims);

          let isAdmin = false;
          let isCustomer = false;
          const realmAccess = claims.find((c) => c.type === 'realm_access');
          if (realmAccess && typeof realmAccess.value === 'string') {
            try {
              const parsed = JSON.parse(realmAccess.value);
              const roles = Array.isArray(parsed.roles) ? parsed.roles : [];
              isAdmin = roles.includes('pizza-admin');
              isCustomer = roles.includes('pizza-customer');
            } catch {
              // ignore parse errors, treat as no roles
            }
          }

          setState({
            isLoading: false,
            isAuthenticated: true,
            claims,
            isAdmin,
            isCustomer,
          });
        }
      } catch {
        if (!cancelled) {
          setState({
            isLoading: false,
            isAuthenticated: false,
            claims: [],
            isAdmin: false,
            isCustomer: false,
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
