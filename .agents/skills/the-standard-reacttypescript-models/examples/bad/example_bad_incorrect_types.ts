---
skill: the-standard-reacttypescript-models
type: example
source-section: "3. Models"
demonstrates: "tsr-models-011, tsr-models-012 violation"
---

// ❌ WRONG: Incorrect optionality and using any

export type Patient = {
  id?: string; // ❌ Always required, but marked optional
  firstName?: string; // ❌ Always required, but marked optional
  lastName?: string; // ❌ Always required, but marked optional
  middleName?: string; // ✅ Correctly optional
  dateOfBirth?: string; // ❌ Always required, but marked optional
  email?: string; // ❌ Always required, but marked optional
  metadata: any; // ❌ Using any instead of explicit type
  settings: any; // ❌ Using any instead of explicit type
  tags?: any[]; // ❌ Using any array
};

// This leads to unnecessary checks:
function displayPatient(patient: Patient): string {
  // Have to check everything even though they're never actually missing
  if (!patient.firstName || !patient.lastName) {
    return 'Unknown Patient';
  }

  return `${patient.firstName} ${patient.lastName}`;
}

// Why this is wrong:
// 1. Incorrect optionality forces unnecessary null checks
// 2. Hides true requirements
// 3. any defeats type safety
// 4. No documentation of structure
// 5. Runtime errors possible

// Fix: Explicit types and correct optionality
export type PatientCorrect = {
  id: string; // Required
  firstName: string; // Required
  lastName: string; // Required
  middleName?: string; // Truly optional
  dateOfBirth: string; // Required
  email: string; // Required
  metadata: PatientMetadata; // Explicit type
  settings: PatientSettings; // Explicit type
  tags?: string[]; // Typed array, optional
};

export type PatientMetadata = {
  createdAt: string;
  createdBy: string;
  updatedAt: string;
  updatedBy: string;
  version: number;
};

export type PatientSettings = {
  notificationsEnabled: boolean;
  preferredLanguage: 'en' | 'es' | 'fr';
  timezone: string;
};

// Now no unnecessary checks needed:
function displayPatientCorrect(patient: PatientCorrect): string {
  // TypeScript guarantees firstName and lastName exist
  return `${patient.firstName} ${patient.lastName}`;
}
