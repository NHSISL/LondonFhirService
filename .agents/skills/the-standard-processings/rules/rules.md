# The Standard — Processing Services — Rules

## Dependency

**ts-processings-001** [ERROR] Processing services must call only foundation services — never brokers directly.

## Single Entity

**ts-processings-002** [ERROR] Processing services must serve only one entity type.

## Higher-Order Operations

**ts-processings-003** [ERROR] Processing services must implement higher-order operations that combine or extend foundation service calls (e.g., EnsureStudentExistsAsync, UpsertStudentAsync).

## Exception Handling

**ts-processings-004** [ERROR] Processing services must catch and wrap all downstream exceptions from foundation services into processing-level exceptions.

## Naming

**ts-processings-005** [ERROR] Processing service interfaces must follow the naming pattern `I{Entity}ProcessingService`.

## Structure

**ts-processings-006** [ERROR] Processing services must be partial classes split by operation category.
