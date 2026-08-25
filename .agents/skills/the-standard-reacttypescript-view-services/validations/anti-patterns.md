# The Standard React TypeScript — View Services — Anti-Patterns

## Foundation Model Leak
**Violates:** tsr-view-006
Foundation models returned directly to UI instead of view models.

## Business Logic in View
**Violates:** tsr-view-003
View service contains validation, business rules, or authorization logic.

## Broker in View Service
**Violates:** tsr-view-002
View service calls broker directly instead of foundation service.

## Mixed Concerns
**Violates:** tsr-view-003, tsr-view-004
Business logic mixed with presentation transformations.
