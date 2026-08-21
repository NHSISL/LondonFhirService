---
skill: the-standard-csharp-files
type: anti-patterns
source-section: "0. Files"
---

# The Standard C# — Files — Anti-Patterns

## Wrong Case Filename

**Violates:** tsc-csharp-files-001, tsc-csharp-files-002
**What happens:** A file is named `student.cs` or `studentService.cs` using camelCase or all-lowercase.
**Why it's wrong:** C# file names must use PascalCase. Lowercase or camelCase names are inconsistent with C# class naming conventions and make the codebase harder to navigate.
**Fix:** Rename to `Student.cs` or `StudentService.cs`.

## Wrong Separator

**Violates:** tsc-csharp-files-002
**What happens:** A file is named `Student_Service.cs` or `student-service.cs` using underscores or hyphens.
**Why it's wrong:** C# file names must use PascalCase word boundaries — not underscores or hyphens. These separators violate the standard naming convention.
**Fix:** Remove separators and capitalise each word: `StudentService.cs`.

## Missing Aspect in Partial Class File

**Violates:** tsc-csharp-files-003
**What happens:** A partial class file is named `StudentService2.cs` or `StudentServiceExtra.cs` instead of `StudentService.Validations.cs`.
**Why it's wrong:** Partial class file names must communicate which dimension (aspect) of the class they contain. Arbitrary suffixes are not meaningful.
**Fix:** Name the file with a dot-separated aspect: `StudentService.Validations.cs` or `StudentService.Exceptions.cs`.
