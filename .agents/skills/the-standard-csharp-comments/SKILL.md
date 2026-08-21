---
name: the-standard-csharp-comments
version: 0.1.0
csharp-standard-version: v0.8
applies-to: ["*.cs"]
depends-on: []
---

# The Standard C# — Comments

## 0/ Context (Purpose — WHY this skill exists)

0.0/ Where: Any C# project following the C# Coding Standard.
0.1/ Who: Engineers writing or reviewing comments in C# source files.
0.2/ What: Enforces when and how to write comments, copyright blocks, and method documentation.
0.3/ Applies to: *.cs
0.4/ Version: v0.8
0.5/ Depends on: none

## 1/ Actual (Dependency — WHAT the rules are and what they depend on)

1.0/ Dos:
  1. Use comments only to explain what the code itself cannot express → see rules/rules.md#tsc-csharp-comments-001
  2. Copyright blocks must use the single-line `//` style with the exact dashed border → see rules/rules.md#tsc-csharp-comments-002
  3. Document non-accessible or complex methods with: Purposing, Incomes, Outcomes, Side Effects → see rules/rules.md#tsc-csharp-comments-003

1.1/ Don'ts:
  1. Must not use `/* */` block comment style for copyright blocks → see validations/anti-patterns.md#wrong-copyright-style
  2. Must not use XML `<copyright>` tags in comment headers → see validations/anti-patterns.md#xml-copyright-tag
  3. Must not write comments that simply re-state what the code already says → see validations/anti-patterns.md#redundant-comment

1.2/ Ask:
  - Ask when it is unclear whether a method's logic is truly inaccessible at dev-time or just complex.

1.3/ Defaults:
  - Default: no comment unless the code cannot communicate intent on its own.
  - Default copyright style: dashed `//` border with two lines of content.

1.4/ Examples:
  - ✅ see examples/good/example_good_comments.cs
  - ❌ see examples/bad/example_bad_comments.cs

1.5/ Templates:
  - Scaffold a copyright block: see templates/

1.6/ Checklists:
  - Pre-review checklist: see validations/checklist.md

1.7/ Contracts:
  - Formal contracts: see contracts/contracts.json

## 2/ Expected (Exposure — WHAT comes out)

2.0/ Format: C# comment blocks.
2.1/ Outcome: All comments are necessary, copyright uses the correct dashed `//` style, and complex/inaccessible methods are documented with the four required sections.
2.2/ Tone: Direct. Cite rule IDs. Remove redundant comments. Correct non-compliant copyright blocks.
