import React from 'react';
import { usePageTitle } from '../hooks/usePageTitle';

const pizzas = [
  {
    id: 1,
    name: 'Margherita Natale',
    image: '/img/Margherita.png',
    alt: 'Margherita Natale pizza',
    align: 'left',
    description:
      'Slow-risen dough, bright tomato sugo, fior di latte and basil leaves scattered like little wreaths. The purest way to taste Christmas on a crust.',
  },
  {
    id: 2,
    name: 'Pepperoni Sleigh Ride',
    image: '/img/Pepperoni.png',
    alt: 'Pepperoni Sleigh Ride pizza',
    align: 'right',
    description:
      'Crisp-edged pepperoni, melted mozzarella and a rich tomato base, dotted with chili flakes like glowing sleigh lights on fresh snow.',
  },
  {
    id: 3,
    name: 'Truffle Forest',
    image: '/img/FourCheese.png',
    alt: 'Truffle Forest pizza',
    align: 'left',
    description:
      'A creamy white base with fior di latte, taleggio and Parmesan, drizzled with truffle oil and scattered with mushrooms like a winter forest floor.',
  },
  {
    id: 4,
    name: 'Prosciutto Garland',
    image: '/img/ProsciuttoArugula.png',
    alt: 'Prosciutto Garland pizza',
    align: 'right',
    description:
      'Paper-thin prosciutto ribbons, peppery rocket and shaved Parmesan on a light tomato base—like a savory garland across a festive table.',
  },
  {
    id: 5,
    name: 'Hawaiian Snowfall',
    image: '/img/Hawaiian.png',
    alt: 'Hawaiian Snowfall pizza',
    align: 'left',
    description:
      'Smoky ham, sweet pineapple and a blanket of mozzarella, for those who like their Christmas with a little sunshine.',
  },
  {
    id: 6,
    name: 'Veggie Carol',
    image: '/img/Veggie.png',
    alt: 'Veggie Carol pizza',
    align: 'right',
    description:
      'Roasted peppers, courgette, red onion and olives on a tomato base—colourful, cozy and completely meat-free.',
  },
  {
    id: 7,
    name: 'BBQ Reindeer Tracks',
    image: '/img/BBQChicken.png',
    alt: 'BBQ Reindeer Tracks pizza',
    align: 'left',
    description:
      'Smoky BBQ chicken, red onion and a drizzle of sauce leaving a trail across melted cheese, like hoofprints in fresh snow.',
  },
  {
    id: 8,
    name: 'Supreme Sleigh Feast',
    image: '/img/Supreme.png',
    alt: 'Supreme Sleigh Feast pizza',
    align: 'right',
    description:
      'Pepperoni, sausage, peppers, onions and olives loaded onto a hearty base—the everything-on-it slice for the hungriest elves.',
  },
  {
    id: 9,
    name: 'Capricciosa Carol',
    image: '/img/Capricciosa.png',
    alt: 'Capricciosa Carol pizza',
    align: 'left',
    description:
      'Artichokes, ham, mushrooms and olives come together like a choir of classic Italian flavors around the tree.',
  },
  {
    id: 10,
    name: 'Meat Lovers Midnight',
    image: '/img/MeatLovers.png',
    alt: 'Meat Lovers Midnight pizza',
    align: 'right',
    description:
      'A rich medley of cured meats piled high for late-night feasts and post-carol hunger.',
  },
  {
    id: 11,
    name: 'Starlit Napoli',
    image: '/img/Napoli.png',
    alt: 'Starlit Napoli pizza',
    align: 'left',
    description:
      'Anchovies, capers and olives on a bright tomato base—salty, briny, and perfect for those who like their Christmas stories a little bolder.',
  },
  {
    id: 12,
    name: 'Fireplace Neapolitan',
    image: '/img/Neapolitan.png',
    alt: 'Fireplace Neapolitan pizza',
    align: 'right',
    description:
      'Charred, airy crust, simple toppings and a soft centre—the kind of slice you eat slowly while staring at the flicker of fairy lights.',
  },
];

export default function Menu() {
  usePageTitle('Menu');

  return (
    <div className="min-h-screen bg-[#7f2615] text-[#fef3c7] px-6 py-12 md:py-16">
      <div className="max-w-5xl mx-auto flex flex-col gap-10">
        <header className="text-center space-y-3">
          <h1 className="text-4xl md:text-5xl font-extrabold tracking-wide">
            Merry Crustmas Menu
          </h1>
          <p className="text-lg md:text-xl max-w-2xl mx-auto leading-relaxed">
            From cozy classics to North Pole specials, pick the slice that feels most like your
            holiday.
          </p>
        </header>

        <section className="flex flex-col gap-8 md:gap-10">
          {pizzas.map((pizza) => {
            const isRight = pizza.align === 'right';
            return (
              <article
                key={pizza.id}
                className={`bg-black/20 rounded-3xl overflow-hidden shadow-xl border border-[#fef3c7]/10 md:max-w-4xl ${
                  isRight ? 'md:self-end' : 'md:self-start'
                }`}
              >
                <div className="md:grid md:grid-cols-2 md:items-stretch">
                  <img
                    src={pizza.image}
                    alt={pizza.alt}
                    className={`w-full h-full object-cover block ${
                      isRight ? 'md:order-2' : ''
                    }`}
                  />
                  <div
                    className={`px-6 py-6 md:py-8 space-y-3 flex flex-col justify-center ${
                      isRight ? 'md:order-1' : ''
                    }`}
                  >
                    <h2 className="text-3xl md:text-4xl font-extrabold tracking-wide">
                      {pizza.name}
                    </h2>
                    <p className="text-lg md:text-xl leading-relaxed">{pizza.description}</p>
                  </div>
                </div>
              </article>
            );
          })}
        </section>
      </div>
    </div>
  );
}

