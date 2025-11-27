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
- C# projects generally have `ImplicitUsings` enabled; only add explicit `using` directives when they are required (for example, static extension methods or types not covered by implicit usings), and avoid redundant `using` statements.
- When building, prefer running `dotnet build` from the `PizzaShop.Aspire` directory to cover all apps in one pass.

## Styling / UI theme (PizzaShop.Web)

- Use Tailwind CSS (currently via `PizzaShop.Web/PizzaShop.Web/wwwroot/tailwind.css`) and avoid Bootstrap for new UI work.
- Overall vibe: warm, modern pizzeria — think wood-fired oven and neon signage, not generic admin dashboard.
- Color palette (Tailwind-ish):
  - Base/backgrounds: warm neutrals (e.g., `stone-50`/`stone-100`).
  - Primary accent: “tomato sauce” red/orange (e.g., `orange-500`/`red-500`).
  - Secondary accent: “basil” green (e.g., `emerald-500`/`green-600`).
  - Highlights/CTAs: “cheese” yellow (e.g., `amber-400`/`amber-500`).
- Layout & components:
  - Prefer simple, card-like surfaces with soft radius (`rounded-lg`/`rounded-xl`) and subtle shadows (`shadow-sm`/`shadow-md`).
  - Use clear focus styles (`focus:outline-none` + `focus:ring-2` in matching accent colors) for accessibility.
  - Keep pages relatively minimal; avoid overly dense, “enterprise” look unless explicitly requested.
