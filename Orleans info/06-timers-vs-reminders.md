# Timers vs Reminders

## Timers (`RegisterGrainTimer`)

- In-memory, bound to a single grain activation.
- Stop when:
  - The activation deactivates.
  - The silo crashes.
  - You dispose them.
- Period is measured after the callback finishes, so callbacks do not overlap by default.
- Single-threaded with grain turns; interleaving is possible with `Interleave = true`.
- Best for:
  - Short intervals (seconds / tens of seconds).
  - Work that does not need to survive deactivation or restarts.

---

## Reminders (`RegisterOrUpdateReminder`)

- Persistent, storage-backed (for example, Azure Table, SQL, in-memory for dev).
- Bound to grain identity, not a specific activation.
- Survive:
  - Deactivation.
  - Silo or cluster restarts (missed ticks during downtime are skipped).
- Grain implements `IRemindable.ReceiveReminder(...)` to receive reminder callbacks.
- Best for:
  - Infrequent tasks (minutes/hours/days).
  - Behavior that must wake the grain back up later.

---

## When to choose which

- Use a **Timer** when:
  - It is acceptable for it to stop with the activation or failures.
  - You need fine-grained intervals (seconds).

- Use a **Reminder** when:
  - It must survive deactivation and restarts.
  - The period is in minutes/hours/days.

---

## Combo pattern

- Reminder acts as a durable wake-up (for example, every 5 minutes).
- In `ReceiveReminder`, the grain (re)starts a local Timer with a small period.

