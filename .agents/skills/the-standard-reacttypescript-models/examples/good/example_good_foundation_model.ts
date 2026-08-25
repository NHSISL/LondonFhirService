---
skill: the-standard-reacttypescript-models
type: example
source-section: "3. Models"
demonstrates: "tsr-models-001, tsr-models-007, tsr-models-008"
---

// ✅ CORRECT: Foundation model representing domain concept
// File: src/models/foundations/patients/Patient.ts

export type Patient = {
  id: string;
  firstName: string;
  lastName: string;
  dateOfBirth: string; // ISO 8601 string at API boundary
  email: string;
  phoneNumber: string;
  status: PatientStatus;
  insuranceId?: string; // Truly optional
};

export type PatientStatus = 'active' | 'inactive' | 'archived';

// Related error model (separate file recommended)
export type PatientError = {
  code: string;
  message: string;
  field?: string;
};

// Why this is correct:
// 1. Represents domain concept (Patient)
// 2. Uses type keyword for data shape
// 3. Explicit types for all properties
// 4. No behavior/methods
// 5. Optional only for truly optional fields
// 6. Union type for status (not string)
// 7. ISO date string at boundary
