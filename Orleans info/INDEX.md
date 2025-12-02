# Orleans Docs Index

Quick lookup for the Orleans notes in this folder. For each doc: a one-line summary and some tags to help decide what to open for a given task.

## 01-grains-basics.md
Summary: Core concepts for grains: interfaces, classes, async patterns, timeouts, and basic lifecycle hooks.
Tags: grains, grain-interface, grain-class, async, timeouts, lifecycle, activation, deactivation, error-handling

## 02-grain-references.md
Summary: How grain references (proxies) work, how Orleans maps interfaces to implementations, and ways to disambiguate multiple implementations.
Tags: grain-references, proxies, GetGrain, multiple-implementations, GrainType, DefaultGrainType, GrainId, interface-mapping, routing

## 03-grain-identity.md
Summary: Grain identity model as (type + key), supported key types, and naming conventions for grain types.
Tags: identity, GrainId, grain-type, grain-key, string-key, Guid-key, long-key, compound-keys, naming-conventions

## 04-grain-placement-and-filters.md
Summary: Placement strategies for deciding which silo runs a grain activation, plus filters to influence or observe call behavior.
Tags: placement, RandomPlacement, PreferLocalPlacement, placement-strategies, silo-selection, call-filters, load-balancing, routing, cluster-topology

## 05-grain-extensions.md
Summary: Extending grains with additional interfaces/behaviors at runtime using grain extensions.
Tags: grain-extensions, extension-interfaces, dynamic-behavior, plugin-patterns, extension-registration, versioning, cross-cutting

## 06-timers-vs-reminders.md
Summary: Differences between in-memory timers and durable reminders, with guidance on when to use each for scheduling work.
Tags: timers, reminders, scheduling, periodic-tasks, persistence, reliability, background-work, OnActivateAsync, OnDeactivateAsync

## 07-observers.md
Summary: Using grain observers for push-based notifications and callback-style communication from grains to clients or other grains.
Tags: observers, IGrainObserver, subscriptions, callbacks, push-notifications, observer-pattern, disconnection-handling, streaming-adjacent

## 08-cancellation.md
Summary: Patterns for cooperative cancellation of grain calls using CancellationToken, timeouts, and propagated cancellation.
Tags: cancellation, CancellationToken, request-timeouts, cooperative-cancellation, cancellation-propagation, linked-tokens, best-practices

## 09-request-scheduling.md
Summary: How Orleans schedules grain requests, single-threaded execution model, reentrancy, and avoiding deadlocks.
Tags: scheduling, requests, reentrancy, interleaving, deadlocks, single-threaded, throughput, ordering, concurrency-model

## 10-request-context.md
Summary: Using RequestContext to flow ambient data like correlation IDs, tenant info, or auth metadata across grain calls.
Tags: RequestContext, ambient-data, correlation-id, tenant, headers, logging, tracing, context-propagation, multi-tenant

## 11-code-generation.md
Summary: Orleans code generation/source generators for grain interfaces and references, and how this impacts build/runtime.
Tags: code-generation, source-generators, grain-references, proxies, build, AOT, analyzers, configuration, tooling

## 12-grain-persistence.md
Summary: Persistent state patterns for grains, storage providers, schema design, and common persistence pitfalls.
Tags: persistence, IPersistentState, grain-state, storage-providers, AzureTable, grain-storage, serialization, versioning, consistency

## 13-event-sourcing.md
Summary: Designing event-sourced grains that persist events (and snapshots) instead of mutating state directly.
Tags: event-sourcing, events, snapshots, append-only, projections, aggregates, replay, persistence, sourcing-patterns

## 14-external-tasks.md
Summary: Handling external async work (HTTP, DB, etc.) from grains safely, including ConfigureAwait and timeout considerations.
Tags: external-tasks, async-io, HttpClient, database-calls, ConfigureAwait, timeouts, orchestration, error-handling, retries

## 15-grain-call-filters.md
Summary: Using incoming/outgoing grain call filters as middleware for logging, metrics, auth, and other cross-cutting concerns.
Tags: call-filters, IIncomingGrainCallFilter, IOutgoingGrainCallFilter, middleware, logging, metrics, tracing, authorization, cross-cutting

## 16-grain-services.md
Summary: Grain services and related patterns for shared singleton-like services and infrastructure within an Orleans cluster.
Tags: grain-services, singleton-grains, system-targets, DI, hosting, background-services, infrastructure-grains, cluster-services

## 17-stateless-worker-grains.md
Summary: Stateless worker grains for high-throughput, parallel, stateless processing using multiple activations per key.
Tags: stateless-worker, StatelessWorkerAttribute, fan-out, concurrency, scaling, transient-state, CPU-bound-work, idempotency

## 18-transactions.md
Summary: Orleans transactions for coordinating state changes across multiple grains with ACID-like guarantees and trade-offs.
Tags: transactions, ACID, multi-grain, consistency, transactional-state, orchestration, isolation, performance, trade-offs

## 19-one-way-grain-calls.md
Summary: Fire-and-forget (one-way) grain calls that do not await a response, and when they are appropriate.
Tags: one-way-calls, fire-and-forget, notifications, performance, grain-calls, reliability, usage-patterns

## 20-grain-lifecycle.md
Summary: Grain activation/deactivation lifecycle, initialization patterns, and how to hook into lifecycle events.
Tags: lifecycle, OnActivateAsync, OnDeactivateAsync, initialization, cleanup, state-loading, timers, reminders, deactivation

## 21-grain-interfaces-and-versioning.md
Summary: Versioning strategies for grain interfaces and how to evolve contracts without breaking existing callers.
Tags: versioning, interfaces, backward-compatibility, breaking-changes, new-methods, compatibility, multi-version, evolution

## 22-streaming-basics-and-use-cases.md
Summary: Basics of Orleans streaming—producers, consumers, and common scenarios for stream-based communication.
Tags: streams, streaming, producers, consumers, pub-sub, event-streams, use-cases, backpressure, subscriptions

## 23-stream-providers-and-broadcast-channels.md
Summary: Stream provider options (e.g., Azure queues, SMS) and broadcast channels, plus how to configure and use them.
Tags: stream-providers, broadcast-channels, AzureQueue, SMSProvider, in-memory-streams, configuration, fanout, routing

## 24-host-clients.md
Summary: Hosting and configuring Orleans clients, connection management, and patterns for client-side usage.
Tags: clients, OrleansClient, hosting, connection, gateways, reconnection, configuration, client-lifecycle, deployment

## 25-host-silo-lifecycle.md
Summary: Silo host lifecycle, startup and shutdown phases, and where to hook custom logic.
Tags: silo, lifecycle, host, startup, shutdown, IHostApplicationLifetime, background-tasks, deployment, hosting-patterns

## 26-host-heterogeneous-silos.md
Summary: Heterogeneous silo setups where different silos host different grain types for specialized workloads.
Tags: heterogeneous-silos, specialized-hardware, GPU, security, placement, deployment, silo-roles, segregation

## 27-host-grain-directory.md
Summary: Grain directory internals and options, including the default distributed directory and storage-based alternatives.
Tags: grain-directory, activations, placement, AzureTable, Redis, consistency, DHT, routing, directory-providers

