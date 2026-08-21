---
skill: the-standard-reacttypescript-typescript
type: example
source-section: "2. TypeScript Rules"
demonstrates: "tsr-typescript-003"
---

// ✅ CORRECT: Using `type` for data shapes and `interface` for contracts

// ✅ Data shapes - use `type`
export type Patient = {
  id: string;
  firstName: string;
  lastName: string;
  dateOfBirth: string;
};

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

// ✅ Behavioral contracts - use `interface`
export interface IPatientService {
  retrievePatientAsync(patientId: string): Promise<Patient>;
  retrieveAllPatientsAsync(): Promise<Patient[]>;
}

export interface IPatientApiBroker {
  getPatientAsync(patientId: string): Promise<Patient>;
  getAllPatientsAsync(): Promise<Patient[]>;
}

export interface IDateTimeBroker {
  getCurrentDate(): Date;
  calculateAge(dateOfBirth: Date): number;
}

// ✅ When to use each:
// - `type` for:
//   - Data models
//   - Component props
//   - View models
//   - Union types
//   - Mapped types
//
// - `interface` for:
//   - Service contracts
//   - Broker contracts
//   - Object-oriented contracts
//   - Extensible contracts (can be extended by others)
