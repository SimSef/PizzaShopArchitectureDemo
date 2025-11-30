import React, { useEffect } from 'react';
import { usePageTitle } from '../hooks/usePageTitle';

export default function NotFound() {
  usePageTitle('Not Found');

  useEffect(() => {
    const timer = setTimeout(() => {
      const url = new URL(window.location.href);
      url.pathname = '/';
      url.search = '?reason=notFound';
      window.location.href = url.toString();
    }, 2000);

    return () => clearTimeout(timer);
  }, []);

  return (
    <div className="min-h-[60vh] flex items-center justify-center bg-[#7f2615] text-[#fef3c7] px-6">
      <div className="max-w-xl text-center space-y-3">
        <h1 className="text-3xl md:text-4xl font-extrabold tracking-wide">
          That page has gone out with last year&apos;s stockings.
        </h1>
        <p className="text-lg md:text-xl leading-relaxed">
          In a couple of moments we&apos;ll walk you back to the warm glow of the Merry Crustmas
          front door.
        </p>
      </div>
    </div>
  );
}

