import React, { useEffect } from 'react';
import { usePageTitle } from '../hooks/usePageTitle';
import { BFF_BASE_URL } from '../bffConfig';

export default function StartOrder() {
  usePageTitle('Start Order');

  useEffect(() => {
    const timer = setTimeout(() => {
      const encodedReturnUrl = encodeURIComponent('/order');
      window.location.href = `${BFF_BASE_URL}/authentication/login?returnUrl=${encodedReturnUrl}`;
    }, 1000);

    return () => clearTimeout(timer);
  }, []);

  return (
    <div className="min-h-[60vh] flex items-center justify-center bg-[#7f2615] text-[#fef3c7] px-6">
      <div className="max-w-xl text-center space-y-3">
        <h1 className="text-3xl md:text-4xl font-extrabold tracking-wide">
          Warming up the oven for your Merry Crustmas order…
        </h1>
      </div>
    </div>
  );
}
