# The Standard React TypeScript — Foundation Services — Rules

## Service Responsibility

**tsr-services-001** [ERROR] Foundation services MUST wrap exactly one broker — no more, no less.

**tsr-services-002** [ERROR] Foundation services MUST validate all inputs before calling brokers.

**tsr-services-003** [ERROR] Foundation services MUST map broker exceptions to service-layer exceptions (ValidationException, DependencyException, ServiceException).

**tsr-services-004** [ERROR] Service methods MUST use try-catch pattern when calling brokers.

**tsr-services-005** [ERROR] Foundation services MUST use dependency injection to receive broker instances.

**tsr-services-006** [ERROR] Service method names SHOULD mirror broker method names (e.g., broker: `getPatientAsync` → service: `retrievePatientByIdAsync`).

**tsr-services-007** [ERROR] Services MUST implement TryCatch wrapper for exception mapping and logging.

**tsr-services-008** [ERROR] Foundation services MUST NOT contain UI logic (formatting, display strings, className generation).

**tsr-services-009** [ERROR] Foundation services MUST NOT call other services — composition happens at processing/orchestration layer.

**tsr-services-010** [ERROR] Services MUST define clear exception types: ValidationException, DependencyException, ServiceException.

## Validation Rules

**tsr-services-011** [ERROR] Services MUST validate for null/undefined inputs.

**tsr-services-012** [ERROR] Services MUST validate required string fields are not empty.

**tsr-services-013** [ERROR] Services MUST validate required ID fields exist before retrieval operations.

**tsr-services-014** [WARN] Services SHOULD validate structural integrity (e.g., valid email format, date ranges).

**tsr-services-015** [ERROR] Validation exceptions MUST include the field name and reason for failure.

**tsr-services-016** [ERROR] Validation MUST happen before broker calls, not after.

## Exception Mapping

**tsr-services-017** [ERROR] Broker exceptions MUST be caught and wrapped in DependencyException.

**tsr-services-018** [ERROR] Validation failures MUST throw ValidationException with descriptive messages.

**tsr-services-019** [ERROR] Unexpected service errors MUST be wrapped in ServiceException.

**tsr-services-020** [ERROR] All service exceptions MUST preserve the inner exception for debugging.

**tsr-services-021** [ERROR] Exception messages MUST be user-facing and actionable.

## Service Organization

**tsr-services-022** [ERROR] Foundation services MUST be in `services/foundations/{domain}/` directory.

**tsr-services-023** [ERROR] Service class names MUST follow pattern `{Domain}Service` (e.g., `PatientService`).

**tsr-services-024** [ERROR] Service interface names MUST follow pattern `I{Domain}Service` (e.g., `IPatientService`).

**tsr-services-025** [WARN] Services SHOULD expose interfaces for testing and dependency inversion.

**tsr-services-026** [ERROR] Each service file MUST contain one service class only.
