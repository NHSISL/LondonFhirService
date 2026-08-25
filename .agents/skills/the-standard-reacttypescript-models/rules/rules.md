# The Standard React TypeScript — Models — Rules

## Model Types

**tsr-models-001** [ERROR] Foundation models MUST represent domain or API-facing concepts.

**tsr-models-002** [ERROR] View models MUST represent UI-ready data produced by view services.

**tsr-models-003** [ERROR] Component prop models MUST describe rendering input only.

**tsr-models-004** [ERROR] Do NOT reuse API response models directly as component props when the UI needs a different shape.

**tsr-models-005** [ERROR] Models MUST NOT contain behavior (methods, functions, API calls, mutations).

## Model Organization

**tsr-models-006** [ERROR] Models MUST be organized by architectural layer: foundations/, views/, components/, configurations/.

**tsr-models-007** [ERROR] Use `type` keyword for model definitions (not `interface` unless defining a behavioral contract).

**tsr-models-008** [ERROR] Model files MUST follow pattern `{Domain}.ts` or `{Domain}{Purpose}.ts`.

**tsr-models-009** [WARN] Foundation models SHOULD match backend domain models when possible.

**tsr-models-010** [ERROR] View models MUST be transformation output from view services, never created in components.

## Property Rules

**tsr-models-011** [ERROR] Optional properties MUST use `?` only when the field can truly be absent.

**tsr-models-012** [ERROR] Use explicit types for all properties — never implicit `any`.

**tsr-models-013** [WARN] Prefer explicit union types over string for fixed sets of values.

**tsr-models-014** [ERROR] Date/time values SHOULD be strings (ISO 8601) at API boundaries, Date objects internally when needed.

**tsr-models-015** [WARN] Avoid deeply nested model structures beyond 3 levels.

**tsr-models-016** [ERROR] Error models MUST be separate from domain models.
