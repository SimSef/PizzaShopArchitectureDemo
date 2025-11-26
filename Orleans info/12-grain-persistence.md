# Grain Persistence

## What it is

- Each grain can have one or more named state objects.
- State lives:
  - In-memory while the grain is active.
  - Persisted via storage providers (Cosmos, SQL, Blob, DynamoDB, etc.).
- State is:
  - Auto-loaded on activation.
  - Explicitly persisted when you call the write method.

## Goals

- Multiple states per grain (for example, `profile`, `cart`).
- Multiple storage providers with different backends/configs.
- Pluggable model (`IGrainStorage`) so you can build custom providers.
- Orleans is not an ORM; providers decide how to map state to storage.

## How grains use persistence

- Use `IPersistentState<TState>` (injected):
  - `State` → in-memory state object.
  - `WriteStateAsync()` → persist state.
  - `ReadStateAsync()` → reload (only needed if storage changed externally).
  - `ClearStateAsync()` → delete/clear state.
  - `Etag`, `RecordExists` for concurrency control.

## Failure semantics

- Initial load failure:
  - Grain activation fails and the caller gets an exception.
- Read or write failure:
  - The async operation faults; the grain can catch and handle it.
- Concurrency conflicts:
  - `InconsistentStateException` based on Etags.

## Providers to remember

- Azure Cosmos DB.
- Relational storage via ADO.NET.
- Azure Storage (Blob/Table).
- Amazon DynamoDB.

## Legacy vs recommended

- Recommended:
  - Inject `IPersistentState<T>` with a named provider.
- Legacy:
  - Derive from `Grain<TState>` and use `[StorageProvider]` attribute.
  - Still works but is older style.

