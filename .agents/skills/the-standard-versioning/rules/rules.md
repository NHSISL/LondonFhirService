# The Standard — Versioning — Rules

## Semantic Versioning

**ts-versioning-001** [ERROR] All libraries and APIs must use semantic versioning: MAJOR.MINOR.PATCH.
**ts-versioning-002** [ERROR] The MAJOR version must be incremented when introducing breaking changes.
**ts-versioning-003** [ERROR] The MINOR version must be incremented when adding backward-compatible features.
**ts-versioning-004** [ERROR] The PATCH version must be incremented for backward-compatible bug fixes only.

## API Version Exposure

**ts-versioning-005** [ERROR] API version must be exposed in the URL path at the controller level (e.g., `/api/v1/students`).
**ts-versioning-006** [ERROR] Previous API versions must be maintained until all consumers have migrated — do not remove an old version without a deprecation period.

## Project File

**ts-versioning-007** [ERROR] The library or API version must be declared in the `<Version>` element inside the `.csproj` file.
