---
skill: the-standard-reacttypescript-models
type: example
source-section: "3. Models"
demonstrates: "tsr-models-001, tsr-models-002, tsr-models-006 violation"
---

// ❌ WRONG: Mixing foundation and view concerns in one model

export type Patient = {
  // Foundation fields
  id: string;
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  email: string;
  status: number;

  // ❌ View concerns mixed in
  displayName: string; // Should be in view model
  age: number; // Calculated field, belongs in view model
  statusClassName: string; // UI concern
  statusDisplayText: string; // UI concern
  formattedDateOfBirth: string; // Presentation concern

  // ❌ Component state mixed in
  isSelected: boolean; // Component state, not model data
  isExpanded: boolean; // UI state, not domain data
};

// Why this is wrong:
// 1. Mixes domain model with view model
// 2. Mixes domain model with UI state
// 3. Can't use in non-UI contexts (services, tests)
// 4. Violates separation of concerns
// 5. Creates maintenance confusion
// 6. Makes the model too large and complex

// Fix: Separate into proper layers
// Foundation model: src/models/foundations/patients/Patient.ts
export type PatientCorrect = {
  id: string;
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  email: string;
  status: PatientStatus;
};

// View model: src/models/views/patients/PatientCardView.ts
export type PatientCardView = {
  id: string;
  displayName: string;
  age: number;
  statusClassName: string;
  statusDisplayText: string;
  formattedDateOfBirth: string;
};

// Component props: src/models/components/patients/PatientCardProps.ts
export type PatientCardProps = {
  patient: PatientCardView;
  isSelected?: boolean;
  isExpanded?: boolean;
  onSelect?: (id: string) => void;
};

export type PatientStatus = 'active' | 'inactive' | 'archived';
