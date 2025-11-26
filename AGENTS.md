# AGENTS

Scope: This file applies to the entire `PizzaShopArchitectureDemo` repository.

## High-level architecture

- Core stack: Orleans + Blazor + Keycloak + Aspire.
- Core application projects:
  - `PizzaShop.Orleans`: Orleans library for interfaces, grains, and shared contracts.
  - `PizzaShop.Web`: Blazor web application using Keycloak for authentication and an Orleans client to talk to grains.
- Hosting/orchestration:
  - `PizzaShop.Aspire`: Aspire AppHost that wires up and runs the PizzaShop services.

## Commit messages

- Format: `type(scope): message`
  - `type`: e.g. `feat`, `fix`, `chore`, `docs`, `refactor`, `test`.
  - `scope`: project or area, e.g. `web`, `orleans`, `aspire`, `repo`.
  - `message`: short, imperative description, e.g. `feat(web): add basic keycloak config`.

## Agent guidelines

- Do not scaffold projects or code structure beyond this outline until explicitly requested.
- As the user shares Orleans-related information, append it here in new sections, keeping it concise and organized.
