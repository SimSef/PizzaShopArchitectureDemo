# Request Scheduling and Interleaving

## What this is about

- Each grain activation is single-threaded.
- By default, one request is processed at a time from start to finish.
- Safe, but can:
  - Cause deadlocks when grains call each other in cycles.
  - Reduce throughput when waiting on I/O.
- Orleans provides ways to interleave some requests to improve liveness and performance.

## Default (non-reentrant)

- Grain does not process another request while one is in progress, even while `await`ing.
- Cycles like `A` calling `B` and `B` calling `A` can deadlock.

## Reentrancy / interleaving options

### 1. Reentrant grain (class-level)

- Mark the grain as reentrant.
- Requests to this grain can interleave while they await.
- Still single-threaded, but turns from different requests can mix.
- Tradeoff: more throughput and fewer deadlocks vs more complex state reasoning.

### 2. Always-interleaving methods (per-method)

- Mark specific methods as “always interleave”.
- These methods can run concurrently with any other request.
- Good for fast, simple, often stateless operations.

### 3. Read-only methods

- Mark methods that do not modify state as read-only.
- Multiple read-only calls to the same grain can be processed concurrently.
- Good for high-volume reads (for example, `GetX`).

### 4. Call chain reentrancy (per-call-site)

- Temporarily allow reentrancy for the current call chain.
- Handles cycles like `User → Room → User` without making everything globally reentrant.
- Fine-grained: only specific calls in that chain can reenter.

### 5. Predicate-based interleaving (`MayInterleave`)

- Grain defines a predicate that inspects each request and decides if it may interleave.
- Example: only interleave if an argument type has a specific attribute.
- Very flexible, for per-request control in advanced scenarios.

