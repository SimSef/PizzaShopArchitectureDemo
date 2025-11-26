# External Tasks and the Orleans Scheduler

## Core idea

- Each grain activation runs on a single-threaded Orleans scheduler.
- Tasks kicked off in grain code normally stay on that scheduler, keeping state access safe and single-threaded.

## What stays in grain context (safe by default)

These respect the current scheduler and stay on the Orleans scheduler when used from grains:

- `await` in normal async methods.
- `Task.Delay`, `Task.WhenAny`, `Task.WhenAll`.
- `Task.ContinueWith` (without a custom scheduler).
- `Task.Factory.StartNew` (with the default scheduler).
- `Task.Factory.StartNew(WorkerAsync).Unwrap()` for async delegates.

Use these for normal grain logic.

## Escaping to the thread pool

Sometimes you must run blocking synchronous code (legacy APIs, blocking I/O). Running it directly in a grain will block the scheduler.

- Use `Task.Run(...)` to offload blocking work:
  - Code inside `Task.Run` runs on the .NET thread pool (`TaskScheduler.Default`), outside Orleans.
  - After `await Task.Run(...)`, execution resumes on the grain scheduler.
- `ConfigureAwait(false)` also escapes to the thread pool:
  - Do **not** use `ConfigureAwait(false)` in grain code.

Rule of thumb:

- Use `Task.Run` only to wrap blocking synchronous calls.
- Do not use `Task.Run` for normal async APIs (`FooAsync()`).

## Async libraries

- Async libraries may use `ConfigureAwait(false)` internally; that is fine.
- From grain code, just `await myLibrary.FooAsync()` normally.
- Use `Task.Run` only if the library is truly synchronous and blocking.

## Things to absolutely avoid in grains

- Never synchronously block on tasks:
  - `Task.Wait()`, `.Result`, `Task.WaitAll(...)`, `Task.WaitAny(...)`, `.GetAwaiter().GetResult()`.
- Avoid `async void` (including patterns that become `async void`, like `list.ForEach(async x => ...)`).

Why:

- Grains are single-threaded; blocking can cause deadlocks and thread-pool starvation.
- `async void` can surface unhandled exceptions that crash the process.

## Quick cheat sheet

- Normal grain work → use `async/await` normally.
- Run async worker but stay on grain scheduler → `Task.Factory.StartNew(WorkerAsync).Unwrap()`.
- Run blocking legacy sync work → `await Task.Run(() => BlockingCall());`.
- Timeouts → `Task.Delay` + `Task.WhenAny`.
- Call async library method → `await lib.FooAsync()`.
- `ConfigureAwait(false)` → OK inside libraries, not in grain code.

