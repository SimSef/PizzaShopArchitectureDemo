# Grain Extensions

## What they are

- Extra behavior you can bolt onto grains via separate interfaces.
- Extension interfaces must derive from `IGrainExtension`.
- Accessed via: `grain.AsReference<IMyExtension>()`.

---

## 1) Global extensions (registered once via DI)

**When to use**

- Cross-cutting behavior available on any grain (for example, a custom deactivate operation).

**Pattern**

```csharp
public interface IMyExtension : IGrainExtension
{
    Task DoSomething(string msg);
}

public sealed class MyExtension : IMyExtension
{
    private readonly IGrainContext _context;  // injected

    public MyExtension(IGrainContext context) => _context = context;

    public Task DoSomething(string msg)
    {
        // use _context to act on the grain
        return Task.CompletedTask;
    }
}
```

**Register**

```csharp
siloBuilder.AddGrainExtension<IMyExtension, MyExtension>();
```

**Use**

```csharp
var grain = client.GetGrain<SomeGrain>(key);
var ext   = grain.AsReference<IMyExtension>();
await ext.DoSomething("msg");
```

---

## 2) Per-grain extensions (attached as components)

**When to use**

- Grain-specific behavior, especially when tied to that grain’s internal state.

**Pattern**

```csharp
public interface IGrainStateAccessor<T> : IGrainExtension
{
    Task<T> GetState();
    Task SetState(T state);
}

public sealed class GrainStateAccessor<T> : IGrainStateAccessor<T>
{
    private readonly Func<T> _getter;
    private readonly Action<T> _setter;

    public GrainStateAccessor(Func<T> getter, Action<T> setter)
    {
        _getter = getter;
        _setter = setter;
    }

    public Task<T> GetState() => Task.FromResult(_getter());
    public Task SetState(T state) { _setter(state); return Task.CompletedTask; }
}
```

**Attach in the grain**

```csharp
public class MyGrain : Grain
{
    private int Value;

    public override Task OnActivateAsync()
    {
        var accessor = new GrainStateAccessor<int>(
            getter: () => this.Value,
            setter: v => this.Value = v);

        ((IGrainBase)this)
            .GrainContext
            .SetComponent<IGrainStateAccessor<int>>(accessor);

        return base.OnActivateAsync();
    }
}
```

**Use**

```csharp
var grain    = client.GetGrain<MyGrain>(key);
var accessor = grain.AsReference<IGrainStateAccessor<int>>();
var v        = await accessor.GetState();
await accessor.SetState(10);
```

---

## Mental model

- Both global and per-grain approaches use `IGrainExtension` interfaces.
- Global: `AddGrainExtension<IExt, Impl>()` → DI/Orleans wires it for all grains.
- Per-grain: create in `OnActivateAsync`, attach via `GrainContext.SetComponent<T>()`.

