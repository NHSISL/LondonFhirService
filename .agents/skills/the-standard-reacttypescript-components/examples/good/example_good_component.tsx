---
skill: the-standard-reacttypescript-components
type: example
source-section: "Components"
demonstrates: "tsr-comp-001, tsr-comp-002, tsr-comp-003, tsr-comp-006, tsr-comp-012"
---

// ✅ CORRECT: Pure rendering component

import type { PatientCardProps } from '../../models/components/patients/PatientCardProps';

export function PatientCard({ 
  patient, 
  onSelect, 
  onEdit, 
  isSelected,
  className 
}: PatientCardProps) {
  return (
    <div 
      className={`patient-card ${className || ''} ${isSelected ? 'selected' : ''}`}
      onClick={() => onSelect?.(patient.id)}
    >
      <div className="patient-header">
        <h3>{patient.displayName}</h3>
        <span className={patient.statusClassName}>
          {patient.statusDisplayText}
        </span>
      </div>

      <div className="patient-details">
        <p>Age: {patient.age}</p>
        <p>{patient.contactInfo}</p>
      </div>

      {onEdit && (
        <button onClick={(e) => {
          e.stopPropagation();
          onEdit(patient.id);
        }}>
          Edit
        </button>
      )}
    </div>
  );
}

// Why this is correct:
// 1. Pure rendering function (tsr-comp-001)
// 2. All data via props (tsr-comp-002)
// 3. TypeScript props (tsr-comp-003)
// 4. Named export (tsr-comp-006)
// 5. Props destructured (tsr-comp-012)
// 6. View model used (patient is PatientCardView) (tsr-comp-020)
// 7. Callbacks optional (tsr-comp-018)
// 8. No business logic (tsr-comp-009)
// 9. No data fetching (tsr-comp-010)
