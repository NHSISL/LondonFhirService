---
skill: the-standard-team-core
type: anti-patterns
source-section: "1 Purposing (1.0 Overall Purpose, 1.1 Scenarios)"
---

# The Standard Team — Core — Anti-Patterns

## Missing Purpose Elements

**Violates:** tst-core-001, tst-core-002, tst-core-003, tst-core-004
**What happens:** Engineering work begins without any documented business nature, goals, or future considerations.
**Why it's wrong:** Engineers lack context about what they are building, for whom, and to what scale. This leads to systems that solve the wrong problem or that cannot grow.
**Fix:** Add a purpose document that explicitly states the business nature, its goals, and its future considerations before starting any technical work.

## Technical Scenarios

**Violates:** tst-core-006, tst-core-007
**What happens:** Scenarios describe API endpoints, database tables, or specific user interface elements rather than user-visible outcomes.
**Why it's wrong:** Scenarios exist to communicate what the system does — not how it works. Technical scenarios confuse business requirements with implementation choices.
**Fix:** Rewrite scenarios in language a non-technical stakeholder can understand. Describe what users experience, not how the system implements it.

## Scenario As Task

**Violates:** tst-core-005, tst-core-009
**What happens:** Scenarios describe developer actions ("As a developer I will create an endpoint") rather than user actions and outcomes.
**Why it's wrong:** Scenarios must represent end-user or customer perspective. Developer tasks belong in engineering planning documents, not purpose scenarios.
**Fix:** Rewrite the scenario with an end-user actor. Identify the real actor (customer, employee, manager) and their real outcome.

## Incomplete Scenario Structure

**Violates:** tst-core-005
**What happens:** A scenario is written as a goal statement without all three required parts: GIVEN, WHEN, THEN.
**Why it's wrong:** Incomplete scenarios cannot be verified or acted on. Without an actor, interaction, and outcome, the scenario cannot be turned into testable acceptance criteria.
**Fix:** Ensure every scenario has a GIVEN (actor), WHEN (interaction or intent), and THEN (observable outcome).
