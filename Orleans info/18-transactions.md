# Transactions

## 1. What they are

- Distributed ACID transactions over grain state.
- Let you update multiple grains atomically (all commit or all roll back).
- Used to enforce cross-grain invariants (for example, money transfers between accounts).

## 2. Enabling transactions

- On the silo:
  - Call `UseTransactions()` on the silo builder.
- On the client:
  - Call `UseTransactions()` on the client builder.
- Configure transactional storage (`ITransactionalStateStorage<T>`), for example, Azure Table.
- Dev-only:
  - Can bridge to normal grain storage, but this is slower and not future-proof.

## 3. Marking transactional methods (interfaces)

- Use `[Transaction(TransactionOption.X)]` on grain interface methods.
- Options:
  - `Create` – always starts a new transaction.
  - `Join` – must be called inside an existing transaction.
  - `CreateOrJoin` – join if one exists; otherwise create a new one.
  - `Suppress` – runs outside any transaction.
  - `Supported` – non-transactional itself, but sees transaction context if present.
  - `NotAllowed` – throws if called inside a transaction.

Typical pattern:

- Top-level commands (for example, `Transfer`) → `Create`.
- Inner operations (`Withdraw`, `Deposit`) → `Join`.
- Queries (`GetBalance`) → `CreateOrJoin`.

## 4. Transactional state in grains

- Use `ITransactionalState<TState>` instead of `IPersistentState<TState>`.
- Inject via `[TransactionalState("stateName", "storeName")]` on a constructor parameter.
- Access via:
  - `PerformRead(func)` – read inside a transaction.
  - `PerformUpdate(func)` – mutate inside a transaction.
- `TState` must be serializable (with `GenerateSerializer`, `Id` attributes, etc.).
- Grains using transactional state should be marked `[Reentrant]`.

## 5. How to call transactional methods

### From a client (recommended)

- Use `ITransactionClient.RunTransaction(option, async () => { ... })`.
- Inside the delegate, call transactional grain methods:
  - `await from.Withdraw(amount);`
  - `await to.Deposit(amount);`
- All those calls run in one transaction.

### From another grain

- Call transactional methods normally:
  - Outer call marked as `Create`.
  - Inner calls marked as `Join`.
- Orleans propagates the transaction context automatically.

## 6. Errors and retries

- On failure, you see Orleans transaction exceptions:
  - `OrleansTransactionAbortedException` → transaction definitely aborted; safe to retry.
  - Other exceptions → transaction state might be unknown; be cautious and possibly delay retries.
- Application exceptions inside a transaction become the `InnerException` of the transaction exception.

## 7. When to use transactions vs normal persistence

- Use transactions when:
  - You have invariants across multiple grains (no overdrafts, consistent order/payment/inventory).
  - Partial updates are unacceptable and you need strong guarantees.

- Skip transactions (use normal persistence and design) when:
  - Only one grain is involved.
  - Eventually consistent behavior is acceptable.
  - You just need simple state saves and idempotent logic.

