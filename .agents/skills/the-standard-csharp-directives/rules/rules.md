---
skill: the-standard-csharp-directives
type: rules
source-section: "3. Directives"
---

# The Standard C# — Directives — Rules

## Placement

**tsc-csharp-directives-001** [ERROR] All `using` directives must be placed at the top of the file, outside any namespace block.

## Ordering and Grouping

**tsc-csharp-directives-002** [ERROR] `using` directives must be ordered in groups: (1) System, (2) Microsoft, (3) third-party packages, (4) project-local namespaces.
**tsc-csharp-directives-003** [ERROR] Each `using` group must be separated from the next by exactly one blank line.
**tsc-csharp-directives-004** [WARN]  Directives within each group must be sorted alphabetically.

## Cleanliness

**tsc-csharp-directives-005** [ERROR] Unused `using` directives must be removed from the file.
