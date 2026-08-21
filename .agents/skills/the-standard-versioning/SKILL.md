---
name: the-standard-versioning
version: 0.1.0
standard-version: v2.50.0
applies-to: ["*.cs", "*.csproj"]
depends-on: ["the-standard-core"]
---

# The Standard — Versioning

## 0/ Context (Purpose — WHY this skill exists)

0.0/ Where: All Standard-compliant systems — broker, service, and API layers, and project files.
0.1/ Who: Engineers designing, reviewing, or maintaining versioned APIs and libraries.
0.2/ What: Enforces semantic versioning, backward-compatible API design, version exposure at the exposer layer, and non-breaking change management.
0.3/ Applies to: *.cs, *.csproj
0.4/ Version: v2.50.0
0.5/ Depends on: the-standard-core

## 1/ Actual (Dependency — WHAT the rules are and what they depend on)

1.0/ Dos:
  1. Use semantic versioning (MAJOR.MINOR.PATCH) for all libraries and APIs → see rules/rules.md#ts-versioning-001
  2. Increment MAJOR version when introducing breaking changes → see rules/rules.md#ts-versioning-002
  3. Increment MINOR version when adding backward-compatible features → see rules/rules.md#ts-versioning-003
  4. Increment PATCH version for backward-compatible bug fixes → see rules/rules.md#ts-versioning-004
  5. Expose API version through the URL path at the controller level (e.g., `/api/v1/students`) → see rules/rules.md#ts-versioning-005
  6. Maintain previous API versions until all consumers have migrated → see rules/rules.md#ts-versioning-006
  7. Declare the version in `<Version>` inside the `.csproj` file → see rules/rules.md#ts-versioning-007

1.1/ Don'ts:
  1. Must not introduce breaking changes in a MINOR or PATCH release → see validations/anti-patterns.md#breaking-minor
  2. Must not remove or rename public API members without a MAJOR version bump → see validations/anti-patterns.md#silent-breaking-change
  3. Must not use non-semantic version strings (e.g., `1.0.0-alpha-final-v2`) → see validations/anti-patterns.md#bad-version-string
  4. Must not version internal/private members — only public API surfaces → see validations/anti-patterns.md#internal-versioning

1.2/ Ask:
  - Ask when a change removes or renames a public method or property — determine if a MAJOR bump is required.

1.3/ Defaults:
  - Initial version: `1.0.0` for GA, `0.1.0` for pre-release.
  - Version location: `<Version>` element in `.csproj`.
  - API version in route: `/api/v{major}/` prefix.

1.4/ Examples:
  - ✅ see examples/good/example_good_versioned_controller.cs
  - ❌ see examples/bad/example_bad_versioned_controller.cs

1.5/ Templates:
  - Scaffold a versioned controller: see templates/

1.6/ Checklists:
  - Pre-review checklist: see validations/checklist.md

1.7/ Contracts:
  - Versioning contracts: see contracts/contracts.json

## 2/ Expected (Exposure — WHAT comes out)

2.0/ Format: C# source code and .csproj XML.
2.1/ Outcome: Correctly versioned APIs and libraries with semantic versioning and backward-compatible change management.
2.2/ Tone: Direct. Cite rule IDs. Violations must be fixed, not suggested.
