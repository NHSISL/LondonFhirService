# The Standard React TypeScript — TypeScript — Anti-Patterns

## Any Type

**Violates:** tsr-typescript-004

**What happens:** The code uses `any` as a type.

Example:
```typescript
export async function retrievePatientAsync(id: any): Promise<any> {
  const response = await fetch(`/api/patients/${id}`);
  return await response.json();
}
```

**Why it's wrong:** `any` disables all type checking and defeats the purpose of TypeScript. It hides bugs and makes refactoring dangerous.

**Fix:** Use explicit types or `unknown` for truly dynamic data:
```typescript
export async function retrievePatientAsync(
  patientId: string
): Promise<Patient> {
  const response = await fetch(`/api/patients/${patientId}`);
  return await response.json() as Patient;
}
```

---

## Suppressed Errors

**Violates:** tsr-typescript-010

**What happens:** TypeScript errors are suppressed with `@ts-ignore` or `@ts-expect-error` without explanation.

Example:
```typescript
// @ts-ignore
const result = someUnsafeOperation();
```

**Why it's wrong:** Error suppression hides real problems and makes code unmaintainable. Future developers won't know why the error was suppressed.

**Fix:** Either fix the underlying type issue or document why suppression is necessary:
```typescript
// @ts-expect-error - Legacy API returns incorrect type, fixed in v2.0
// TODO: Remove after API v2.0 migration (ticket #1234)
const result = someUnsafeOperation() as ExpectedType;
```

---

## Unsafe Assertions

**Violates:** tsr-typescript-011

**What happens:** Type assertions are used without runtime validation.

Example:
```typescript
const patient = data as Patient;
patient.firstName.toUpperCase(); // Runtime error if data is wrong shape
```

**Why it's wrong:** Type assertions bypass type checking. If the runtime data doesn't match the assertion, you get runtime errors.

**Fix:** Validate before asserting, or use type guards:
```typescript
function isPatient(data: unknown): data is Patient {
  return (
    typeof data === 'object' &&
    data !== null &&
    'firstName' in data &&
    typeof (data as Patient).firstName === 'string'
  );
}

if (isPatient(data)) {
  data.firstName.toUpperCase(); // Safe
}
```

---

## Barrel Files Hiding Dependencies

**Violates:** tsr-typescript-012

**What happens:** An `index.ts` file re-exports everything, hiding which layer depends on which.

Example:
```typescript
// src/services/index.ts
export * from './foundations/patientService';
export * from './views/dashboardViewService';

// Elsewhere - can't tell which layer this comes from
import { patientService } from '../services';
```

**Why it's wrong:** Barrel files make it unclear which architectural layer is being imported. This can lead to circular dependencies and layer violations.

**Fix:** Import directly from the specific file:
```typescript
import { patientService } from '../services/foundations/patientService';
import { dashboardViewService } from '../services/views/dashboardViewService';
```

---

## Implicit Return Types

**Violates:** tsr-typescript-014

**What happens:** Functions at architectural boundaries don't declare their return types.

Example:
```typescript
// What does this return? Have to read the implementation
export async function retrievePatientAsync(patientId: string) {
  return await broker.getPatientAsync(patientId);
}
```

**Why it's wrong:** Implicit return types make the contract unclear and can accidentally change if the implementation changes.

**Fix:** Explicitly declare return types:
```typescript
export async function retrievePatientAsync(
  patientId: string
): Promise<Patient> {
  return await broker.getPatientAsync(patientId);
}
```

---

## Generic Function/Object Types

**Violates:** tsr-typescript-016

**What happens:** Code uses `Function` or `Object` as types instead of specific signatures.

Example:
```typescript
type PatientCardProps = {
  patient: Patient;
  onClick: Function; // Too generic
  metadata: Object; // Too generic
};
```

**Why it's wrong:** Generic types provide no useful type information and don't enforce correct usage.

**Fix:** Use specific types:
```typescript
type PatientCardProps = {
  patient: Patient;
  onClick: (patientId: string) => void;
  metadata: {
    createdAt: string;
    updatedAt: string;
  };
};
```
