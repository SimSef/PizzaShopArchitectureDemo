# AGENTS

Scope: This file applies to the entire `PizzaShopArchitectureDemo` repository.

## High-level architecture

- Core stack: Orleans + Blazor + Keycloak + Aspire.

### Projects

- `PizzaShop.Orleans.Library`
  - Orleans contracts and grain implementations (e.g., `ICalculatorGrain`, `CalculatorGrain`).
  - Shared library referenced by the silo, tests, and (later) the web app.

- `PizzaShop.Orleans.Silo`
  - Console/host process for running the Orleans silo cluster.
  - Uses `Orleans.Contrib.UniversalSilo` and references `PizzaShop.Orleans.Library`.

- `PizzaShop.Orleans.Tests`
  - Test project for Orleans grains.
  - Uses Orleans testing host and references `PizzaShop.Orleans.Library`.

- `PizzaShop.Web`
  - Blazor web application using OIDC/Keycloak and an Orleans client to talk to grains.

- `PizzaShop.Aspire`
  - Aspire AppHost that wires up and runs the PizzaShop services.

## Commit messages

- Format: `type(scope): message`
  - `type`: e.g. `feat`, `fix`, `chore`, `docs`, `refactor`, `test`.
  - `scope`: project or area, e.g. `orleans-silo`, `orleans-lib`, `orleans-tests`, `web`, `aspire`, `repo`.
  - `message`: short, imperative description, e.g. `feat(web): add basic keycloak config`.

## Agent guidelines

- Do not scaffold projects or code structure beyond this outline until explicitly requested.
- For Orleans-specific concepts, patterns, and “how to design grains/silos/streams”, prefer the documentation in the `Orleans info` folder:
  - Examples:
    - Grain basics, timeouts, lifecycle: `Orleans info/01-grains-basics.md`, `Orleans info/20-grain-lifecycle.md`.
    - Persistence and event sourcing: `Orleans info/12-grain-persistence.md`, `Orleans info/13-event-sourcing.md`.
    - Streaming and broadcast: `Orleans info/22-streaming-basics-and-use-cases.md`, `Orleans info/23-stream-providers-and-broadcast-channels.md`.
    - Host/client concerns: `Orleans info/24-host-clients.md`, `Orleans info/25-host-silo-lifecycle.md`.
- When extending Orleans behavior or adding new grains, consult these docs first and keep new notes in that folder instead of this file.
