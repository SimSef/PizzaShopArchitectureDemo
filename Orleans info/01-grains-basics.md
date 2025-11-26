# Grains: Interface, Class, Timeouts, Lifecycle

## Grain interface

- Inherits `IGrainWithXKey` (identity type).
- All methods must return `Task`, `Task<T>`, or `ValueTask<T>` (no `void` or synchronous returns).

## Grain class

- Inherits `Grain`, implements grain interfaces.
- Holds state in fields/properties like a normal C# class.

## Response timeouts

- Per-method timeout:
  - Apply `[ResponseTimeout("hh:mm:ss")]` on the **interface method**.
  - Exceeding the timeout results in a `TimeoutException`.
- Default timeout:
  - Configure via `ClientMessagingOptions.ResponseTimeout` or `SiloMessagingOptions.ResponseTimeout`.
  - Default is 30 seconds.

## Async patterns

- Synchronous work with async signature:
  - Return value: `Task.FromResult(result)`.
  - No value: `Task.CompletedTask`.
- You can `return OtherAsyncCall();` directly if no extra logic is needed.
- `ValueTask<T>` is allowed for performance-sensitive paths.

## Grain references (proxies)

- Created via:
  - Inside a grain: `GrainFactory.GetGrain<T>(key)`.
  - From a client: `client.GetGrain<T>(key)`.
- Represent **(type + key)** identity, not an in-memory instance.
- Location-transparent, survive restarts, and can be passed around and persisted.

## Calling grains

- Calls are asynchronous messages:
  - `await grain.SomeMethod();`
  - Fan-out/fan-in via `Task.WhenAll(tasks)` for parallel calls.

## Error behavior

- Exceptions propagate to the caller (exception type must be serializable).
- Unknown exception types are wrapped in `UnavailableExceptionFallbackException`.
- Exceptions do **not** deactivate the grain, except `InconsistentStateException` (storage/state issues).

## Lifecycle hooks

- `OnActivateAsync`:
  - Called on activation; use for initialization.
  - Throwing from here fails activation.
- `OnDeactivateAsync`:
  - Best-effort cleanup; **not guaranteed** to run.
  - Do not rely on it for critical persistence.

