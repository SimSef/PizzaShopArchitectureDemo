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
- As the user shares Orleans-related information, append it here in new sections, keeping it concise and organized.
