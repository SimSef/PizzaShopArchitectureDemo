# Request Context

## What it is

- A per-request key-value bag that flows with Orleans calls:
  - Client → grain → grain → …
- Backed by async-local data.
- Used via the static `RequestContext` API (set/get).

## How it behaves

- Metadata is captured when a request is sent and restored on the callee.
- Flows downstream with requests, not back with responses.
- Async continuations (`await`, `ContinueWith`, etc.) see the context captured when they were scheduled.
- If a grain does not modify `RequestContext`, its outgoing calls inherit the same metadata.

## What to put in it

- Light, serializable metadata such as:
  - Trace / correlation ID.
  - Tenant ID.
  - User ID.
  - Debug flags.
- Avoid large or complex objects because of serialization overhead.

## When to use it

- You want cross-grain / cross-silo metadata without adding parameters to every method.
- You want consistent logging, tracing, or multi-tenant behavior across a call chain.

