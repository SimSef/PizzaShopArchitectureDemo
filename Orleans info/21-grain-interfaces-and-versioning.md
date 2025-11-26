# Grain Interfaces and Versioning

## 1. Basics

- Grain interfaces can be versioned using `[Version(n)]`:
  - If no attribute is present, version is `0`.
  - Versions are per interface, not per method.
- Multiple silo versions can run in the same cluster:
  - Some silos compiled with v1 of an interface.
  - Others with v2, v3, and so on.

## 2. Evolving grain interfaces safely

You should:

- Keep old methods exactly the same:
  - Same name.
  - Same parameter types and order.
  - Same return type.
- Add new methods in newer versions.
- Use `[Obsolete]` before deleting old methods.

You must not:

- Change the signature of existing methods:
  - No changing parameter types.
  - No reordering parameters.
  - No adding or removing parameters.
- Change a method’s meaning in a way that breaks old callers (unless that is acceptable).
- Remove methods while old versions of clients/silos still use them.

Method removal pattern (two-step):

1. Version N:
   - Keep the old method, mark it `[Obsolete]`, and add the new method.
2. Update all callers to use the new method.
3. Version N+1:
   - Remove the old method once nothing uses it.

## 3. Compatibility types

Used by Orleans to decide if an activation can handle a request.

### Backward compatible (default)

- v2 is backward compatible with v1 if:
  - Same interface name.
  - All v1 methods still exist in v2, unchanged.
- Then:
  - v1 caller → v2 grain ✅
  - v2 caller → v1 grain ❌

> Newer implementation can safely serve older callers.

### Fully compatible

- v2 is fully compatible with v1 if:
  - It is backward compatible and adds no new methods.
- Then:
  - v1 caller ↔ v2 grain both ways ✅

> Both versions are interchangeable.

## 4. Version selection strategy

Which version to activate is configured via `GrainVersioningOptions.DefaultVersionSelectorStrategy`. Built-in strategies:

1. `AllCompatibleVersions` (default):
   - Random among all compatible versions.
   - Skewed by how many silos run each version.
   - Good for simple rolling upgrades.

2. `LatestVersion`:
   - Always picks the highest compatible version.
   - New activations quickly converge on newest code.

3. `MinimumVersion`:
   - Picks the requested or lowest compatible version.
   - Good for staging/blue-green setups where you control when new versions are used.

Compatibility logic is provided by `DefaultCompatibilityStrategy` (typically `BackwardCompatible`).

## 5. Deployment patterns

### A. Simple rolling upgrade (single environment)

Config:

- `DefaultCompatibilityStrategy = BackwardCompatible`.
- `DefaultVersionSelectorStrategy = AllCompatibleVersions`.

Flow:

1. All clients and silos running v1.
2. Deploy some v2 silos into the same cluster.
3. New activations:
   - v1 callers may hit v1 or v2 (v2 is backward compatible, so safe).
   - v2 callers get v2.
4. Gradually roll all silos to v2.
5. Once fully migrated, you can move towards v3, and so on.

Pros: simple.
Cons: rollback means rolling back deployments.

### B. Staging / blue-green in the same cluster

Config:

- `DefaultCompatibilityStrategy = BackwardCompatible`.
- `DefaultVersionSelectorStrategy = MinimumVersion`.

Flow:

1. Prod slot:
   - v1 silos and clients.
2. Staging slot:
   - Start v2 silos and clients; they join the same cluster.
   - With `MinimumVersion`, v1 callers still trigger v1 activations.
3. Route some traffic to v2 clients for smoke/beta:
   - Those v2 clients create v2 activations.
4. If bad:
   - Kill staging; v2 activations die; v1 continues.
5. If good:
   - Swap: make v2 clients primary, then retire v1 silos.

Pros: safe rollout and easy rollback.

## 6. Important caveats

- Stateless workers are not versioned.
- Streaming interfaces are not versioned.
- Grain state versioning is separate; these rules apply only to interfaces/method calls.
- Always think about:
  - Can old callers still talk to new grains?
  - Can new callers accidentally hit old grains?

