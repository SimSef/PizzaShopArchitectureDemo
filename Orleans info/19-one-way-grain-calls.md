# One-Way Grain Calls

## What they are

- “Fire-and-forget” grain calls:
  - Caller sends a message and does not wait for completion or errors.
- Marked with an attribute on the grain interface method (for example, `[OneWay]`).

## Behavior

- Caller returns immediately after sending.
- No response message is sent back.
- Caller:
  - Does not know when/if the grain processed the call.
  - Never sees exceptions from that method.
  - Cannot get a return value (method is conceptually `void`).

## Method shape

- Must return `Task` or `ValueTask` (non-generic).
- Cannot return `Task<T>` or `ValueTask<T>` for one-way calls.

## When it is useful

- High-volume “best-effort” notifications such as:
  - Telemetry/metrics.
  - “Just letting you know this happened” events.
- When:
  - You do not care about success/failure of each call.
  - You want to avoid reply/serialization overhead.

## Risks and trade-offs

- No reliability guarantees for the caller:
  - Message might be lost and you will not know.
- No error handling at the caller side.
- Harder to reason about correctness and debugging.

## Rule of thumb

- Default: use normal (two-way) async grain calls.
- Only use one-way calls for carefully chosen, non-critical, performance-sensitive notifications.

