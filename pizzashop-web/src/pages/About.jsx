import React from 'react';
import { usePageTitle } from '../hooks/usePageTitle';

export default function About() {
  usePageTitle('About');

  return (
    <section
      className="relative w-screen"
      style={{
        marginLeft: 'calc(50% - 50vw)',
        marginRight: 'calc(50% - 50vw)',
      }}
    >
      <img
        src="/img/About.png"
        alt="Merry Crustmas story illustration"
        className="w-full h-auto block"
      />
    </section>
  );
}

