# Event Sourcing

## What it is

- Alternative persistence model where grains store events instead of just snapshots.
- Grain derives from `JournaledGrain<TState, TEvent>`:
  - `State`: current confirmed state (read-only).
  - `Version`: number of confirmed events.
- You raise events; Orleans applies them via your transition logic.

## Core concepts

- `RaiseEvent` / `RaiseEvents`:
  - Append events to the log.
- `ConfirmEvents`:
  - Waits until events are persisted and become official.
- Transition methods:
  - `Apply(Event)` or `TransitionState` define how events update state.
- `RetrieveConfirmedEvents`:
  - Get past events (only supported by some providers).

## Confirmation modes

- Immediate:
  - Raise event and call `ConfirmEvents()` before method returns.
  - No reentrancy.
  - Strong consistency, lower throughput, more sensitive to storage issues.

- Delayed:
  - Events can be unconfirmed for a while.
  - `UnconfirmedEvents` + `TentativeState` represent current best-guess state.
  - Higher availability/throughput, but state may roll back or reorder.

## Replication and conflicts

- Multiple instances can exist (multi-cluster scenarios).
- Log-consistency providers ensure they agree on the same event sequence.
- `RaiseConditionalEvent`:
  - Only appends an event if the version still matches (ETag-like).
  - Avoids conflicting events (for example, double withdrawals).

## Notifications and diagnostics

- `OnStateChanged()`:
  - Called when confirmed state changes.
- `OnTentativeStateChanged()`:
  - Called when tentative state changes (includes unconfirmed events).
- Connection issues:
  - Monitored via `OnConnectionIssue` and `OnConnectionIssueResolved`.

## Log-consistency providers

- `StateStorage`:
  - Persist snapshots only.
  - No event retrieval; state is read/written as a whole.
- `LogStorage`:
  - Persist full event list.
  - Supports event retrieval; suitable only for short streams/tests.
- `CustomStorage`:
  - You implement the storage interface and can store snapshots and/or events however you want.

## When to use

- You need:
  - Event history / audit.
  - Multi-cluster replicated grains.
  - Conflict-aware updates (conditional events).
  - Integration with event stores.
- Not needed for most simple CRUD-like grain scenarios.

