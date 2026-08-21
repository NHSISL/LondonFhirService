---
skill: the-standard-reacttypescript-files
type: example
source-section: "1. Project Structure"
demonstrates: "tsr-files-001 violation"
---

// ❌ WRONG: Mixed concerns in a single file
// File: src/patientEverything.tsx

import React, { useState, useEffect } from 'react';

// Concern 1: API Broker (should be in patientApiBroker.ts)
export const fetchPatients = async (): Promise<Patient[]> => {
  const response = await fetch('/api/patients');
  return await response.json();
};

// Concern 2: Business Logic (should be in patientService.ts)
export const validatePatient = (patient: Patient): string[] => {
  const errors: string[] = [];

  if (!patient.firstName || patient.firstName.trim() === '') {
    errors.push('First name is required');
  }

  if (!patient.lastName || patient.lastName.trim() === '') {
    errors.push('Last name is required');
  }

  return errors;
};

// Concern 3: View Model Transformation (should be in patientViewService.ts)
export const transformToPatientCardView = (patient: Patient): PatientCardView => {
  return {
    id: patient.id,
    displayName: `${patient.firstName} ${patient.lastName}`,
    age: calculateAge(patient.dateOfBirth),
    statusClassName: patient.age >= 18 ? 'text-success' : 'text-warning'
  };
};

// Concern 4: Component (should be in PatientCard.tsx)
export function PatientCard({ patient }: { patient: Patient }) {
  return (
    <div className="card">
      <div className="card-body">
        <h5>{patient.firstName} {patient.lastName}</h5>
      </div>
    </div>
  );
}

// Concern 5: Hook (should be in usePatientList.ts)
export function usePatientList() {
  const [patients, setPatients] = useState<Patient[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchPatients()
      .then(setPatients)
      .finally(() => setLoading(false));
  }, []);

  return { patients, loading };
}

// Concern 6: Utility (should be in dateTimeBroker.ts)
const calculateAge = (dateOfBirth: string): number => {
  const today = new Date();
  const birthDate = new Date(dateOfBirth);
  let age = today.getFullYear() - birthDate.getFullYear();
  return age;
};

// Types (should be in models/)
export type Patient = {
  id: string;
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  age: number;
};

export type PatientCardView = {
  id: string;
  displayName: string;
  age: number;
  statusClassName: string;
};

// Why this is wrong:
// This file mixes 6 different architectural concerns in one place.
// It has no clear identity and violates single responsibility principle.
//
// Fix: Split into properly named files following their architectural roles.
