---
skill: the-standard-csharp-comments
type: rules
source-section: "12. Comments"
---

# The Standard C# — Comments — Rules

## General Usage

**tsc-csharp-comments-001** [ERROR] Comments must only be used to explain what the code itself cannot express.

## Copyright Blocks

**tsc-csharp-comments-002** [ERROR] Copyright blocks must use the single-line `//` dashed-border style with exactly two content lines (copyright holder and license note).
**tsc-csharp-comments-003** [ERROR] Copyright blocks must not use `/* */` block comment syntax.
**tsc-csharp-comments-004** [ERROR] Copyright blocks must not use XML `<copyright>` tag syntax.

## Method Documentation

**tsc-csharp-comments-005** [WARN] Methods that are not accessible at dev-time or perform complex logic must document: Purposing, Incomes, Outcomes, and Side Effects.

## Redundant Comments

**tsc-csharp-comments-006** [ERROR] Comments that simply restate what the code already says must be removed.
