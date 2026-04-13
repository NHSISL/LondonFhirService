---
applyTo: "**"
description: "Core theory, design principles, and non-negotiable values of The Standard."
---

# The Standard Core — Design and Architecture Principles

These rules govern every file, every design decision, and every review.
They take precedence over all other conventions. When any conflict exists between
generic convention and The Standard, The Standard wins.
C# naming and formatting rules are in `csharp.instructions.md`.
Test conventions are in `testing.instructions.md`.

---

## Tri-Nature Theory (core-001, core-002, core-003)

Every system — at every level — MUST be understood through three lenses:

| Lens | Meaning |
|---|---|
| **Purpose** | Why does this exist? What problem does it solve? |
| **Dependency** | What does this rely on to function? |
| **Exposure** | What does this offer to others? |

- **core-001** Flag any design that cannot be described in these three terms.
- **core-002** The pattern is fractal — it applies at system level, sub-system level, service level, validation level, and exposure level. Apply it at every layer.
- **core-003** Design every part knowing it will become someone else's dependency or exposure.

---

## Engineering Sequence (core-010, core-011, core-012)

The sequence is fixed and MUST NOT be skipped or reordered:

0. Purposing   → understand and articulate the problem
1. Modeling    → define the data and operational structures required
2. Simulation  → implement the behavior using those models

- **core-010** Flag any implementation started before purpose and models are clear.
- **core-011** Never begin implementation when purpose is unclear.
- **core-012** Never begin modeling when problem observation is incomplete.

---

## Purposing (core-020 to core-024)

- **core-020** Every design session MUST start with observation — identify the real blocker, constraint, or unmet need. Do not jump to solutions.
- **core-021** Articulate the problem clearly before proposing any solution. A well-described problem is halfway solved.
- **core-022** A solution MUST honor all five qualities: readability, configurability, longevity, optimization, and maintainability.
- **core-023** Reaching the goal the wrong way is a violation. Never cut corners.
- **core-024** If purpose is unclear, stop and clarify before modeling or writing any code.

---

## Modeling (core-030 to core-037)

- **core-030** Model ONLY what the purpose requires. No speculative or future-proofing attributes.
- **core-031** Do not attract irrelevant attributes into a model.
- **core-032** Prefer the most generic valid name that still fits the problem scope.

### Data Carrier Models

| Type | Rule | Example |
|---|---|---|
| **Primary** | Self-sufficient — no physical dependency on another model | Student, Course |
| **Secondary** | Depends on a primary model — references or nests within one | StudentAddress |
| **Relational** | Connects exactly two primary models — references only, no unrelated detail | StudentCourse |
| **Hybrid** | Mixes relational + additional details — only when business flow truly requires it | StudentCourseGrade |

- **core-033** Primary models MUST NOT physically depend on another model to exist.
- **core-034** Secondary models MUST reference or nest within a primary model.
- **core-035** Relational models MUST connect exactly two primary models and MUST NOT carry unrelated details.
- **core-036** [WARNING] Hybrid models are permitted ONLY when the business flow genuinely requires mixing relational structure with additional relationship details.

### Operational Models

- **core-037** Classify operational models correctly — flag any misclassification:

| Role | Type |
|---|---|
| Integration (external resource shim) | Broker |
| Processing (business logic) | Service |
| Exposure (API/UI surface) | Controller / Exposer |
| Startup / composition / middleware | Configuration model |

---

## Simulation (core-040, core-041)

- **core-040** Simulation MUST stay within the purpose and model boundaries. Flag any method that reaches beyond its owning model's responsibility.
- **core-041** Functions, methods, and routines are the only simulation mechanisms. They MUST NOT cross model ownership.

---

## Simplicity (core-050 to core-055)

- **core-050** Simplicity is mandatory. Flag any design that introduces unnecessary complexity.
- **core-051** MUST NOT use more than one level of inheritance. Two or more levels are excessive and forbidden except when vertical versioning of flows absolutely requires it.
- **core-052** Utils, Commons, and Helpers classes are forbidden — they create horizontal entanglement.
- **core-053** MUST NOT share models across independent flows.
- **core-054** MUST NOT create local base components that produce hidden coupling (vertical entanglement). Native or external base types are the only permitted base classes.
- **core-055** [WARNING] Prefer duplication over cross-flow entanglement when duplication preserves autonomy.

---

## Autonomous Components (core-060 to core-062)

- **core-060** Every component MUST be self-sufficient. It owns its own validations, tooling, and utilities within one of its dimensions.
- **core-061** Components MAY depend on injected dependencies. They MUST NOT rely on hidden shared helpers.
- **core-062** [WARNING] Duplication is permitted when it preserves ownership and autonomy.

---

## No Magic (core-070 to core-073)

- **core-070** No hidden routines. Every behavior must be traceable from the call site.
- **core-071** No magical extensions that require chasing references to understand what they do.
- **core-072** No runtime tricks (reflection-heavy frameworks, source generators that obscure logic, auto-wiring not visible at the call site) that make the system hard to understand.
- **core-073** Validation, exception handling, tracing, security, and localization MUST be explicit and in plain sight — not hidden behind attributes, middleware, or base-class magic.

---

## Rewritability (core-080 to core-083)

A Standard-compliant system must be forkable, clonable, buildable, and testable with minimal surprise.

- **core-080** Every system MUST be easy to understand, modify, and fully rewrite.
- **core-081** No hidden dependencies — all dependencies must be declared and visible.
- **core-082** No unknown prerequisites — the system must build and run from a clean clone.
- **core-083** No injected routines that obscure behavior — no framework magic that silently alters execution.

---

## Level 0 (core-090, core-091)

- **core-090** Code MUST be understandable by an entry-level engineer.
- **core-091** If a new engineer cannot follow the system without a guided tour, the system is too complex. Simplify it.

---

## All-In / All-Out (core-100, core-101)

- **core-100** The Standard MUST be embraced fully or rejected fully. Partial adoption is not standardization — flag any cherry-picked subset presented as compliance.
- **core-101** Outdated partial adherence does not constitute a Standardized system. Stale compliance is non-compliance.

---

## Readability over Optimization (core-110, core-111)

- **core-110** When readability and performance conflict, choose readability unless a measured bottleneck demands otherwise.
- **core-111** Unreadable "optimal" software is not truly optimum — the maintenance cost exceeds any runtime gain.

---

## Airplane Mode (core-120, core-121)

- **core-120** The system MUST be runnable locally without a mandatory cloud dependency. Flag any flow that cannot be exercised without live cloud resources.
- **core-121** [WARNING] Develop tooling that bridges cloud resources to local stand-ins (emulators, mocks, local containers).

---

## No Toasters (core-130, core-131)

- **core-130** MUST NOT enforce Standard compliance via linters, style cops, or analyzers as the primary mechanism. Tools may assist; they MUST NOT replace understanding.
- **core-131** Teach The Standard person-to-person through conviction and shared understanding, not coercion.

AI-assisted coding is acceptable provided a human remains actively involved and retains full responsibility for the final code. AI may assist with reviews and suggestions, but the final authority for approval and merge decisions must always rest with a human.

---

## Last Day (core-140, core-141)

- **core-140** [WARNING] Work every day as if it might be your last day on the project. Leave the codebase in a state that does not require your presence to continue.
- **core-141** [WARNING] End each engineering day at a clear stopping point so another engineer can pick up the work seamlessly. Apply this to design, code, documentation, and automation.
