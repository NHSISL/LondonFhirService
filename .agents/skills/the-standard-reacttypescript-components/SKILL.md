---
name: the-standard-reacttypescript-components
version: 0.1.0
standard-version: v2.50.0
applies-to: ["src/components/**/*.tsx"]
depends-on: ["the-standard-reacttypescript-typescript", "the-standard-reacttypescript-models"]
---

# The Standard React TypeScript — Components

## 0/ Context (Purpose — WHY this skill exists)

0.0/ Where: React + TypeScript + Vite projects following The Standard component layer
0.1/ Who: Frontend engineers implementing reusable UI components
0.2/ What: Governs component implementations that render view models without business logic
0.3/ Applies to: src/components/**/*.tsx — all component files
0.4/ Version: The Standard React TypeScript v0.1.0
0.5/ Depends on: the-standard-reacttypescript-typescript, the-standard-reacttypescript-models

## 1/ Actual (Dependency — WHAT the rules are and what they depend on)

1.0/ Dos:

1. Components MUST be pure rendering functions → see rules/rules.md#tsr-comp-001
2. Components MUST receive all data via props → see rules/rules.md#tsr-comp-002
3. Components MUST use TypeScript for props → see rules/rules.md#tsr-comp-003
4. Components MUST be in src/components/{domain}/ → see rules/rules.md#tsr-comp-004
5. Component files MUST follow {Domain}{Purpose}.tsx pattern → see rules/rules.md#tsr-comp-005
6. Components MUST export as named exports → see rules/rules.md#tsr-comp-006
7. Components MUST handle loading/error states via props → see rules/rules.md#tsr-comp-007

1.1/ Don'ts:

1. Never fetch data in components → see validations/anti-patterns.md#data-fetching-in-component
2. Never put business logic in components → see validations/anti-patterns.md#business-logic-in-component
3. Never use useEffect for data fetching → see validations/anti-patterns.md#useeffect-data-fetching
4. Never access services directly → see validations/anti-patterns.md#service-in-component

1.2/ Ask:

- When uncertain if logic belongs in component or hook
- When deciding component granularity
- When component grows beyond 200 lines

1.3/ Defaults:

- Components in src/components/{domain}/
- Props type in models/components/{domain}/
- Named exports only
- Pure functions (no side effects)
- Data via props, callbacks for events

1.4/ Examples:

**✅ Component:**
```typescript
export function PatientCard({ patient, onSelect }: PatientCardProps) {
  return (
    <div onClick={() => onSelect?.(patient.id)}>
      <h3>{patient.displayName}</h3>
      <p>Age: {patient.age}</p>
      <span className={patient.statusClassName}>{patient.statusText}</span>
    </div>
  );
}
```

For complete examples, see:
- ✅ examples/good/example_good_component.tsx
- ✅ examples/good/example_good_conditional_rendering.tsx
- ❌ examples/bad/example_bad_data_fetching.tsx
- ❌ examples/bad/example_bad_business_logic.tsx

1.5/ Templates:

- Component template: see templates/

1.6/ Checklists:

- Pre-review checklist: see validations/checklist.md

1.7/ Contracts:

- Component contracts: see contracts/contracts.json

## 2/ Expected (Exposure — WHAT comes out)

2.0/ Format: TypeScript React function components (.tsx)
2.1/ Outcome: Pure rendering components. All data via props. No business logic. No data fetching. Reusable UI building blocks.
2.2/ Tone: Direct. Cite rule IDs. Block data fetching. Block business logic. Enforce props typing. Require pure functions.
