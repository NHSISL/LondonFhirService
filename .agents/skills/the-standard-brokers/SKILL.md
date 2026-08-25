---
name: the-standard-brokers
version: 0.1.0
standard-version: v2.50.0
applies-to: ["*Broker*.cs"]
depends-on: ["the-standard-core"]
---

# The Standard — Brokers

## 0/ Context (Purpose — WHY this skill exists)

0.0/ Where: The infrastructure layer of any Standard-compliant system.
0.1/ Who: Engineers implementing or reviewing broker classes.
0.2/ What: Enforces broker design, naming, language, abstraction, and SPAL rules.
0.3/ Applies to: *Broker*.cs
0.4/ Version: v2.50.0
0.5/ Depends on: the-standard-core

## 1/ Actual (Dependency — WHAT the rules are and what they depend on)

1.0/ Dos:
  1. Define brokers as thin wrappers over external resources with no business logic → see rules/rules.md#ts-brokers-001
  2. Name broker interfaces generically (e.g., `IStorageBroker`, not `ISqlStorageBroker`) → see rules/rules.md#ts-brokers-002
  3. Use storage language inside brokers: `InsertAsync`, `SelectAsync`, `UpdateAsync`, `DeleteAsync` → see rules/rules.md#ts-brokers-003
  4. Use RESTful language for API brokers: `PostAsync`, `GetAsync`, `PutAsync`, `DeleteAsync` → see rules/rules.md#ts-brokers-004
  5. Use queue language for queue brokers: `EnqueueAsync`, `DequeueAsync` → see rules/rules.md#ts-brokers-005
  6. Accept only primitive types or native models as input → see rules/rules.md#ts-brokers-006
  7. Return only the raw external resource response without transformation → see rules/rules.md#ts-brokers-007
  8. Retrieve configuration values via `IConfiguration` injected in the constructor → see rules/rules.md#ts-brokers-008
  9. Brokers must be partial classes when supporting multiple entities → see rules/rules.md#ts-brokers-009
  10. Register any broker that captures per-request state in its constructor as `Scoped` or `Transient` — never `Singleton` → see 1.8/ below

1.1/ Don'ts:
  1. Must not contain validation logic → see validations/anti-patterns.md#broker-with-validation
  2. Must not call other brokers → see validations/anti-patterns.md#broker-calling-broker
  3. Must not expose internal infrastructure types (e.g., `DbContext`, `HttpClient`) → see validations/anti-patterns.md#leaking-infrastructure
  4. Must not use technology-specific names in the interface (e.g., `ISqlBroker`) → see validations/anti-patterns.md#technology-specific-naming
  5. Must not register an identity-capturing broker as a singleton, and must not let a longer-lived service compose one → see 1.8/ below

1.2/ Ask:
  - Ask when a broker candidate wraps more than one external system type.
  - Ask when naming a broker supporting multiple queue or API endpoints.

1.3/ Defaults:
  - When a single entity requires storage operations, use `IStorageBroker` as the interface name.
  - When multiple storage technologies exist, append the technology name only to the concrete class, not the interface.

1.4/ Examples:
  - ✅ see examples/good/example_good_storage_broker.cs
  - ❌ see examples/bad/example_bad_broker_with_logic.cs

1.5/ Templates:
  - Scaffold a new broker: see templates/

1.6/ Checklists:
  - Pre-review checklist: see validations/checklist.md

1.7/ Contracts:
  - Verb mappings and naming contracts: see contracts/contracts.json

1.8/ ALWAYS CHECK ON REVIEW — broker lifetime is a security control:
  A broker that resolves the caller's identity does so in its CONSTRUCTOR, not per call, so its
  registered lifetime decides WHOSE identity every downstream rule and audit stamp is evaluated
  against. Registering one as a singleton freezes the first principal the process ever saw: the
  service keeps enforcing every rule correctly, against the wrong subject. Nothing throws.

  On every review of a broker or of host registration code, verify:
  1. Does this broker read `IHttpContextAccessor.HttpContext`, a `ClaimsPrincipal`, an access
     token, or any other per-request value in its CONSTRUCTOR (or in a client it constructs
     there)? Follow the chain into referenced packages — the capture is often one or two
     assemblies away behind a parameterless constructor that looks stateless.
  2. If yes, it must be registered `Scoped` or `Transient`. A `Singleton` registration is a
     defect, not a tuning choice.
  3. Every service that composes such a broker must be no longer-lived than it. A singleton
     service holding a scoped identity broker is the same defect.
  4. Behavioural tests do NOT catch this — assertions routinely exclude the audit fields.
     Require a registration test that asserts the lifetime directly.
  5. Beware shared `Add*Service()` helpers that register singletons for an unrelated reason
     (e.g. binding event-substrate handlers into a singleton event broker). A host that does
     not wire that reason must not use the helper.

## 2/ Expected (Exposure — WHAT comes out)

2.0/ Format: C# source code.
2.1/ Outcome: Brokers that are thin, technology-agnostic, correctly named, and free of business logic.
2.2/ Tone: Direct. Cite rule IDs. Violations must be fixed, not suggested.
