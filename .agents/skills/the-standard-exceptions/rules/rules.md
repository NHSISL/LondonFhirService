# The Standard — Exceptions — Rules

## Three-Category Model

**ts-exceptions-001** [ERROR] Every service must expose exactly three outer exception categories: Validation, Dependency, and Service.
**ts-exceptions-002** [ERROR] ValidationException must wrap all input-related errors (null model, empty required field, invalid value).
**ts-exceptions-003** [ERROR] DependencyException must wrap all infrastructure-related errors (SqlException, HttpRequestException, queue failures).
**ts-exceptions-004** [ERROR] DependencyValidationException must wrap dependency-detected business rule violations (duplicate key, not found).
**ts-exceptions-005** [ERROR] ServiceException must wrap any unexpected exception not covered by the Validation or Dependency categories.

## Layer Isolation

**ts-exceptions-006** [ERROR] Each layer must define its own exception types — never re-use exception types from a lower layer upward.

## Inner Exception Preservation

**ts-exceptions-007** [ERROR] The original caught exception must always be passed as the inner exception when wrapping.

## Message Quality

**ts-exceptions-008** [ERROR] Exception messages must be human-readable and must not expose infrastructure details (SQL error codes, stack traces, connection strings).
