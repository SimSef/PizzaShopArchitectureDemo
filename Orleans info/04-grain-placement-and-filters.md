# Grain Placement and Filters

## What placement is

- Decides **which silo** (Orleans process/pod/instance) runs a grain activation.
- Used for:
  - Load balancing across the cluster.
  - Keeping certain grains close to each other or to specific resources.
- Configurable:
  - Global default.
  - Per grain via attributes on grain classes.

---

## Built-in placement strategies

Apply these attributes on grain classes:

- `RandomPlacement` (default)
  - Grains placed on a random compatible silo.
  - Good baseline for many grains and unpredictable load.

- `PreferLocalPlacement`
  - Tries to place/activate the grain on the same silo that is making the call.
  - Falls back to random if not possible.
  - Good when locality and cache warmth matter.

- `HashBasedPlacement`
  - Uses `hash(grainId) % siloCount` to pick a silo.
  - Same key tends to map to the same silo until cluster membership changes.
  - Good for stable-ish distribution based on keys.

- `ActivationCountBasedPlacement`
  - Uses “power of two choices”:
    - Samples a few silos and picks one with fewer activations.
  - More load-aware than random, but still simple.
  - Good for balanced load across silos.

- `StatelessWorkerPlacement`
  - Multiple activations of same key per silo; no directory entry.
  - Good for high-throughput workers, fan-out processing, “stateless service” behavior.

- `SiloRoleBasedPlacement`
  - Places grains only on silos with a specific role.
  - Good for separating workloads (for example, backend-only grains).

- `ResourceOptimizedPlacement`
  - Uses CPU and memory metrics to compute a score per silo.
  - Picks the silo with the best resource score (can bias toward local silo).
  - Good when resource utilization (not just activation count) matters.

---

## Strategy selection (rules of thumb)

- Start with: `RandomPlacement`.
- Need better load spread: `ActivationCountBasedPlacement`.
- Need locality: `PreferLocalPlacement` or stateless workers close to callers.
- Special silos/roles: `SiloRoleBasedPlacement`.
- Resource-heavy/sensitive apps: `ResourceOptimizedPlacement`.

---

## Placement filters: what they do

- Filters run **before** the placement strategy:
  1. Orleans finds all compatible silos.
  2. Filters narrow that set.
  3. Placement strategy picks from the filtered set.
- Mental model:
  - Filters: who can even be considered?
  - Strategy: how to pick among those?
- Multiple filters on the same grain:
  - Must have unique `order` values.
  - Order matters and affects behavior.

---

## Built-in metadata filters

Use silo metadata (for example, `cloud.region`, `cloud.availability-zone`):

- `RequiredMatchSiloMetadata`
  - Keeps only silos whose metadata fully matches the caller for all keys.
  - If none match, the candidate set is empty and placement fails.
  - Good for hard constraints (must be same zone/region/etc.).

- `PreferredMatchSiloMetadata`
  - Tries full match on all keys; if too few candidates:
    - Gradually drops keys and retries (less strict match).
    - If still nothing, falls back to all candidates.
  - `minCandidates` = minimal number of silos to avoid overloading perfect matches (default 2).
  - Good for: prefer locality/metadata match, but don’t break load-balancing.

---

## Custom placement filters (pattern)

To define reusable placement logic:

1. Attribute: derive from `PlacementFilterAttribute`.
2. Strategy: derive from `PlacementFilterStrategy` (holds config like `order`).
3. Director: implement `IPlacementFilterDirector` and its `Filter(...)` method to prune candidate silos.
4. Register in startup:
   - `services.AddPlacementFilter<Strategy, Director>();`
5. Use:
   - Add your attribute to a grain class.
   - Combine with any placement strategy attribute (for example, `[ActivationCountBasedPlacement]`).

Example:

- “Prefer local silo if it can host, otherwise let `ActivationCountBasedPlacement` choose among all”:
  - Custom `PreferLocal` filter + `[ActivationCountBasedPlacement]` on the grain.

---

## Big picture for scaling

- Think in two layers:
  1. Where can this grain run? (constraints/filters, metadata, roles).
  2. How should we choose among those silos? (placement strategy).
- Combine:
  - Filters → honor topology/zones/special silos.
  - Strategies → balance load, locality, and resource utilization.

