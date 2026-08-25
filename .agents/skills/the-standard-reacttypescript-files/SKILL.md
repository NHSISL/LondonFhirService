---
name: the-standard-reacttypescript-files
version: 0.1.0
standard-version: v2.50.0
applies-to: ["src/**/*"]
depends-on: []
---

# The Standard React TypeScript — Files

## 0/ Context (Purpose — WHY this skill exists)

0.0/ Where: React + TypeScript + Vite projects following The Standard architectural philosophy
0.1/ Who: Frontend engineers writing or reviewing React TypeScript code
0.2/ What: Governs file naming, organization, and structural conventions for React TypeScript projects
0.3/ Applies to: src/**/* — all files within the React TypeScript source directory
0.4/ Version: The Standard React TypeScript v0.1.0
0.5/ Depends on: none

## 1/ Actual (Dependency — WHAT the rules are and what they depend on)

1.0/ Dos:

1. A file MUST contain one primary responsibility → see rules/rules.md#tsr-files-001
2. File names MUST describe architectural role → see rules/rules.md#tsr-files-002
3. React component files MUST use `.tsx` extension → see rules/rules.md#tsr-files-003
4. Non-rendering TypeScript files MUST use `.ts` extension → see rules/rules.md#tsr-files-004

1.1/ Don'ts:

1. Never create generic `utils`, `helpers`, or `common` files → see validations/anti-patterns.md#generic-utility-files
2. Never mix multiple concerns in a single file → see validations/anti-patterns.md#mixed-concerns
3. Never use vague file names that hide purpose → see validations/anti-patterns.md#vague-naming

1.2/ Ask:

- When uncertain whether a utility function warrants its own file or belongs in an existing module
- When a file seems to have multiple equally important responsibilities
- When architectural role is ambiguous (e.g., is this a service or a hook?)

1.3/ Defaults:

- Brokers follow pattern: `{domain}{Kind}Broker.ts` (e.g., `patientApiBroker.ts`)
- Services follow pattern: `{domain}Service.ts` (e.g., `patientService.ts`)
- View services follow pattern: `{domain}ViewService.ts` (e.g., `dashboardViewService.ts`)
- Components follow pattern: `{Domain}{Purpose}.tsx` (e.g., `PatientCard.tsx`)
- Pages follow pattern: `{Domain}Page.tsx` (e.g., `DashboardPage.tsx`)
- Hooks follow pattern: `use{Purpose}.ts` (e.g., `useDashboardPage.ts`)
- Models follow pattern: `{Domain}.ts` or `{Domain}{Purpose}.ts` (e.g., `Patient.ts`, `PatientCardProps.ts`)

1.4/ Examples:

**✅ Good file organization:**
```
src/
  brokers/
    apis/
      patientApiBroker.ts
      iPatientApiBroker.ts
  services/
    foundations/
      patientService.ts
      iPatientService.ts
    views/
      dashboardViewService.ts
  components/
    patients/
      PatientCard.tsx
      PatientList.tsx
  pages/
    dashboard/
      DashboardPage.tsx
  models/
    foundations/
      patients/
        Patient.ts
```

**❌ Bad file organization:**
```
src/
  utils/
    helpers.ts
    common.ts
  patientEverything.ts
  stuff.tsx
```

For complete examples, see:
- ✅ examples/good/example_good_file_structure.md
- ✅ examples/good/example_good_broker_naming.ts
- ✅ examples/good/example_good_component_naming.tsx
- ❌ examples/bad/example_bad_generic_utils.ts
- ❌ examples/bad/example_bad_mixed_concerns.tsx

1.5/ Templates:

- Scaffold project structure: see templates/

1.6/ Checklists:

- Pre-review checklist: see validations/checklist.md

1.7/ Contracts:

- File naming patterns: see contracts/contracts.json

## 2/ Expected (Exposure — WHAT comes out)

2.0/ Format: Properly named and organized TypeScript/TSX files following architectural conventions
2.1/ Outcome: Clear file organization that communicates architectural intent. Every file name reveals its responsibility.
2.2/ Tone: Direct. Cite rule IDs. Reject vague naming. Block generic utility files.
