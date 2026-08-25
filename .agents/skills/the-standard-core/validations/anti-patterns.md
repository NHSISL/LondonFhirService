# The Standard — Core Principles — Anti-Patterns

## Layer Skipping

**Violates:** ts-core-003
**What happens:** A controller or service calls a broker directly, bypassing the service layer.
**Why it's wrong:** It couples infrastructure to exposure, destroys testability, and breaks The Standard's tri-nature separation.
**Fix:** Route the call through the appropriate service layer.

## Mixed Concerns

**Violates:** ts-core-001, ts-core-006
**What happens:** A single class contains both infrastructure access and business rule logic.
**Why it's wrong:** The class now has more than one reason to change, making it harder to test and maintain.
**Fix:** Extract the business logic into a service and the infrastructure access into a broker.

## Poor Naming

**Violates:** ts-core-002, ts-core-007
**What happens:** A class or method is named by what it does internally (e.g., `SqlStudentHelper`, `ProcessData`) rather than what it represents.
**Why it's wrong:** Callers cannot understand intent without reading the implementation.
**Fix:** Rename to reflect purpose: `StudentService`, `AddStudentAsync`.

## God Class

**Violates:** ts-core-008
**What happens:** A single class accumulates responsibilities for multiple entities or concerns.
**Why it's wrong:** Changes to one concern risk breaking another; the class is untestable in isolation.
**Fix:** Split into focused, single-purpose classes — one per entity or concern.
