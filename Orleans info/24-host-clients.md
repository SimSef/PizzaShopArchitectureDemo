# Hosts and Clients

## What is a client?

- Any non-grain code that talks to grains or streams:
  - Web apps.
  - Worker services.
  - CLI tools, tests, etc.

## 1. Co-hosted client (recommended)

- Runs in the same process as the silo.
- Obtain the client via DI and call grains directly.

Pros:

- Fewer network hops and lower latency.
- Simpler deployment and configuration.

Cons:

- No isolation:
  - Bad client code (blocking I/O, CPU hogs) can hurt grains running in the same process.

## 2. External client

- Runs in a separate process (for example, frontend web server).
- Connects to the cluster over the network.
- You configure clustering and then connect, using it like any other grain caller.

Considerations:

- Must handle connect/retry logic.
- Need to handle `SiloUnavailableException` and transient failures.

## 3. Calling grains from a client

- Same pattern as inside grains:
  - Get a grain reference.
  - Call async methods and `await` them (do not block).
- Clients are not single-threaded by design:
  - You are responsible for managing concurrency on the client side.

## 4. Getting data back to clients

- Observers:
  - One-way, best-effort callbacks (no delivery guarantees).
- Streams:
  - Async, can report success/failure.
  - Better for more reliable, push-style updates.

*** Add File: Orleans info/25-host-silo-lifecycle.md
# Host: Silo Lifecycle

## 1. What is the silo lifecycle?

- Ordered startup and shutdown pipeline for a silo.
- System and app components subscribe to lifecycle stages rather than a single “startup” method.
- Used by Orleans internals and your own services.

## 2. Lifecycle stages (ordered)

From first to last:

1. `First`
   - Earliest possible stage (reserved, rarely used directly).
2. `RuntimeInitialize`
   - Core runtime initialization (threading, environment).
3. `RuntimeServices`
   - Networking and Orleans internal agents.
4. `RuntimeStorageServices`
   - Storage-related runtime components.
5. `RuntimeGrainServices`
   - Grain-level system services: membership, grain directory, grain services.
6. `ApplicationServices`
   - Application-level services (DI-registered components).
7. `BecomeActive`
   - Silo joins the cluster / becomes ready.
8. `Active`
   - Silo is fully active and serving grain traffic.
9. `Last`
   - Final teardown stage.

## 3. Logging

- Orleans logs who participates in which stage and how long each stage took.
- Logger category: `Orleans.Runtime.SiloLifecycleSubject`.
- Helps diagnose:
  - Startup order.
  - Slow components.
  - Failures per stage.

## 4. How your code hooks into the lifecycle

- Implement `ILifecycleParticipant<ISiloLifecycle>`.
- At startup, Orleans:
  - Resolves all `ILifecycleParticipant<ISiloLifecycle>` from DI.
  - Calls `Participate(...)` on each so they can subscribe to stages.

Typical pattern:

- Component implements `ILifecycleParticipant<ISiloLifecycle>`.
- In `Participate(lifecycle)`:
  - Call `lifecycle.Subscribe(...)`.
  - Provide:
    - A stage (for example, `ServiceLifecycleStage.ApplicationServices` or `ServiceLifecycleStage.Active`).
    - An async callback to run at that stage.

## 5. Startup tasks (replacement for bootstrap providers)

- Old bootstrap providers are replaced by lifecycle-based startup tasks.
- Startup task:
  - A small class which:
    - Implements `ILifecycleParticipant<ISiloLifecycle>`.
    - Subscribes an async function to a lifecycle stage (often `Active`).
- Register it in DI as `ILifecycleParticipant<ISiloLifecycle>`, often via an extension method on `ISiloHostBuilder`.

### Mental model

> The silo walks through stages. At each stage, Orleans calls everyone who subscribed there. Your services just say: “At stage X, run this async setup/cleanup logic.”

*** Add File: Orleans info/26-host-heterogeneous-silos.md
# Host: Heterogeneous Silos

## What it is

- A single Orleans cluster where different silos host different grain types (implementations).
- All silos still share the same grain interfaces.

## Good use cases

Use only when some grains must run in special environments:

- Special dependencies:
  - Native libraries, GPU libraries, unusual SDKs, COM, etc.
  - You do not want these on every silo.
- Different hardware tiers:
  - “Normal” grains on standard nodes.
  - Heavy ML/image/analytics grains on GPU or high-RAM nodes.
- Security/compliance:
  - Payment or highly sensitive grains restricted to hardened silos.
  - Other grains run on general-purpose silos.
- Migration/experiments:
  - Legacy or experimental grain types hosted only on a subset of silos.

## Downsides and limits

- More deployment and operational complexity.
- Not supported with:
  - Stateless worker grains.
  - Implicit stream subscriptions (must use explicit subscriptions).
- Clients are not auto-aware:
  - If no silo supports a grain type, calls result in runtime exceptions.

## Rule of thumb

- For typical apps (web, e-commerce, SaaS):
  - Do not use heterogeneous silos.
- Prefer all silos hosting all grain types unless you truly need specialized hardware/dependencies/security for some grains.

*** Add File: Orleans info/27-host-grain-directory.md
# Host: Grain Directory

## What it is

- Maps `GrainId` to the silo where its activation lives.
- Ensures at most one activation per grain id at a time (aside from rare duplicates under failure).
- On activation, a grain registers itself so calls route to the correct silo.

## Default directory (recommended)

- Built-in distributed in-memory directory:
  - Partitioned across all silos (similar to a DHT).
  - Eventually consistent:
    - Under heavy churn/failures you might see occasional duplicate activations.
- No extra infrastructure and minimal configuration.
- Recommended default for almost all grain types.

## Pluggable / storage-based directories

- Orleans can use external storage for the directory, such as:
  - Azure Table directory (`Microsoft.Orleans.GrainDirectory.AzureStorage`).
  - Redis directory (`Microsoft.Orleans.GrainDirectory.Redis`).
- You can:
  - Choose a directory implementation per grain type.
  - Register multiple directories and map different grain classes to different ones.

## When to consider a storage-based directory

- Start with the default in-memory directory for everything.
- Consider Redis/Azure Table directory for a small set of critical grains when:
  - You need stronger “exactly one activation” behavior:
    - Duplicate activations would be very bad (high-value side effects, money, external commands).
  - You want to reduce deactivation/rehoming:
    - Long-lived grains with large state or expensive startup where stable placement matters.

Suggested approach:

1. Use the default directory for all grains initially.
2. Identify high-value, long-lived, expensive grains.
3. Move only those to a Redis/AzureTable directory; observe behavior, expand only if needed.

## Mental model

- Default directory:
  - Fast, simple, zero additional infra, good enough for most apps, accepts rare duplicates during turbulence.
- Storage-based directory (Redis/Azure Table):
  - More infra and config, slightly more overhead, but provides stronger guarantees and stability for selected important grains.

