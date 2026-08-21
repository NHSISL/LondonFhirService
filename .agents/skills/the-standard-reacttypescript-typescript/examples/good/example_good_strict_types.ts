---
skill: the-standard-reacttypescript-typescript
type: example
source-section: "2. TypeScript Rules"
demonstrates: "tsr-typescript-002, tsr-typescript-003, tsr-typescript-014"
---

// ✅ CORRECT: Explicit types at architectural boundaries

// Data shape - use `type`
export type Patient = {
  id: string;
  firstName: string;
  lastName: string;
  dateOfBirth: string;
};

// Behavioral contract - use `interface`
export interface IPatientService {
  retrievePatientAsync(patientId: string): Promise<Patient>;
  updatePatientAsync(patient: Patient): Promise<Patient>;
  deletePatientAsync(patientId: string): Promise<void>;
}

// Service implementation with explicit types
export class PatientService implements IPatientService {
  private readonly patientApiBroker: IPatientApiBroker;

  public constructor(patientApiBroker: IPatientApiBroker) {
    this.patientApiBroker = patientApiBroker;
  }

  // ✅ Explicit parameter types and return type
  public async retrievePatientAsync(
    patientId: string
  ): Promise<Patient> {
    this.validatePatientId(patientId);

    return await this.patientApiBroker.getPatientAsync(patientId);
  }

  // ✅ Explicit parameter types and return type
  public async updatePatientAsync(
    patient: Patient
  ): Promise<Patient> {
    this.validatePatient(patient);

    return await this.patientApiBroker.putPatientAsync(patient);
  }

  // ✅ Explicit void return type
  public async deletePatientAsync(
    patientId: string
  ): Promise<void> {
    this.validatePatientId(patientId);

    await this.patientApiBroker.deletePatientAsync(patientId);
  }

  private validatePatientId(patientId: string): void {
    if (!patientId || patientId.trim() === '') {
      throw new Error('Patient ID is required.');
    }
  }

  private validatePatient(patient: Patient): void {
    if (!patient) {
      throw new Error('Patient is required.');
    }

    this.validatePatientId(patient.id);
  }
}

export interface IPatientApiBroker {
  getPatientAsync(patientId: string): Promise<Patient>;
  putPatientAsync(patient: Patient): Promise<Patient>;
  deletePatientAsync(patientId: string): Promise<void>;
}
