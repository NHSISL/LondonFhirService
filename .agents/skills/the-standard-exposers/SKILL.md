---
name: the-standard-exposers
version: 0.1.0
standard-version: v2.50.0
applies-to: ["*Controller*.cs"]
depends-on: ["the-standard-core"]
---

# The Standard — Exposers (RESTful APIs)

## 0/ Context (Purpose — WHY this skill exists)

0.0/ Where: The exposer layer of any Standard-compliant system — RESTful API controllers.
0.1/ Who: Engineers implementing or reviewing API controllers.
0.2/ What: Enforces thin controller design, correct HTTP verb and status code usage, single-service dependency, and exception-to-HTTP mapping.
0.3/ Applies to: *Controller*.cs
0.4/ Version: v2.50.0
0.5/ Depends on: the-standard-core

## 1/ Actual (Dependency — WHAT the rules are and what they depend on)

1.0/ Dos:
  1. Controllers must be thin — delegate all logic to a single service → see rules/rules.md#ts-exposers-001
  2. Controllers must map each service exception type to the correct HTTP status code → see rules/rules.md#ts-exposers-002
  3. POST endpoints must return 201 Created with the created resource → see rules/rules.md#ts-exposers-003
  4. GET endpoints must return 200 OK → see rules/rules.md#ts-exposers-004
  5. PUT endpoints must return 200 OK with the updated resource → see rules/rules.md#ts-exposers-005
  6. DELETE endpoints must return 200 OK with the deleted resource → see rules/rules.md#ts-exposers-006
  7. Controllers must use `[ApiController]` and `[Route("api/[controller]")]` attributes → see rules/rules.md#ts-exposers-007
  8. Controllers must inherit from `RESTFulController` or `ControllerBase` → see rules/rules.md#ts-exposers-008

1.1/ Don'ts:
  1. Must not contain business logic or validation — only pass-through and HTTP mapping → see validations/anti-patterns.md#controller-with-business-logic
  2. Must not inject more than one service → see validations/anti-patterns.md#multiple-service-injection
  3. Must not return raw exception messages to the client → see validations/anti-patterns.md#leaking-exceptions
  4. Must not use `[HttpGet]` for mutations or `[HttpPost]` for queries → see validations/anti-patterns.md#wrong-http-verb
  5. Must not let inferred model-binding rules reject a request the foundation would accept → see 1.8/ below

1.2/ Ask:
  - Ask when a controller endpoint needs data from more than one service — that belongs in an aggregation service above.

1.3/ Defaults:
  - Validation exception → 400 Bad Request.
  - Dependency validation exception (already exists) → 409 Conflict.
  - Dependency exception (storage/infra failure) → 424 Failed Dependency.
  - Service exception (unexpected) → 500 Internal Server Error.
  - Not found → 404 Not Found.

1.4/ Examples:
  - ✅ see examples/good/example_good_controller.cs
  - ❌ see examples/bad/example_bad_controller.cs

1.5/ Templates:
  - Scaffold a new REST controller: see templates/

1.6/ Checklists:
  - Pre-review checklist: see validations/checklist.md

1.7/ Contracts:
  - HTTP verb and status code contracts: see contracts/contracts.json

1.8/ ALWAYS CHECK ON REVIEW — inferred model-binding rules are validation the exposer did not write:
  `[ApiController]` runs ModelState before the action body. Any rule MVC infers from the bound
  type therefore rejects the request before the service is reached — a validation decision made
  outside the layer that owns validation, and one no controller test exercises, because a unit
  test calls the action method directly and never goes through model binding.

  The rule that bites is `SuppressImplicitRequiredAttributeForNonNullableReferenceTypes`, which
  defaults to OFF: MVC infers `[Required]` from C# nullability. Bind a persistence entity and
  every non-nullable member becomes mandatory on the wire — including EF navigation properties,
  which are non-nullable because the foreign key guarantees them in STORAGE and which no caller
  ever sends. `ApprovalComment.Approval` made every valid POST a 400 until it was suppressed in
  `Program.cs`.

  On every review of a controller or of host MVC configuration, verify:
  1. Does the bound model carry a non-nullable navigation property, or any non-nullable member
     the service fills rather than the caller? If the host has not suppressed the implicit
     `[Required]`, that endpoint cannot be called successfully.
  2. Is every rejection the endpoint can produce traceable to a rule the foundation states? A
     400 that no `Validate(...)` in the service explains came from model binding.
  3. Is there an acceptance test that POSTs and PUTs the entity over real HTTP? A unit test
     cannot catch this class of defect — only a request through the pipeline can.

## 2/ Expected (Exposure — WHAT comes out)

2.0/ Format: C# source code.
2.1/ Outcome: Thin controllers that delegate to one service and map exceptions to correct HTTP responses.
2.2/ Tone: Direct. Cite rule IDs. Violations must be fixed, not suggested.
