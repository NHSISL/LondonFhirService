# The Standard — Orchestration Services — Rules

## Dependency

**ts-orchestrations-001** [ERROR] Orchestration services must depend only on processing services — never on foundation services or brokers directly.

## Multi-Entity Coordination

**ts-orchestrations-002** [ERROR] Orchestration services may coordinate multiple entity types; each entity must be managed through its own processing service.

## Exception Handling

**ts-orchestrations-003** [ERROR] Orchestration services must wrap all downstream exceptions in orchestration-level exceptions before propagating upward.

## Naming

**ts-orchestrations-004** [ERROR] Orchestration service interfaces must follow the naming pattern `I{Purpose}OrchestrationService`.

## Structure

**ts-orchestrations-005** [ERROR] Orchestration services must be partial classes split by operation category.
