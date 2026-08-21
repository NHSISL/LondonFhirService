# The Standard React TypeScript — TypeScript — Rules

## Type Safety

**tsr-typescript-001** [ERROR] TypeScript MUST be configured with strict mode enabled.

**tsr-typescript-002** [ERROR] Use explicit domain types at architectural boundaries (service methods, broker methods, component props).

**tsr-typescript-003** [ERROR] Use `type` for data shapes and `interface` for behavioral contracts.

**tsr-typescript-004** [ERROR] Use `unknown` at unsafe boundaries — never use `any`.

**tsr-typescript-005** [ERROR] Prefer named exports for services, brokers, models, hooks, and utilities.

## TypeScript Configuration

**tsr-typescript-006** [ERROR] `noImplicitAny` MUST be set to `true`.

**tsr-typescript-007** [ERROR] `strictNullChecks` MUST be set to `true`.

**tsr-typescript-008** [ERROR] `noUncheckedIndexedAccess` MUST be set to `true`.

**tsr-typescript-009** [ERROR] `exactOptionalPropertyTypes` MUST be set to `true`.

## Type Usage

**tsr-typescript-010** [ERROR] Never suppress TypeScript errors with `@ts-ignore` or `@ts-expect-error` without documented justification.

**tsr-typescript-011** [ERROR] Never use type assertions (`as`) without validation or guard.

**tsr-typescript-012** [WARN] Avoid barrel files (index.ts) when they hide dependency direction.

**tsr-typescript-013** [ERROR] Default exports MAY be used for pages and components only when framework convention benefits from it.

**tsr-typescript-014** [ERROR] Function return types MUST be explicit at architectural boundaries.

**tsr-typescript-015** [WARN] Consider using `Readonly<T>` for props types to prevent mutation.

**tsr-typescript-016** [ERROR] Never use `Function` or `Object` as types — use specific function signatures or object shapes.
