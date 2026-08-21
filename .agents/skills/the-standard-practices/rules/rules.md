# The Standard — Practices — Rules

## Branching

**ts-practices-001** [ERROR] Contributions must be made from a fork of the target repository — never push feature branches directly to upstream.
**ts-practices-002** [ERROR] Branch names must follow the pattern `users/{handle}/{type}/{short-description}` using lowercase kebab-case.

## Commits

**ts-practices-003** [ERROR] Commit messages must be in the imperative mood with a category prefix in ALL CAPS, e.g., `BROKERS: Add Student Storage Broker`.
**ts-practices-003b** [ERROR] Commit messages must not be vague — `fix`, `update`, `changes`, or `wip` without specifics are forbidden.

## Pull Requests

**ts-practices-004** [ERROR] Each pull request must be focused on a single concern — one PR per feature or fix.
**ts-practices-005** [ERROR] A pull request must have a passing build and all tests green before it is submitted for review.

## Configuration and Secrets

**ts-practices-006** [ERROR] Secrets, connection strings, and API keys must never be committed to source control — use environment variables or secret stores.

## CI/CD

**ts-practices-007** [ERROR] CI pipelines must be configured using ADotNet or an equivalent YAML-based pipeline configuration.
