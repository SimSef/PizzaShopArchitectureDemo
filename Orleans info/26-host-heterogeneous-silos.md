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

