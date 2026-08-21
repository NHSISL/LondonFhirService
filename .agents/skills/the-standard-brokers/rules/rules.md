# The Standard — Brokers — Rules

## Design

**ts-brokers-001** [ERROR] Brokers must be thin wrappers over external resources and must contain no business logic.
**ts-brokers-009** [ERROR] Brokers must be partial classes when supporting multiple entities.

## Naming

**ts-brokers-002** [ERROR] Broker interfaces must be named generically — technology must not appear in the interface name (e.g., use `IStorageBroker`, not `ISqlStorageBroker`).

## Storage Language

**ts-brokers-003** [ERROR] Storage brokers must use storage language: `InsertAsync`, `SelectAsync`, `UpdateAsync`, `DeleteAsync`.

## API Language

**ts-brokers-004** [ERROR] API brokers must use RESTful language: `PostAsync`, `GetAsync`, `PutAsync`, `DeleteAsync`.

## Queue Language

**ts-brokers-005** [ERROR] Queue brokers must use queue language: `EnqueueAsync`, `DequeueAsync`.

## Input and Output

**ts-brokers-006** [ERROR] Brokers must accept only primitive types or native models as input — never upstream service models.
**ts-brokers-007** [ERROR] Brokers must return only the raw external resource response without transformation.

## Configuration

**ts-brokers-008** [ERROR] Brokers must retrieve configuration values (e.g., connection strings) via `IConfiguration` injected in the constructor.
