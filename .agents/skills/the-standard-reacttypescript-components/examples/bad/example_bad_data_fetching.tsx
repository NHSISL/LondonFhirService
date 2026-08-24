---
skill: the-standard-reacttypescript-components
type: example
source-section: "Components"
demonstrates: "tsr-comp-002, tsr-comp-008, tsr-comp-010 violation"
---

// ❌ WRONG: Component fetching data

import { useEffect, useState } from 'react';
import { PatientService } from '../../services/foundations/patients/PatientService';

export function PatientCard({ id }: { id: string }) {
  const [patient, setPatient] = useState(null);
  const patientService = new PatientService(); // ❌ Direct service instantiation

  // ❌ useEffect data fetching
  useEffect(() => {
    async function loadPatient() {
      const data = await patientService.retrievePatientByIdAsync(id); // ❌ Service call
      setPatient(data);
    }
    loadPatient();
  }, [id]);

  if (!patient) return <div>Loading...</div>;

  return <div>{patient.firstName}</div>;
}

// ✅ CORRECT: Receive data via props
export function PatientCardCorrect({ patient }: PatientCardProps) {
  return <div>{patient.displayName}</div>;
}
