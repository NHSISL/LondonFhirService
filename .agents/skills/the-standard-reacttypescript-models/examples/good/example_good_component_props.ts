---
skill: the-standard-reacttypescript-models
type: example
source-section: "3. Models"
demonstrates: "tsr-models-003, tsr-models-007"
---

// ✅ CORRECT: Component props describing rendering input only
// File: src/models/components/patients/PatientCardProps.ts

export type PatientCardProps = {
  patient: PatientCardView;
  onSelect?: (patientId: string) => void;
  onEdit?: (patientId: string) => void;
  onDelete?: (patientId: string) => void;
  isSelected?: boolean;
  isDisabled?: boolean;
  className?: string;
};

export type PatientListProps = {
  patients: PatientCardView[];
  onPatientSelect: (patientId: string) => void;
  selectedPatientId?: string;
  emptyMessage?: string;
  loading?: boolean;
};

export type PatientFormProps = {
  patient?: Patient; // Optional for create scenario
  onSubmit: (patient: Patient) => Promise<void>;
  onCancel: () => void;
  submitButtonText?: string;
  isSubmitting?: boolean;
};

// Why this is correct:
// 1. Describes rendering input only
// 2. Uses view models, not domain models (PatientCardView)
// 3. Callbacks for interaction
// 4. UI state flags (isSelected, isDisabled, loading)
// 5. No business logic
// 6. Optional props where appropriate
// 7. Type keyword for data shape
