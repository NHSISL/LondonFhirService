# The Standard — Aggregation Services — Rules

## Dependency

**ts-aggregations-001** [ERROR] Aggregation services must depend only on orchestration services — never on processing, foundation services, or brokers.

## Fan-Out

**ts-aggregations-002** [ERROR] Aggregation services must expose a single operation that fans out to multiple orchestration services.

## Exception Handling

**ts-aggregations-003** [ERROR] Aggregation services must wrap all downstream exceptions in aggregation-level exceptions before propagating upward.

## Naming

**ts-aggregations-004** [ERROR] Aggregation service interfaces must follow the naming pattern `I{Purpose}AggregationService`.

## Structure

**ts-aggregations-005** [ERROR] Aggregation services must be partial classes split by operation category.
