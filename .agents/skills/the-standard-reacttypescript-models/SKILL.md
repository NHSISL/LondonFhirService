---
name: the-standard-reacttypescript-models
version: 0.1.0
standard-version: v2.50.0
applies-to: ["src/models/**/*"]
depends-on: ["the-standard-reacttypescript-typescript"]
---

# The Standard React TypeScript — Models

## 0/ Context (Purpose — WHY this skill exists)

0.0/ Where: React + TypeScript + Vite projects following The Standard architectural philosophy
0.1/ Who: Frontend engineers defining data contracts and models
0.2/ What: Governs model definitions, contracts between layers, and data shapes
0.3/ Applies to: src/models/**/* — all model definition files
0.4/ Version: The Standard React TypeScript v0.1.0
0.5/ Depends on: the-standard-reacttypescript-typescript

## 1/ Actual (Dependency — WHAT the rules are and what they depend on)

1.0/ Dos:

1. Foundation models MUST represent domain or API-facing concepts → see rules/rules.md#tsr-models-001
2. View models MUST represent UI-ready data produced by view services → see rules/rules.md#tsr-models-002
3. Component prop models MUST describe rendering input only → see rules/rules.md#tsr-models-003
4. Models MUST be organized by architectural layer → see rules/rules.md#tsr-models-006
5. Use `type` for model definitions → see rules/rules.md#tsr-models-007

1.1/ Don'ts:

1. Never reuse API response models directly as component props when UI needs different shape → see validations/anti-patterns.md#direct-api-props
2. Never put behavior into models → see validations/anti-patterns.md#models-with-behavior
3. Never mix foundation and view model concerns → see validations/anti-patterns.md#mixed-model-concerns
4. Never use optional properties when the field is required at runtime → see validations/anti-patterns.md#incorrect-optionality

1.2/ Ask:

- When uncertain whether a model belongs in foundations/ or views/
- When a model seems to serve both domain and UI purposes
- When considering nested vs. flat model structures

1.3/ Defaults:

- Foundation models in `models/foundations/{domain}/`
- View models in `models/views/{domain}/`
- Component props in `models/components/{domain}/`
- Configuration models in `models/configurations/`
- Use explicit property types, avoid `any`

1.4/ Examples:

**✅ Foundation model:**
```typescript
export type Patient = {
  id: string;
  firstName: string;
  lastName: string;
  dateOfBirth: string;
};
```

**✅ View model:**
```typescript
export type PatientCardView = {
  id: string;
  displayName: string;
  age: number;
  statusClassName: string;
};
```

**❌ Mixing concerns:**
```typescript
export type Patient = {
  id: string;
  firstName: string;
  displayName: string; // UI concern
  statusClassName: string; // UI concern
};
```

For complete examples, see:
- ✅ examples/good/example_good_foundation_model.ts
- ✅ examples/good/example_good_view_model.ts
- ✅ examples/good/example_good_component_props.ts
- ❌ examples/bad/example_bad_mixed_concerns.ts
- ❌ examples/bad/example_bad_models_with_behavior.ts

1.5/ Templates:

- Model organization structure: see templates/

1.6/ Checklists:

- Pre-review checklist: see validations/checklist.md

1.7/ Contracts:

- Model organization patterns: see contracts/contracts.json

## 2/ Expected (Exposure — WHAT comes out)

2.0/ Format: TypeScript type definitions organized by architectural layer
2.1/ Outcome: Clear separation between domain models, view models, and component props. No behavior in models. Type-safe contracts between layers.
2.2/ Tone: Direct. Cite rule IDs. Block behavior in models. Enforce layer separation.
