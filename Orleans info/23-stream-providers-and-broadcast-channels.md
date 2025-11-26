# Stream Providers and Broadcast Channels

## Stream providers (where data flows)

- Streams are backed by pluggable providers, for example:
  - Dev/in-memory providers (used in quickstarts).
  - Azure Queue, Azure Event Hubs.
  - Kafka or other systems via adapters.
- You configure a named stream provider on:
  - Silo (server side).
  - Client (Blazor app, console, etc.).

## Providers and capabilities

### Memory streams

- In-memory only, very simple.
- Use for:
  - Local development.
  - Tests and demos.
- Not durable and not for production.

### Azure Queue streams

- Based on Azure Storage Queues.
- At-least-once delivery; ordering not strictly guaranteed under failures.
- Use for:
  - Simple durable messaging.
  - Small/medium throughput where strict ordering is not required.

### Azure Event Hubs streams

- High-throughput event ingestion with partitions and replay/rewind.
- Use for:
  - Telemetry, IoT, and big event pipelines.
  - Scenarios that need replay from history.

### PersistentStreamProvider + queue adapters

- General plumbing for building custom providers on top of any queue technology (Kafka, RabbitMQ, etc.).
- Use for:
  - Custom or non-Azure brokers.

## Pub/Sub and storage

- Orleans manages a PubSub grain backed by `PubSubStore`:
  - Tracks which consumers are subscribed to which stream.
  - Backed by configurable storage (memory, Azure Table, etc.).
- The streaming runtime:
  - Manages activation, delivery, retries, and cleanup of unused streams.

## Broadcast channels vs streams

### Broadcast channels

- Lightweight in-cluster broadcast mechanism.
- No persistence and no replay; not a full stream provider.
- Decouples producers and consumers:
  - Producers publish updates on a channel name.
  - Any grain that cares can listen and react.
- Typical uses:
  - Stock price tickers.
  - Config/feature flag changes.
  - Cache invalidation (“invalidate cache for X”).
  - “Model/read side changed, refresh your view.”
- You care about latest updates, not history.

### Streams vs broadcast channels

- Streams:
  - General event pipeline, often backed by queues/Kafka/etc.
  - Can be persistent and replayable.
  - Often per-entity, using IDs in the `StreamId`.

- Broadcast channels:
  - In-memory broadcast within the cluster.
  - No history; pure fire-and-forget fan-out.
  - One producer → many subscribers in real time.

## Rules of thumb

- Use Kafka/Event Hubs/Spark for:
  - Big, uniform data processing and analytics pipelines.
- Use Orleans streams for:
  - Per-user/per-entity event-driven logic.
  - Stateful, dynamic, “mini services” as grains.
- Use broadcast channels for:
  - “Tell everyone who cares about X, right now.”
  - No need for persistence or replay, only live push.

