---
name: the-standard-csharp-files
version: 0.1.0
csharp-standard-version: v0.8
applies-to: ["*.cs"]
depends-on: []
---

# The Standard C# — Files

## 0/ Context (Purpose — WHY this skill exists)

0.0/ Where: Any C# project following the C# Coding Standard.
0.1/ Who: Engineers writing, naming, or reviewing C# source files.
0.2/ What: Enforces file naming conventions and partial class file conventions for C# source files.
0.3/ Applies to: *.cs
0.4/ Version: v0.8
0.5/ Depends on: none

## 1/ Actual (Dependency — WHAT the rules are and what they depend on)

1.0/ Dos:
  1. File names must use PascalCase followed by the `.cs` extension → see rules/rules.md#tsc-csharp-files-001
  2. Partial class files must be named `{RootClass}.{Aspect}.cs` (e.g., `StudentService.Validations.cs`) → see rules/rules.md#tsc-csharp-files-002
  3. Further nested partial files may add a third segment: `{RootClass}.{Aspect}.{SubAspect}.cs` → see rules/rules.md#tsc-csharp-files-003

1.1/ Don'ts:
  1. Must not use camelCase, lowercase, or underscore-separated file names → see validations/anti-patterns.md#wrong-case-filename
  2. Must not use underscores or hyphens to separate words in file names → see validations/anti-patterns.md#wrong-separator

1.2/ Ask:
  - Ask when it is unclear which aspect (Validations, Exceptions) a partial class represents.

1.3/ Defaults:
  - Default file name casing: PascalCase.
  - Default partial class naming: `{RootClass}.{Aspect}.cs`.
  - Common aspects: `Validations`, `Exceptions`.

1.4/ Examples:
  - ✅ `Student.cs`, `StudentService.cs`, `StudentService.Validations.cs`
  - ❌ `student.cs`, `studentService.cs`, `Student_Service.cs`

1.5/ Templates:
  - Scaffold a class file: see templates/

1.6/ Checklists:
  - Pre-review checklist: see validations/checklist.md

1.7/ Contracts:
  - File naming contracts: see contracts/contracts.json

## 2/ Expected (Exposure — WHAT comes out)

2.0/ Format: C# file names.
2.1/ Outcome: All C# files use PascalCase naming; partial class files follow the dot-separated aspect naming pattern.
2.2/ Tone: Direct. Cite rule IDs. Non-compliant file names must be corrected.
