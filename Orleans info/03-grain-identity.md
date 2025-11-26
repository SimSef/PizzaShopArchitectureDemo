# Grain Identity: Type + Key

## Identity = Type + Key

- Logical identity is `(grain type, grain key)`.
- Often written as `type/key` (e.g. `shoppingcart/bob65`).

## Type name

- Default:
  - Class name without the `Grain` suffix, lowercased.
  - Example: `ShoppingCartGrain` → `shoppingcart`.
- Custom:
  - Use `[GrainType("cart")]` to override the logical type name.

## Key types

- Supported key types:
  - `Guid`
  - `long`
  - `string`
  - Compound keys: `Guid+string` or `long+string`
- Inside grains, use `GetPrimaryKey*()` APIs to read the key.

## Why this model

- `(type, key)` identity works across:
  - Processes
  - Restarts
  - Network boundaries
  - Storage providers

