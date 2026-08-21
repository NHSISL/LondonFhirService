---
skill: the-standard-csharp-comments
type: anti-patterns
source-section: "12. Comments"
---

# The Standard C# — Comments — Anti-Patterns

## Wrong Copyright Style (Block Comment)

**Violates:** tsc-csharp-comments-003
**What happens:** A `/* */` block comment is used for the copyright header.
**Why it's wrong:** Copyright blocks must use the `//` single-line dashed-border style. Block comments are not permitted for this purpose.
**Fix:** Replace with the `//` dashed-border pattern.

## XML Copyright Tag

**Violates:** tsc-csharp-comments-004
**What happens:** An XML `<copyright file="..." company="...">` tag block is used as the copyright header.
**Why it's wrong:** XML-style copyright tags are not the Standard-compliant format. The dashed `//` border style is required.
**Fix:** Replace with the `//` dashed-border pattern.

## Redundant Comment

**Violates:** tsc-csharp-comments-001, tsc-csharp-comments-006
**What happens:** A comment restates what the code already expresses, e.g., `// increment i` above `i++`.
**Why it's wrong:** Comments must only explain what the code cannot. If the code is clear, the comment adds noise and must be removed.
**Fix:** Remove the comment. If the logic is truly unclear, rename the variable or method to be self-documenting first.

## Missing Method Documentation

**Violates:** tsc-csharp-comments-005
**What happens:** A method performing inaccessible or complex logic has no documentation.
**Why it's wrong:** Methods not accessible at dev-time or performing complex operations must document their Purposing, Incomes, Outcomes, and Side Effects so that the intent is clear without running the code.
**Fix:** Add a structured comment block with all four sections above the method signature.
