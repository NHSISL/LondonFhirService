# The Standard — Foundation Services — Rules

## Single Entity

**ts-foundations-001** [ERROR] Foundation services must serve only one entity type.

## Validation

**ts-foundations-002** [ERROR] Foundation services must perform structural and logical validation on all incoming models before passing them to a broker.

## Broker Dependency

**ts-foundations-003** [ERROR] Foundation services must call only one broker per operation — never another service.
**ts-foundations-004** [ERROR] Foundation services must be a pass-through to the broker — no model transformation or enrichment is permitted.

## Exception Handling

**ts-foundations-005** [ERROR] Foundation services must map broker exceptions to local exceptions using dependency validation wrappers.
**ts-foundations-006** [ERROR] Foundation services must wrap all broker calls in try-catch blocks at minimum for storage exceptions and concurrency exceptions.

## Structure

**ts-foundations-007** [ERROR] Foundation services must be partial classes, one file per operation category (e.g., Add, Retrieve, Modify, Remove).
**ts-foundations-008** [ERROR] Foundation service interfaces must follow the naming pattern `I{Entity}Service`.
