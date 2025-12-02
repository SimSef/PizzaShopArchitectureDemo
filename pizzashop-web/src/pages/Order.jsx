import React, { useEffect, useMemo, useState } from 'react';
import { usePageTitle } from '../hooks/usePageTitle';
import { useAuth } from '../auth/AuthContext';
import { BFF_BASE_URL } from '../bffConfig';
import { pizzas } from './Menu';

export default function Order() {
  usePageTitle('Order');
  const { isLoading, isAuthenticated } = useAuth();

  const [quantities, setQuantities] = useState(() => {
    const initial = {};
    pizzas.forEach((pizza) => {
      initial[pizza.id] = 0;
    });
    return initial;
  });

  const [isSubmitting, setIsSubmitting] = useState(false);
  const [orderResult, setOrderResult] = useState(null);
  const [error, setError] = useState(null);

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      const url = new URL(window.location.href);
      url.pathname = '/';
      url.search = '?reason=notAuthorized';
      window.location.href = url.toString();
    }
  }, [isLoading, isAuthenticated]);

  const items = useMemo(() => {
    return pizzas
      .map((pizza) => {
        const quantity = quantities[pizza.id] ?? 0;
        return {
          ...pizza,
          quantity,
          lineTotal: quantity * pizza.price,
        };
      })
      .filter((item) => item.quantity > 0);
  }, [quantities]);

  const hasItems = items.length > 0;
  const total = items.reduce((sum, item) => sum + item.lineTotal, 0);

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

  const updateQuantity = (pizzaId, delta) => {
    setQuantities((prev) => {
      const next = { ...prev };
      const current = next[pizzaId] ?? 0;
      const updated = current + delta;
      next[pizzaId] = updated < 0 ? 0 : updated;
      return next;
    });
  };

  const handleCheckout = async () => {
    if (!hasItems || isSubmitting) {
      return;
    }

    setError(null);
    setOrderResult(null);
    setIsSubmitting(true);

    try {
      const payload = {
        items: items.map((item) => ({
          pizzaId: item.id,
          name: item.name,
          quantity: item.quantity,
          unitPrice: item.price,
        })),
      };

      const delay = new Promise((resolve) => setTimeout(resolve, 2000));
      const responsePromise = fetch(`${BFF_BASE_URL}/api/orders`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include',
        body: JSON.stringify(payload),
      });

      const [response] = await Promise.all([responsePromise, delay]);

      if (!response.ok) {
        const text = await response.text().catch(() => '');
        throw new Error(text || 'Failed to create order.');
      }

      const data = await response.json();
      setOrderResult({
        orderId: data.orderId,
        total: data.total,
      });
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create order.');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="min-h-screen bg-[#7f2615] text-[#fef3c7] px-6 py-12 md:py-16">
      <div className="max-w-5xl mx-auto space-y-10">
        <header className="space-y-3 text-center md:text-left">
          <h1 className="text-3xl md:text-4xl font-extrabold tracking-wide">
            Build Your Merry Crustmas Order
          </h1>
          <p className="text-lg md:text-xl max-w-2xl leading-relaxed mx-auto md:mx-0">
            Pick your favorite pizzas, choose how many slices of cheer you want, and we&apos;ll
            whisk your order off to our magical oven.
          </p>
        </header>

        <div className="grid md:grid-cols-[minmax(0,2fr)_minmax(0,1fr)] gap-8 items-start">
          <section className="space-y-4">
            {pizzas.map((pizza) => {
              const quantity = quantities[pizza.id] ?? 0;
              return (
                <div
                  key={pizza.id}
                  className="bg-black/25 border border-[#fef3c7]/10 rounded-2xl p-4 flex items-center justify-between gap-4"
                >
                  <div className="flex items-center gap-4">
                    <img
                      src={pizza.image}
                      alt={pizza.alt}
                      className="w-16 h-16 md:w-20 md:h-20 rounded-2xl object-cover border border-[#fef3c7]/20 shadow-md shadow-black/40"
                    />
                    <div className="space-y-1">
                      <h2 className="text-lg md:text-xl font-semibold">{pizza.name}</h2>
                      <p className="text-sm md:text-base text-[#fde68a]">
                        €{pizza.price.toFixed(2)}
                      </p>
                    </div>
                  </div>
                  <div className="flex items-center gap-3">
                    <button
                      type="button"
                      onClick={() => updateQuantity(pizza.id, -1)}
                      className="w-8 h-8 flex items-center justify-center rounded-full bg-[#451a0a] border border-[#fef3c7]/20 text-lg font-bold hover:bg-[#7c2d12] transition-colors focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500/70 focus:ring-offset-[#7f2615]"
                    >
                      -
                    </button>
                    <span className="w-8 text-center font-semibold">{quantity}</span>
                    <button
                      type="button"
                      onClick={() => updateQuantity(pizza.id, 1)}
                      className="w-8 h-8 flex items-center justify-center rounded-full bg-[#b91c1c] border border-[#fef3c7]/30 text-lg font-bold hover:bg-[#ef4444] transition-colors focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500/70 focus:ring-offset-[#7f2615]"
                    >
                      +
                    </button>
                  </div>
                </div>
              );
            })}
          </section>

          <aside className="bg-black/30 border border-[#fef3c7]/10 rounded-2xl p-5 space-y-4 shadow-lg">
            <h2 className="text-xl font-semibold">Your sleigh</h2>

            {!hasItems && (
              <p className="text-sm md:text-base text-[#fde68a]">
                Start by adding at least one pizza to your sleigh to see your order summary and
                checkout.
              </p>
            )}

            {hasItems && (
              <>
                <ul className="space-y-2 text-sm md:text-base">
                  {items.map((item) => (
                    <li key={item.id} className="flex justify-between">
                      <span>
                        {item.quantity}x {item.name}
                      </span>
                      <span>€{item.lineTotal.toFixed(2)}</span>
                    </li>
                  ))}
                </ul>

                <div className="border-t border-[#fef3c7]/10 pt-3 flex justify-between items-center">
                  <span className="font-semibold">Total</span>
                  <span className="text-lg font-bold">€{total.toFixed(2)}</span>
                </div>

                <button
                  type="button"
                  onClick={handleCheckout}
                  disabled={isSubmitting}
                  className="mt-2 w-full inline-flex items-center justify-center rounded-xl bg-[#b91c1c] px-4 py-3 text-sm md:text-base font-semibold shadow-md shadow-black/40 hover:bg-[#ef4444] disabled:opacity-60 disabled:cursor-not-allowed transition-colors focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500/70 focus:ring-offset-[#7f2615]"
                >
                  {isSubmitting ? 'Baking your Merry Crustmas order…' : 'Place order'}
                </button>
              </>
            )}

            {orderResult && (
              <div className="mt-3 bg-emerald-900/50 border border-emerald-400/40 rounded-xl p-3 text-sm md:text-base">
                <p className="font-semibold">Order created! 🎄</p>
                <p className="mt-1 break-all text-xs md:text-sm">
                  Order ID: <span className="font-mono">{orderResult.orderId}</span>
                </p>
                <p className="mt-1">
                  Total:{' '}
                  <span className="font-semibold">€{Number(orderResult.total).toFixed(2)}</span>
                </p>
              </div>
            )}

            {error && (
              <p className="mt-3 text-sm text-red-200">
                {error}
              </p>
            )}
          </aside>
        </div>
      </div>
    </div>
  );
}
