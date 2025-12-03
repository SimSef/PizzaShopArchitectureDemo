import React, { useEffect, useState } from 'react';
import { usePageTitle } from '../hooks/usePageTitle';
import { useAuth } from '../auth/AuthContext';
import { BFF_BASE_URL } from '../bffConfig';

export default function Dashboard() {
  usePageTitle('Admin Dashboard');
  const { isLoading, isAuthenticated, isAdmin } = useAuth();
  const [state, setState] = useState({
    isLoading: true,
    users: [],
    orders: [],
    error: null,
  });

  useEffect(() => {
    if (isLoading || !isAuthenticated || !isAdmin) {
      return;
    }

    let cancelled = false;

    async function load() {
      try {
        const response = await fetch(`${BFF_BASE_URL}/api/admin/dashboard`, {
          credentials: 'include',
        });

        if (!response.ok) {
          const text = await response.text().catch(() => '');
          if (!cancelled) {
            setState({
              isLoading: false,
              users: [],
              orders: [],
              error: text || 'Failed to load admin dashboard.',
            });
          }
          return;
        }

        const data = await response.json();
        if (!cancelled) {
          setState({
            isLoading: false,
            users: Array.isArray(data.users) ? data.users : [],
            orders: Array.isArray(data.orders) ? data.orders : [],
            error: null,
          });
        }
      } catch (err) {
        if (!cancelled) {
          setState({
            isLoading: false,
            users: [],
            orders: [],
            error: err instanceof Error ? err.message : 'Failed to load admin dashboard.',
          });
        }
      }
    }

    load();

    return () => {
      cancelled = true;
    };
  }, [isLoading, isAuthenticated, isAdmin]);

  if (isLoading) {
    return (
      <div className="min-h-[60vh] flex items-center justify-center bg-[#7f2615] text-[#fef3c7] px-6">
        <p className="text-lg md:text-xl">Checking your Merry Crustmas session…</p>
      </div>
    );
  }

  if (!isAuthenticated || !isAdmin) {
    return (
      <div className="min-h-[60vh] flex items-center justify-center bg-[#7f2615] text-[#fef3c7] px-6">
        <p className="text-lg md:text-xl">This sleigh route is only for pizza admins.</p>
      </div>
    );
  }

  if (state.isLoading) {
    return (
      <div className="min-h-[60vh] flex items-center justify-center bg-[#7f2615] text-[#fef3c7] px-6">
        <p className="text-lg md:text-xl">Loading your Merry Crustmas dashboard…</p>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-[#7f2615] text-[#fef3c7] px-6 py-12 md:py-16">
      <div className="max-w-6xl mx-auto space-y-8">
        <header className="space-y-3">
          <h1 className="text-3xl md:text-4xl font-extrabold tracking-wide">
            Pizza Admin Sleighboard
          </h1>
          <p className="text-lg md:text-xl max-w-2xl leading-relaxed">
            A cozy overview of everyone&apos;s Merry Crustmas cravings—who&apos;s ordering, and what&apos;s
            flying out of the oven.
          </p>
        </header>

        {state.error && (
          <p className="text-sm text-red-200 bg-black/30 border border-red-400/40 rounded-xl px-4 py-2">
            {state.error}
          </p>
        )}

        <div className="grid md:grid-cols-2 gap-6 items-start">
          <section className="bg-black/25 border border-[#fef3c7]/10 rounded-2xl p-4 space-y-3 shadow-lg">
            <h2 className="text-xl font-semibold mb-2">Users</h2>
            {state.users.length === 0 ? (
              <p className="text-sm text-[#fde68a]">No users have appeared in the sleigh just yet.</p>
            ) : (
              <ul className="space-y-2 text-sm md:text-base">
                {state.users.map((user) => (
                  <li
                    key={user.userId}
                    className="flex flex-col bg-black/30 rounded-xl px-3 py-2 border border-[#fef3c7]/10"
                  >
                    <span className="font-semibold">{user.displayName || user.userId}</span>
                    <span className="text-xs text-[#fde68a] break-all">{user.userId}</span>
                  </li>
                ))}
              </ul>
            )}
          </section>

          <section className="bg-black/25 border border-[#fef3c7]/10 rounded-2xl p-4 space-y-3 shadow-lg">
            <h2 className="text-xl font-semibold mb-2">Orders</h2>
            {state.orders.length === 0 ? (
              <p className="text-sm text-[#fde68a]">
                No orders have been placed yet—your oven is patiently waiting.
              </p>
            ) : (
              <ul className="space-y-3 text-sm md:text-base max-h-[420px] overflow-auto pr-1">
                {state.orders.map((order) => (
                  <li
                    key={order.orderId}
                    className="bg-black/30 rounded-xl px-3 py-2 border border-[#fef3c7]/10"
                  >
                    <div className="flex justify-between items-center gap-2">
                      <span className="font-semibold">{order.userName}</span>
                      <span className="text-xs text-[#fde68a]">
                        €{Number(order.totalAmount).toFixed(2)}
                      </span>
                    </div>
                    <p className="text-xs mt-1 text-[#fee2b3]">{order.summary}</p>
                    <p className="text-[10px] mt-1 text-[#fed7aa] break-all">
                      #{order.orderId}
                    </p>
                  </li>
                ))}
              </ul>
            )}
          </section>
        </div>
      </div>
    </div>
  );
}

