# The Standard React TypeScript — View Services — Rules

## View Service Responsibility

**tsr-view-001** [ERROR] View services MUST transform foundation models to view models for UI consumption.

**tsr-view-002** [ERROR] View services MUST inject foundation services, never brokers directly.

**tsr-view-003** [ERROR] View services MUST NOT contain business logic (validation, business rules, authorization).

**tsr-view-004** [ERROR] View services MUST handle UI-specific transformations (formatting, combining fields, calculating display values).

**tsr-view-005** [ERROR] View services MUST map foundation service exceptions to view-layer exceptions.

**tsr-view-006** [ERROR] View service methods MUST return view models, never foundation models.

**tsr-view-007** [ERROR] View services MUST NOT modify foundation models.

**tsr-view-008** [ERROR] View services MUST centralize UI transformation logic, not scatter it across components.

## Transformation Rules

**tsr-view-009** [ERROR] Date transformations MUST format dates for display (e.g., ISO string → "Jan 1, 2024").

**tsr-view-010** [ERROR] Status mappings MUST convert codes to user-friendly labels.

**tsr-view-011** [ERROR] Computed fields MUST be calculated in view service, not components.

**tsr-view-012** [ERROR] ClassName generation MUST happen in view service for conditional styling.

**tsr-view-013** [WARN] Complex transformations SHOULD be split into private helper methods.

## View Service Organization

**tsr-view-014** [ERROR] View services MUST be in `services/views/{domain}/` directory.

**tsr-view-015** [ERROR] View service class names MUST follow pattern `{Domain}ViewService`.

**tsr-view-016** [ERROR] View service interface names MUST follow pattern `I{Domain}ViewService`.

**tsr-view-017** [WARN] View services SHOULD expose interfaces for testing.

**tsr-view-018** [ERROR] Each view service file MUST contain one service class only.

**tsr-view-019** [ERROR] View services MUST NOT call other view services.

**tsr-view-020** [ERROR] View services MUST handle null/undefined foundation data gracefully with default view values.
