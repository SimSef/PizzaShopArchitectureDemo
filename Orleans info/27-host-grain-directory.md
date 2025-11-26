# Host: Grain Directory

## What it is

- Maps `GrainId` to the silo where its activation lives.
- Ensures at most one activation per grain id at a time (aside from rare duplicates under failure).
- On activation, a grain registers itself so calls route to the correct silo.

## Default directory (recommended)

- Built-in distributed in-memory directory:
  - Partitioned across all silos (similar to a DHT).
  - Eventually consistent:
    - Under heavy churn/failures you might see occasional duplicate activations.
- No extra infrastructure and minimal configuration.
- Recommended default for almost all grain types.

## Pluggable / storage-based directories

- Orleans can use external storage for the directory, such as:
  - Azure Table directory (`Microsoft.Orleans.GrainDirectory.AzureStorage`).
  - Redis directory (`Microsoft.Orleans.GrainDirectory.Redis`).
- You can:
  - Choose a directory implementation per grain type.
  - Register multiple directories and map different grain classes to different ones.

## When to consider a storage-based directory

- Start with the default in-memory directory for everything.
- Consider Redis/Azure Table directory for a small set of critical grains when:
  - You need stronger “exactly one activation” behavior:
    - Duplicate activations would be very bad (high-value side effects, money, external commands).
  - You want to reduce deactivation/rehoming:
    - Long-lived grains with large state or expensive startup where stable placement matters.

Suggested approach:

1. Use the default directory for all grains initially.
2. Identify high-value, long-lived, expensive grains.
3. Move only those to a Redis/AzureTable directory; observe behavior, expand only if needed.

## Mental model

- Default directory:
  - Fast, simple, zero additional infra, good enough for most apps, accepts rare duplicates during turbulence.
- Storage-based directory (Redis/Azure Table):
  - More infra and config, slightly more overhead, but provides stronger guarantees and stability for selected important grains.

