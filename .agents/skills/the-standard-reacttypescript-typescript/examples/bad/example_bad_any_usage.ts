---
skill: the-standard-reacttypescript-typescript
type: example
source-section: "2. TypeScript Rules"
demonstrates: "tsr-typescript-004 violation"
---

// ❌ WRONG: Using `any` instead of proper types

// ❌ WRONG: any parameters and return type
export async function retrievePatientAsync(id: any): Promise<any> {
  const response = await fetch(`/api/patients/${id}`);
  return await response.json();
}

// ❌ WRONG: any in type definition
export type PatientService = {
  getPatient: (id: any) => Promise<any>;
  updatePatient: (data: any) => Promise<any>;
};

// ❌ WRONG: any in component props
export type PatientCardProps = {
  patient: any;
  onClick: any;
};

// ❌ WRONG: any in error handling
export async function handleError(error: any) {
  console.log(error.message); // What if error is not an Error object?
}

// ❌ WRONG: any in array
export const patients: any[] = [];

// ❌ WRONG: any in generic
export function processData<T = any>(data: T): T {
  return data;
}

// Why this is wrong:
// 1. Defeats the purpose of TypeScript
// 2. Hides bugs at compile time
// 3. Makes refactoring dangerous
// 4. No intellisense or autocomplete
// 5. Runtime errors that could have been caught

// Fix: Use proper types
export type Patient = {
  id: string;
  firstName: string;
  lastName: string;
};

export async function retrievePatientAsyncCorrect(
  patientId: string
): Promise<Patient> {
  const response = await fetch(`/api/patients/${patientId}`);
  const data: unknown = await response.json();

  // Validate and map
  return data as Patient; // With proper validation
}
