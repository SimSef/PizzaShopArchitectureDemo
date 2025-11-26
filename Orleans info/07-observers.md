# Observers

## What they are

- Let Orleans **push notifications** to clients or grains.
- Observer interface example:
  - `interface IChat : IGrainObserver { Task ReceiveMessage(...); }`
- Use `[OneWay]` on methods for fire-and-forget notifications.

## Client observer pattern

1. Define `IGrainObserver` interface (for example, `IChat`).
2. Client implements it: `class Chat : IChat { ... }`.
3. Client creates an observer reference:
   - `var obj = grainFactory.CreateObjectReference<IChat>(chatInstance);`
4. Grain exposes `Subscribe/Unsubscribe` and keeps observers in an `ObserverManager<T>`.
5. Grain notifies observers:
   - `_subsManager.Notify(o => o.ReceiveMessage(msg));`

## Grain as observer

- Grains can implement `IGrainObserver` too.
- Subscribe using `this.AsReference<IMyObserver>()` (no `CreateObjectReference` needed).

## Important details

- `CreateObjectReference` uses `WeakReference`:
  - Keep a strong reference to the observer object or it may be garbage-collected.
- Observers are **unreliable**:
  - Client restarts → new observer id.
  - `ObserverManager<T>` expects periodic resubscription and cleans up inactive observers.
- Execution model:
  - Per observer instance, calls are serialized (non-reentrant).
  - Attributes like `Reentrant` do not affect observers.

