import React, { useEffect } from 'react';
import { usePageTitle } from '../hooks/usePageTitle';
import { useAuth } from '../auth/AuthContext';

export default function Order() {
  usePageTitle('Order');
  const { isLoading, isAuthenticated, claims } = useAuth();

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      const url = new URL(window.location.href);
      url.pathname = '/';
      url.search = '?reason=notAuthorized';
      window.location.href = url.toString();
    }
  }, [isLoading, isAuthenticated]);

  if (isLoading) {
    return (
      <div className="min-h-[60vh] flex items-center justify-center bg-[#7f2615] text-[#fef3c7] px-6">
        <p className="text-lg md:text-xl">Checking your Merry Crustmas session…</p>
      </div>
    );
  }

  if (!isAuthenticated) {
    return null;
  }

  return (
    <div className="min-h-[60vh] flex items-center justify-center bg-[#7f2615] text-[#fef3c7] px-6">
      <div className="max-w-2xl w-full space-y-6">
        <h1 className="text-3xl md:text-4xl font-extrabold tracking-wide">
          Your Once-a-Year Holiday Slice
        </h1>

        {claims && claims.length > 0 && (
          <ul className="space-y-2 bg-black/20 rounded-2xl p-4 border border-[#fef3c7]/10">
            {claims.map((claim, index) => (
              <li key={`${claim.type}-${index}`} className="text-sm md:text-base">
                <span className="font-bold">{claim.type}:</span> {claim.value}
              </li>
            ))}
          </ul>
        )}
      </div>
    </div>
  );
}
