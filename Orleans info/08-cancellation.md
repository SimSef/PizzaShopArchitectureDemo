# Cancellation

## Basics

- Grain methods can take a `CancellationToken` (last parameter, usually optional).
- Works for:
  - `Task` / `Task<T>` methods.
  - `IAsyncEnumerable<T>` streaming methods.
  - Client→grain and grain→grain calls.
- Cancellation is **cooperative**:
  - Grain must check the token or pass it to async APIs.

## Patterns

- Interface:

```csharp
Task DoWorkAsync(int x, CancellationToken cancellationToken = default);
```

- Implementation:
  - Call `cancellationToken.ThrowIfCancellationRequested();`.
  - Check the token in loops.
  - Use cancellation-aware APIs, for example `Task.Delay(ms, cancellationToken)`.

## Streaming (`IAsyncEnumerable<T>`)

- Method:

```csharp
IAsyncEnumerable<T> StreamAsync(
    int count,
    [EnumeratorCancellation] CancellationToken cancellationToken = default);
```

- Consumer:
  - Pass token directly: `await foreach (var x in grain.StreamAsync(1000, token))`.
  - Or: `.WithCancellation(token)`.

## Compatibility

- Adding a token parameter:
  - Old callers still work; grain gets `CancellationToken.None`.
- Removing a token parameter:
  - Old callers still work; extra token argument is ignored.

## Config

- `CancelRequestOnTimeout`:
  - On timeout, send cancellation to the grain.
- `WaitForCancellationAcknowledgement`:
  - Wait for grain to acknowledge cancellation (stronger ordering, slower).

## Legacy API

- `GrainCancellationToken` / `GrainCancellationTokenSource` are legacy.
- New code should prefer plain `CancellationToken`.

