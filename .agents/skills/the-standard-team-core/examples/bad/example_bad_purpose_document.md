---
skill: the-standard-team-core
type: example
source-section: "1 Purposing (1.0 Overall Purpose, 1.1 Scenarios)"
demonstrates: "tst-core-001, tst-core-002, tst-core-003, tst-core-005, tst-core-006, tst-core-009"
---

# ❌ Bad Example — Purpose Document

## Our Project

We are building a web app.

---

## Scenarios

**GIVEN** I am a developer
**WHEN** I create a POST /api/orders endpoint
**THEN** the order is saved to the database

---

**GIVEN** the system
**WHEN** a request comes in
**THEN** the response is returned

---

## Why This Is Wrong

- The "business nature" says "web app" — this describes how the system is built, not what the business is (violates tst-core-001).
- There are no business goals (violates tst-core-002).
- There are no future considerations (violates tst-core-003).
- The scenarios use developer actors ("I am a developer") rather than end-user actors (violates tst-core-009).
- The scenario outcomes describe technical operations ("saved to database") rather than user-visible results (violates tst-core-006, tst-core-007).
- The scenario actor "the system" is not a human actor (violates tst-core-009).
