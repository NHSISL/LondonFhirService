---
name: the-standard-reacttypescript-typescript
version: 0.1.0
standard-version: v2.50.0
applies-to: ["*.ts", "*.tsx"]
depends-on: []
---

# The Standard React TypeScript — TypeScript Rules

## 0/ Context (Purpose — WHY this skill exists)

0.0/ Where: React + TypeScript + Vite projects following The Standard architectural philosophy
0.1/ Who: Frontend engineers writing or reviewing TypeScript code
0.2/ What: Governs TypeScript usage, type safety, and coding conventions
0.3/ Applies to: *.ts, *.tsx — all TypeScript source files
0.4/ Version: The Standard React TypeScript v0.1.0
0.5/ Depends on: none

## 1/ Actual (Dependency — WHAT the rules are and what they depend on)

1.0/ Dos:

1. TypeScript MUST be strict → see rules/rules.md#tsr-typescript-001
2. Use explicit domain types at architectural boundaries → see rules/rules.md#tsr-typescript-002
3. Use `type` for data shapes and `interface` for behavioral contracts → see rules/rules.md#tsr-typescript-003
4. Use `unknown` at unsafe boundaries, never `any` → see rules/rules.md#tsr-typescript-004
5. Prefer named exports for services, brokers, models, hooks → see rules/rules.md#tsr-typescript-005

1.1/ Don'ts:

1. Never use `any` → see validations/anti-patterns.md#any-type
2. Never suppress TypeScript errors without documented justification → see validations/anti-patterns.md#suppressed-errors
3. Never use barrel files when they hide dependency direction → see validations/anti-patterns.md#barrel-files
4. Never use type assertions without validation → see validations/anti-patterns.md#unsafe-assertions

1.2/ Ask:

- When uncertain whether to use `type` or `interface`
- When a type seems too complex or nested
- When considering a type assertion
- When external library types are incompatible

1.3/ Defaults:

- Strict mode enabled in tsconfig.json
- `noImplicitAny: true`
- `strictNullChecks: true`
- `noUncheckedIndexedAccess: true`
- `exactOptionalPropertyTypes: true`
- Named exports preferred over default exports

1.4/ Examples:

**✅ Explicit types at boundaries:**
```typescript
export async function retrievePatientAsync(
    patientId: string)
    : Promise<Patient> {
    // implementation
}
```

**❌ Using any:**
```typescript
export async function retrievePatientAsync(id: any): Promise<any> {
    // implementation
}
```

For complete examples, see:
- ✅ examples/good/example_good_strict_types.ts
- ✅ examples/good/example_good_type_vs_interface.ts
- ✅ examples/good/example_good_unknown_boundary.ts
- ❌ examples/bad/example_bad_any_usage.ts
- ❌ examples/bad/example_bad_type_assertion.ts

1.5/ Templates:

- TypeScript configuration: see templates/

1.6/ Checklists:

- Pre-review checklist: see validations/checklist.md

1.7/ Contracts:

- TypeScript configuration rules: see contracts/contracts.json

## 2/ Expected (Exposure — WHAT comes out)

2.0/ Format: Type-safe TypeScript code with explicit boundaries
2.1/ Outcome: No `any` types. All boundaries are explicitly typed. Type errors are resolved, not suppressed.
2.2/ Tone: Direct. Cite rule IDs. Block `any`. Reject type suppressions without justification.
