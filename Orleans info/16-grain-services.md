# Grain Services

## What is a grain service?

- A long-lived, per-silo service:
  - One instance per silo, from silo startup to shutdown.
  - Remotely callable like a grain, but without a user key (`IGrainWithGuidKey`, etc.).
- Used as infrastructure/helper services for other grains.

## What they are good for

- Shared/infra logic that many grains use:
  - Sharded managers, schedulers, caches, coordination logic.
- Partitioned responsibility across silos:
  - Each silo’s service instance handles a subset of work.
- Always-in-memory silo-local state:
  - Connections, buffers, background workers.

## How they are used

- You define:
  - `IDataService : IGrainService` (service interface).
  - `DataService : GrainService, IDataService` (implementation).
  - `IDataServiceClient : IGrainServiceClient<IDataService>` (client interface).
  - `DataServiceClient` (client implementation using `GetGrainService(...)`).
- Grains inject `IDataServiceClient` and call it like a normal grain.
- The silo host registers:
  - `AddGrainService<DataService>()`.
  - `AddSingleton<IDataServiceClient, DataServiceClient>()`.

## Grain services vs reminders

### Reminders

- Per-grain scheduled callbacks (`ReceiveReminder`).
- Tied to one grain identity.
- Used to “wake this grain every X minutes/hours/days”.
- Persistence ensures they survive restarts.

### Grain services

- One instance per silo, not per grain.
- Used for:
  - Cluster infrastructure / shared logic (reminder subsystem itself, schedulers, sharded managers).
  - Things many grains call into.
  - Always-on background work per silo.

### Mental model

- Reminders → “Wake this grain periodically.”
- Grain services → “Per-silo infrastructure service grains call for shared/partitioned work.”

