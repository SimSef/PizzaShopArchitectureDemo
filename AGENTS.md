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

## Observability (PizzaShop.Web)

- Use OpenTelemetry for tracing, metrics, and logs. Configure providers and exporters once during application startup (see `ObservabilityRegistration.AddObservability` and `Program.cs`), not ad hoc inside random code paths.
- Prefer spans (`ActivitySource`) for important flows (for example, auth and BFF entry points) and avoid excessive per-request logging in components or grains unless you’re actively debugging an issue.
- When adding new traces or metrics, keep names consistent with the existing `PizzaShop.Web.Auth` source and Aspire conventions so they show up cleanly in the Aspire dashboard.

## Styling / UI theme (PizzaShop.Web)

- Use Tailwind CSS (currently via `PizzaShop.Web/PizzaShop.Web/wwwroot/tailwind.css`) and avoid Bootstrap for new UI work.
- Overall vibe: cozy, Christmassy pizzeria — think warm lights, deep reds, evergreen branches, and wood; avoid flat “corporate” or generic admin styles.
- Color palette (Tailwind-ish):
  - Base/backgrounds: deep, warm reds and browns (e.g., `red-900`, `red-800`, `amber-900`, `stone-900`) with softer overlays (`red-950/90`, `stone-950/80`) for modals and panels.
  - Primary accent: “Christmas red” (e.g., `red-500`/`red-600`) used for key actions, selections, and important text highlights.
  - Secondary accent: “pine” green (e.g., `emerald-500`/`green-500`/`green-600`) used for badges, borders, and subtle highlights (mirroring basil and tree branches).
  - Tertiary accent: “golden lights” (e.g., `amber-300`/`amber-400`) for icons, dividers, small dots, and hover glows; avoid using this as the main background.
  - Surfaces/cards: rich, wood-like browns (e.g., `amber-900`, `stone-800`) with subtle gradients or overlays to evoke a wooden table/board under the pizzas.
  - Text: mostly `stone-50`/`stone-100` on dark backgrounds, with muted `stone-400` for secondary text.
- Layout & components:
  - Prefer simple, card-like surfaces with soft radius (`rounded-lg`/`rounded-xl`) and warm, soft shadows (`shadow-md`/`shadow-lg` with low blur) to feel like lit boards on a table.
  - Use clear focus styles (`focus:outline-none` + `focus:ring-2 focus:ring-offset-2 focus:ring-red-500/70` or `focus:ring-emerald-500/70`) so keyboard navigation remains obvious against dark backgrounds.
  - Introduce small “light” details (e.g., dotted backgrounds, subtle `bg-gradient-to-b from-red-900 via-red-950 to-stone-950`) sparingly to echo the out-of-focus Christmas lights in imagery.
  - Keep content relatively minimal and centered, with breathing room around pizzas/images; avoid overly dense tables or grids unless explicitly requested, and soften them with spacing and borders when needed.
