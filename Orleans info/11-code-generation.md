# Code Generation

## What this is about

- Orleans auto-generates glue code at build time:
  - Grain proxies / stubs.
  - Serialization code for your types.
- In Orleans 7+:
  - Codegen is compile-time only (no runtime codegen).
  - Mostly automatic once packages are referenced.

## Required packages (to enable codegen)

Each project must reference the correct Orleans package:

- Client apps:
  - `Microsoft.Orleans.Client`
- Silos (servers):
  - `Microsoft.Orleans.Server`
- Shared / other projects (grain interfaces, DTOs, etc.):
  - `Microsoft.Orleans.Sdk`

Once referenced, the C# source generators run at build.

## Serialization

- For your own types that need to go over the wire (grain args/returns/state):
  - Mark them with a `GenerateSerializer` attribute.
  - This signals Orleans to generate serializers for that type.
- If a type is not marked, Orleans will not auto-serialize it (except built-in types).
- You can further influence serialization with:
  - `Id` attributes → stable member IDs for versioning.
  - `Alias` attributes → alternative names for compatibility.

## When you actually care

- Most of the time:
  - Add the correct Orleans packages and you are done.
- You care about codegen details when:
  - Introducing new custom types passed between grains.
  - Needing explicit control over serialization shape/versioning.
  - Doing cross-language or cross-assembly scenarios where you must hint the generator what to scan.

