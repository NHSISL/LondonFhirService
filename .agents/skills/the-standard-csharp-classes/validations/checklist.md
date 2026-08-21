---
skill: the-standard-csharp-classes
type: checklist
source-section: "4. Classes"
---

# The Standard C# — Classes — Checklist

- [ ] Model class has no suffix (tsc-csharp-classes-001)
- [ ] Service class name is singular `{Entity}Service` (tsc-csharp-classes-002)
- [ ] Service class name does not use `BusinessLogic`, `BL`, or other non-standard suffixes (tsc-csharp-classes-003)
- [ ] Broker class name is singular `{Entity}Broker` (tsc-csharp-classes-004)
- [ ] Controller class name is plural `{Entities}Controller` (tsc-csharp-classes-005)
- [ ] All class fields are camelCase (tsc-csharp-classes-006)
- [ ] No class field is prefixed with underscore (tsc-csharp-classes-007)
- [ ] All private field references inside methods/constructors use `this.` (tsc-csharp-classes-008)
- [ ] Object instantiation uses `var` on the left-hand side (tsc-csharp-classes-009)
- [ ] No unnamed positional literal arguments in constructor calls (tsc-csharp-classes-010)
- [ ] Object initializer property order matches class declaration order (tsc-csharp-classes-011)
- [ ] Constructor parameter order matches field declaration order (tsc-csharp-classes-012)
