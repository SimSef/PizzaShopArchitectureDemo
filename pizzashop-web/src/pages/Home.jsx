import React, { useEffect, useState } from 'react';
import { usePageTitle } from '../hooks/usePageTitle';

export default function Home() {
  usePageTitle('Home');

  const [banner, setBanner] = useState('');

  useEffect(() => {
    const url = new URL(window.location.href);
    const reason = url.searchParams.get('reason');

    if (reason === 'notAuthorized') {
      setBanner("You need to be signed in to see that page. Let's start your Merry Crustmas order here.");
    } else if (reason === 'notFound') {
      setBanner('That page melted away like snow on a warm pizza stone. Back to the cozy front door.');
    }

    if (reason) {
      url.searchParams.delete('reason');
      window.history.replaceState({}, '', url.pathname + url.search);
    }
  }, []);

  return (
    <div className="flex flex-col gap-10">
      {banner && (
        <div className="mx-auto max-w-5xl w-full bg-[#fef3c7] text-[#7f2615] rounded-xl px-4 py-3 shadow-md border border-[#fee2b3]">
          <p className="text-sm md:text-base font-semibold">{banner}</p>
        </div>
      )}
      {/* Hero */}
      <section
        id="order"
        className="relative w-screen"
        style={{
          marginLeft: 'calc(50% - 50vw)',
          marginRight: 'calc(50% - 50vw)',
        }}
      >
        <img
          src="/img/HomeIntro.png"
          alt="Festive pizza table"
          className="w-full h-auto block"
        />
        <div className="absolute inset-0 flex flex-col justify-end items-start bg-gradient-to-t from-black/60 via-black/20 to-transparent px-10 md:px-16 pb-10 text-[#fef3c7]">
          <h1 className="text-4xl md:text-6xl font-extrabold tracking-tight mb-4">
            Welcome to Merry Crustmas
          </h1>
          <p className="text-xl md:text-2xl font-semibold max-w-3xl">
            Cozy, wood-fired pizzas with a Christmas twist — only for the holiday
            season.
          </p>
        </div>
      </section>

      {/* Carol / About */}
      <section id="about" className="text-center px-6 md:px-0">
        <div className="max-w-5xl mx-auto flex flex-col gap-5">
          <h2 className="text-5xl md:text-6xl font-extrabold tracking-wide text-[#fef3c7]">
            Deck the night with crust and cheese.
          </h2>
          <p className="text-2xl md:text-3xl leading-relaxed text-[#fef3c7]">
            Merry Crustmas, warm and bright, pizzas glowing in the night.
            Snow outside and candles glow, basil leaves fall soft like snow.
          </p>
          <p className="text-2xl md:text-3xl leading-relaxed text-[#fef3c7]">
            Slice by slice the table sings, laughter, friends and cheesy strings.
            One short season, one sweet bite — holiday pizza, pure delight.
          </p>
        </div>
      </section>

      {/* Outro / Menu prompt */}
      <section
        id="menu"
        className="relative w-screen"
        style={{
          marginLeft: 'calc(50% - 50vw)',
          marginRight: 'calc(50% - 50vw)',
        }}
      >
        <img
          src="/img/HomeOutro.png"
          alt="Assorted pizzas for sharing"
          className="w-full h-auto block"
        />
        <div className="absolute inset-0 flex flex-col items-center justify-center text-center bg-gradient-to-t from-black/65 via-black/30 to-transparent px-8 py-6 text-[#fef3c7]">
          <h2 className="text-5xl md:text-6xl font-extrabold tracking-wide mb-4">
            Pick your slice of the season.
          </h2>
          <p className="text-2xl md:text-3xl max-w-3xl leading-relaxed">
            Scroll into the menu to see all Merry Crustmas creations:
            Margherita Natale, Pepperoni Sleigh Ride, Truffle Forest, and more —
            all available for a limited time only.
          </p>
        </div>
      </section>
    </div>
  );
}
