---
skill: the-standard-csharp-files
type: rules
source-section: "0. Files"
---

# The Standard C# — Files — Rules

## File Naming

**tsc-csharp-files-001** [ERROR] File names must use PascalCase followed by the `.cs` extension.
**tsc-csharp-files-002** [ERROR] File names must not use camelCase, all-lowercase, or underscore/hyphen-separated words.

## Partial Class Files

**tsc-csharp-files-003** [ERROR] Partial class files must be named `{RootClass}.{Aspect}.cs` where `{Aspect}` describes the dimension of the class being separated.
**tsc-csharp-files-004** [ERROR] Further nested partial files may use a third segment: `{RootClass}.{Aspect}.{SubAspect}.cs`.
**tsc-csharp-files-005** [WARN]  Common aspects are `Validations` and `Exceptions` — use consistent aspect names across the codebase.
