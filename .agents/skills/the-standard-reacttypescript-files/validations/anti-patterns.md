# The Standard React TypeScript — Files — Anti-Patterns

## Generic Utility Files

**Violates:** tsr-files-014

**What happens:** Files named `utils.ts`, `helpers.ts`, or `common.ts` are created to hold miscellaneous functions.

**Why it's wrong:** Generic utility files become dumping grounds for unrelated code. They hide architectural intent and make it unclear where functionality belongs. As projects grow, these files become unmaintainable.

**Fix:** Move each function into a named module that describes its purpose:
- Date formatting → `dateTimeBroker.ts`
- API utilities → `{domain}ApiBroker.ts`
- String manipulation → `stringUtilities.ts` (specific domain)
- Validation → `{domain}Validator.ts`

---

## Mixed Concerns

**Violates:** tsr-files-001

**What happens:** A single file contains multiple unrelated responsibilities.

Example:
```typescript
// patientEverything.ts
export const fetchPatients = async () => { /* API call */ };
export const PatientCard = () => { /* Component */ };
export const formatPatientName = (patient: Patient) => { /* Utility */ };
export const usePatientData = () => { /* Hook */ };
```

**Why it's wrong:** Mixing concerns violates single responsibility principle. The file has no clear architectural identity, making it difficult to understand, test, and maintain.

**Fix:** Split into properly named files:
- `patientApiBroker.ts` — API call
- `PatientCard.tsx` — Component
- `patientService.ts` — Name formatting business logic
- `usePatientData.ts` — Hook

---

## Vague Naming

**Violates:** tsr-files-002, tsr-files-015

**What happens:** Files use non-descriptive names like `stuff.tsx`, `temp.ts`, `misc.ts`, `data.ts`.

**Why it's wrong:** Vague names provide no information about the file's purpose or architectural role. This forces developers to open the file to understand what it does.

**Fix:** Use names that describe both domain and role:
- `stuff.tsx` → `PatientSummaryCard.tsx`
- `temp.ts` → `patientApiBroker.ts`
- `data.ts` → `Patient.ts` (model)
- `misc.ts` → Split into specific modules

---

## Wrong Extension

**Violates:** tsr-files-003, tsr-files-004

**What happens:** A component file uses `.ts` instead of `.tsx`, or a non-rendering file uses `.tsx`.

Example:
```typescript
// PatientCard.ts (WRONG - contains JSX)
export const PatientCard = () => {
  return <div>Patient</div>;
};
```

**Why it's wrong:** TypeScript tooling relies on file extensions to determine how to process the file. Using the wrong extension can cause type checking and build issues.

**Fix:**
- If the file contains JSX → use `.tsx`
- If the file is pure TypeScript with no JSX → use `.ts`

---

## Inconsistent Naming Pattern

**Violates:** tsr-files-005 through tsr-files-013

**What happens:** Files don't follow the standard naming pattern for their architectural role.

Examples:
- `patient_api_broker.ts` (snake_case instead of camelCase)
- `PatientService.ts` (PascalCase for service instead of camelCase)
- `Dashboard.tsx` (missing "Page" suffix for a page)
- `usepatientdata.ts` (missing camelCase)

**Why it's wrong:** Inconsistent naming makes it harder to identify file types at a glance and breaks team conventions.

**Fix:** Apply the correct pattern:
- Brokers: `patientApiBroker.ts` (camelCase)
- Services: `patientService.ts` (camelCase)
- Pages: `DashboardPage.tsx` (PascalCase + "Page")
- Components: `PatientCard.tsx` (PascalCase)
- Hooks: `usePatientData.ts` (camelCase with "use" prefix)
