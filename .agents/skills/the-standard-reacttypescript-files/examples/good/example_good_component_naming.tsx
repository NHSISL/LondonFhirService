---
skill: the-standard-reacttypescript-files
type: example
source-section: "1. Project Structure"
demonstrates: "tsr-files-010, tsr-files-011"
---

// ✅ CORRECT: Component naming pattern
// File: src/components/patients/PatientCard.tsx

import React from 'react';

export type PatientCardProps = {
  patient: PatientCardView;
  onSelect?: (patientId: string) => void;
};

export type PatientCardView = {
  id: string;
  displayName: string;
  age: number;
  statusClassName: string;
};

export function PatientCard(props: PatientCardProps): JSX.Element {
  const { patient, onSelect } = props;

  const handleClick = () => {
    if (onSelect) {
      onSelect(patient.id);
    }
  };

  return (
    <div className="card" onClick={handleClick}>
      <div className="card-body">
        <h5 className="card-title">{patient.displayName}</h5>
        <p className="card-text">
          Age: <span className={patient.statusClassName}>{patient.age}</span>
        </p>
      </div>
    </div>
  );
}
