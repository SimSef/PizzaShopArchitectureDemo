# Stateless Worker Grains

## For CPU / high-throughput work

### What they are

- Grains marked with `[StatelessWorker]`.
- Orleans can create many activations per grain type + key:
  - Multiple per silo.
  - Across all silos.
- Calls are routed to any available activation; the caller does not care which one.

## Why they are good for CPU-heavy work

- Designed for functional-style work:
  - “Do this computation for me.”
  - Not strongly tied to a specific entity identity (`User 123`, `Order 555`, etc.).
- Orleans:
  - Keeps workers local to the calling silo when possible (avoids extra network hops).
  - Auto-scales the number of activations per silo based on load (up to `maxLocalWorkers`).
  - Deactivates idle workers when load drops.

Ideal when you have:

- CPU-bound operations (heavy calculations).
- High request volume.
- No strict per-identity state requirement.

## Typical use cases

- Request preprocessing:
  - Decompress/decrypt/validate/normalize payloads.
- Intensive compute:
  - Scoring, matching, pricing, recommendation calculations.
- Hot paths:
  - Frequently-called helpers that should run locally and scale out automatically.
- Pre-aggregation:
  - Local reduce/aggregate per silo before sending to a central grain.

## State and identity

- Stateless workers can hold state, but:
  - Multiple activations exist.
  - State is not coordinated between them.
- Usually treat them as stateless-ish workers operating on arguments and maybe a local cache.

## Calling pattern

- Typically use a dummy key for a pool:

```csharp
var worker = GrainFactory.GetGrain<IMyWorkerGrain>(0);
await worker.ProcessAsync(args);
```

- Same key = same pool of workers, auto-scaled by Orleans.

