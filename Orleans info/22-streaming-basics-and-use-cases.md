# Streaming Basics and Use Cases

## What it is

- Streams are virtual event pipes in Orleans.
- Identified by a `StreamId`:
  - `(namespace, GUID)` or `(namespace, string)`.
- Streams always logically exist:
  - You do not create/delete them.
  - You simply produce to them or subscribe to them.

## Producers and consumers

- Many producers and many consumers can share a stream.
- `IAsyncStream<T>` is the handle used to:
  - Produce events (`OnNextAsync`).
  - Consume events via subscriptions.
- Subscriptions are:
  - Per grain identity (survive deactivation/re-activation).
  - Independent: multiple subscriptions see events independently.

## Producing to a stream

Basic pattern:

1. Get a stream provider by name.
2. Choose a `StreamId`:
   - Namespace (for example, `"Rooms"`, `"Orders"`, `"RANDOMDATA"`).
   - ID (for example, grain GUID, user ID).
3. Send events to that stream.

Producer and consumer only need to agree on `StreamId`; they do not need to know about each other.

## Consuming via subscriptions

### Explicit subscriptions

- You manually subscribe to a stream.
- You can:
  - Unsubscribe.
  - Resume from a token (where supported).
  - Have multiple subscriptions per consumer.

### Implicit subscriptions

- Grain is marked to automatically subscribe to streams of a given namespace that match its key.
- For a stream `(namespace, grainId)`:
  - Orleans activates the grain on demand.
  - Delivers events to it automatically.
- One implicit subscription per namespace per grain; you attach handlers in `OnActivate`.

## Guarantees and semantics (high level)

- Subscription semantics:
  - After subscription completes, the consumer sees all future events.
  - Some providers support rewinding to past events using sequence tokens.
- Delivery:
  - Provider-dependent:
    - At-most-once (best effort).
    - At-least-once.
    - Exactly-once with custom providers.
- Ordering:
  - Also provider-dependent:
    - Some preserve send order.
    - Others may reorder (for example, many queue-based systems).
  - `StreamSequenceToken` can be used to reason about order and resume positions.

## Mental model

- Streams decouple producers and consumers:
  - They can live on different silos.
  - They can come and go independently.
  - Subscriptions are re-established across failures.
- Each grain can act as a tiny, stateful processor for a user/device/entity:
  - Streams deliver events into those “brains”.
  - Logic is normal C# (imperative/Rx/functional), not a rigid dataflow graph.
- Great for:
  - Per-user feeds, alerts, recommendations.
  - Fraud/cheat detection per account.
  - IoT per-device logic (one grain per sensor/device).
  - Any scenario where:
    - “What to do with this event depends on who it is for and their current state.”

