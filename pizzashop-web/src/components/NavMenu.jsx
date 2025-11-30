import React from 'react';
import { useAuth } from '../auth/AuthContext';

export default function NavMenu() {
  const { isLoading, isAuthenticated } = useAuth();
  const path = window.location.pathname.toLowerCase();

  return (
    <header className="sticky top-0 z-50 bg-[#7f2615] border-b border-[#5b1b0f] shadow-md">
      <div className="max-w-6xl mx-auto px-6 py-4 flex items-center justify-between gap-4 text-[#fef3c7]">
        <a href="/" className="flex flex-col gap-1 hover:text-white transition-colors">
          <span className="text-lg font-extrabold tracking-widest uppercase">
            Merry Crustmas Pizzeria
          </span>
          <span className="text-sm text-[#fee2b3]">“Your Once-a-Year Holiday Slice.”</span>
        </a>
        <nav className="flex items-center gap-6 text-base font-semibold">
          <a
            href="/menu"
            className={`hover:text-white transition-colors ${
              path === '/menu' ? 'text-white' : ''
            }`}
          >
            Menu
          </a>
          <a
            href="/about"
            className={`hover:text-white transition-colors ${
              path === '/about' ? 'text-white' : ''
            }`}
          >
            About
          </a>

          {/* Auth section: mirror Blazor AuthorizeView */}
          {!isLoading && isAuthenticated && (
            <div className="flex items-center gap-4">
              <a
                href="/order"
                className="inline-flex items-center rounded-full bg-white text-[#7f2615] px-4 py-2 text-sm font-bold tracking-wide shadow hover:shadow-lg transition"
              >
                Order
              </a>
              <a
                href="/logout"
                className="text-sm font-semibold text-[#fee2b3] hover:text-white transition-colors"
              >
                Logout
              </a>
            </div>
          )}

          {!isLoading && !isAuthenticated && (
            <a
              href="/start-order"
              className="hover:text-white transition-colors"
            >
              Order Now
            </a>
          )}
        </nav>
      </div>
    </header>
  );
}

