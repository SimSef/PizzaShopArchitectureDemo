# Grain References and Multiple Implementations

## Grain reference

- Proxy that implements the grain interface.
- Carries: **grain type + grain key + interface**.
- Identity is `(grain type, grain key)`.
- Location-transparent, survives restarts; can be passed around and persisted.

## Getting a grain reference

- Inside a grain: `GrainFactory.GetGrain<IMyGrain>(key)`.
- From a client: `client.GetGrain<IMyGrain>(key)`.

## Multiple implementations problem

- If multiple classes implement the same interface (e.g. `ICounterGrain` implemented by `UpCounterGrain` and `DownCounterGrain`), then:
  - `GetGrain<ICounterGrain>(key)` can throw `ArgumentException` due to ambiguous mapping.

## Ways to disambiguate

1. **Unique interfaces (cleanest)**
   - `IUpCounterGrain`, `IDownCounterGrain` extend `ICounterGrain`.
   - Use `GetGrain<IUpCounterGrain>(key)` vs `GetGrain<IDownCounterGrain>(key)`.

2. **Class name prefix**
   - `GetGrain<ICounterGrain>(key, grainClassNamePrefix: "Up")`.

3. **Naming convention default**
   - `ICounterGrain` + `CounterGrain` → `GetGrain<ICounterGrain>` picks `CounterGrain`.

4. **Attributes**
   - `[GrainType("up-counter")]` on the class.
   - `[DefaultGrainType("up-counter")]` on the interface to mark that implementation as the default.

5. **Explicit `GrainId`**
   - `GetGrain<ICounterGrain>(GrainId.Create("up-counter", "my-counter"))`.
   - Directly specifies grain type + key, avoiding ambiguity.

## Implementation detail

- Orleans generates grain reference types per interface (inherit from `GrainReference`).
- `GetGrain` returns these generated proxy instances.

