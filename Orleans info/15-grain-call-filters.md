# Grain Call Filters

## What they are

- Middleware around grain calls that can run before and after grain methods.
- Can:
  - Read/modify arguments.
  - Read/modify results.
  - Use `RequestContext`.
  - Inspect method metadata.
  - Catch and transform exceptions.

There are two types:

- Incoming filters (on the silo where the grain runs).
- Outgoing filters (on the caller: client or silo).

## Incoming filters (server-side)

- Run when a grain receives a call.
- Typical uses:
  - Authorization (check attributes such as `[AdminOnly]`, inspect `RequestContext`).
  - Logging/metrics (method, args, duration, result/exception).
  - Exception shaping (wrap internal exceptions into safer types).
- Can be:
  - Global (registered in DI, affects all grains).
  - Per-grain (grain implements the filter interface, affects only that grain).
- Order:
  1. Global filters (in registration order).
  2. Grain-level filter.
  3. Grain method.

## Outgoing filters (caller-side)

- Run when a client/silo makes a grain call.
- Typical uses:
  - Set correlation IDs or auth info into `RequestContext`.
  - Client-side logging/metrics.
  - Opt-in flags for server behavior.
- Can be registered on:
  - Silo builder.
  - Client builder.

## Key use cases to remember

- Authorization:
  - Check `RequestContext` and method attributes; throw if not allowed.
- Telemetry:
  - Timing, logging, tracing around every call.
- Exception conversion:
  - On the silo, convert unknown exceptions to a safe base type so clients can handle/log them.
- Side-effect hooks:
  - Filters can call other grains (for audit, etc.), but must avoid recursion loops.

