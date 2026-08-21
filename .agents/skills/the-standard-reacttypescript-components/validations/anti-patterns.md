# The Standard React TypeScript — Components — Anti-Patterns

## Data Fetching in Component
**Violates:** tsr-comp-002, tsr-comp-008, tsr-comp-010
Component fetches data using fetch, axios, or service calls.

## Business Logic in Component
**Violates:** tsr-comp-001, tsr-comp-009
Component contains validation, calculations, or business rules.

## useEffect Data Fetching
**Violates:** tsr-comp-010
Component uses useEffect to fetch data on mount.

## Service in Component
**Violates:** tsr-comp-008
Component directly imports and calls services.
