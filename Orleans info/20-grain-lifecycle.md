# Grain Lifecycle

## What it is

- Each grain has an observable lifecycle: an ordered pipeline of stages when a grain:
  - Activates.
  - (Sometimes) deactivates.
- Grains and components can hook into these stages to run logic at the right time.

## Built-in lifecycle stages

Stages are integer priorities; key ones:

- `First`:
  - Earliest stage (system/framework hooks).

- `SetupState` (1,000):
  - Grain state is prepared here.
  - For persistent grains, Orleans loads state from storage into `State`.
  - After this, state is ready to read.

- `Activate` (2,000):
  - Orleans calls `OnActivateAsync` on activation.
  - Orleans calls `OnDeactivateAsync` on deactivation.

- `Last`:
  - Final lifecycle stage (end-of-life cleanup hooks).

> Deactivation stages are not guaranteed to run (for example, on silo crash), so they cannot be relied on for critical guarantees.

## How app code participates

There are two main ways:

### 1. Grain participates in its own lifecycle

- Grains override `Participate(IGrainLifecycle lifecycle)`.
- Inside, they subscribe to stages with callbacks.
- Example scenarios:
  - Run custom logic after state is loaded but before activation logic.
  - Initialize timers/streams/caches in well-defined stages.
  - Split heavy initialization into phases instead of one large `OnActivateAsync`.

### 2. Components hook into the grain lifecycle

- Each grain has an `IGrainContext`, which exposes `ObservableLifecycle`.
- Orleans creates the context and lifecycle before constructing the grain.
- DI-injected components can:
  - Access `IGrainContext`.
  - Subscribe to lifecycle stages (such as `Activate`, `Last`).
  - Run hooks when the grain reaches those stages.

Pattern:

- Component implements `ILifecycleParticipant<IGrainLifecycle>`.
- In `Participate`, it subscribes to stages like `SetupState`, `Activate`, etc.
- Registered via DI with access to `IGrainContext`.
- Any grain injecting this component automatically gains that behavior without extra lifecycle code.

Use cases:

- Reusable “features”:
  - Auto-subscribe to a stream on `Activate`.
  - Periodic timers starting after state is loaded.
  - Logging/metrics hooks per grain type.
  - Background helpers aligned with grain lifecycle.

## Mental model

> A grain’s activation is a mini pipeline:
>
> `First → SetupState → Activate → Last`.
>
> The grain and injected components can subscribe to these stages and run logic there.

