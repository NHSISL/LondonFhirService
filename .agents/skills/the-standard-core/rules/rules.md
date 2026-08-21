# The Standard — Core Principles — Rules

## Single Responsibility

**ts-core-001** [ERROR] Every component must serve one and only one purpose.
**ts-core-008** [ERROR] Favour small, composable units over large monolithic implementations.
**ts-core-009** [ERROR] Separate what changes frequently from what stays stable.

## Naming

**ts-core-002** [ERROR] Name every artifact by what it is, not how it works.
**ts-core-007** [ERROR] Code must communicate intent clearly without requiring comments.

## Layering

**ts-core-003** [ERROR] Every layer must depend only on the layer directly below it — never skip layers.

## Testability

**ts-core-004** [ERROR] Every public member must be testable without modification.

## Encapsulation

**ts-core-005** [ERROR] Expose only what callers need; hide all implementation details.

## Business Rules

**ts-core-006** [ERROR] All business rules must live in a service, never in a controller or broker.
