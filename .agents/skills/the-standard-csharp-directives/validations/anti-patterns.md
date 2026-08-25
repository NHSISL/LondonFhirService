---
skill: the-standard-csharp-directives
type: anti-patterns
source-section: "3. Directives"
---

# The Standard C# — Directives — Anti-Patterns

## Directives Inside Namespace

**Violates:** tsc-csharp-directives-001
**What happens:** `using` statements are placed inside the `namespace` block rather than at the top of the file.
**Why it's wrong:** Directives inside a namespace create scoping differences and are inconsistent with Standard file structure. All directives must be at the file top.
**Fix:** Move all `using` directives above the `namespace` declaration.

## Missing Group Separator

**Violates:** tsc-csharp-directives-003
**What happens:** System, Microsoft, third-party, and project namespaces are all listed together with no blank lines between groups.
**Why it's wrong:** The blank-line separator makes the grouping visually clear and reviewable. Without it, the ordering is hidden.
**Fix:** Insert one blank line between each group.

## Wrong Group Order

**Violates:** tsc-csharp-directives-002
**What happens:** Project-local `using` statements appear before System or Microsoft ones.
**Why it's wrong:** The required order is System → Microsoft → third-party → project-local. Violating this order makes the source inconsistent and harder to scan.
**Fix:** Reorder the groups to follow the required sequence.

## Unused Directives

**Violates:** tsc-csharp-directives-005
**What happens:** A `using` statement references a namespace that is not used anywhere in the file.
**Why it's wrong:** Unused directives increase cognitive load and clutter the file with irrelevant references.
**Fix:** Remove any `using` statement that is not referenced by the file's code.
