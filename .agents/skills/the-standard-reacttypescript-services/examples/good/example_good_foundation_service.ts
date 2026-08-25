---
skill: the-standard-reacttypescript-services
type: example
source-section: "Foundation Services"
demonstrates: "tsr-services-001, tsr-services-002, tsr-services-003, tsr-services-004, tsr-services-007"
---

// ✅ CORRECT: Complete foundation service implementation
// File: src/services/foundations/patients/PatientService.ts

import type { Patient } from '../../../models/foundations/patients/Patient';
import type { IPatientApiBroker } from '../../../brokers/apis/IPatientApiBroker';

export interface IPatientService {
  addPatientAsync(patient: Patient): Promise<Patient>;
  retrieveAllPatientsAsync(): Promise<Patient[]>;
  retrievePatientByIdAsync(id: string): Promise<Patient>;
  modifyPatientAsync(patient: Patient): Promise<Patient>;
  removePatientByIdAsync(id: string): Promise<void>;
}

export class PatientService implements IPatientService {
  constructor(private patientApiBroker: IPatientApiBroker) {}

  async addPatientAsync(patient: Patient): Promise<Patient> {
    this.validatePatientOnAdd(patient);

    return await this.tryCatch(async () => {
      return await this.patientApiBroker.postPatientAsync(patient);
    });
  }

  async retrieveAllPatientsAsync(): Promise<Patient[]> {
    return await this.tryCatch(async () => {
      return await this.patientApiBroker.getAllPatientsAsync();
    });
  }

  async retrievePatientByIdAsync(id: string): Promise<Patient> {
    this.validatePatientId(id);

    return await this.tryCatch(async () => {
      return await this.patientApiBroker.getPatientByIdAsync(id);
    });
  }

  async modifyPatientAsync(patient: Patient): Promise<Patient> {
    this.validatePatientOnModify(patient);

    return await this.tryCatch(async () => {
      return await this.patientApiBroker.putPatientAsync(patient);
    });
  }

  async removePatientByIdAsync(id: string): Promise<void> {
    this.validatePatientId(id);

    return await this.tryCatch(async () => {
      return await this.patientApiBroker.deletePatientByIdAsync(id);
    });
  }

  // Validation methods
  private validatePatientOnAdd(patient: Patient): void {
    this.validatePatientIsNotNull(patient);
    this.validatePatientRequiredFields(patient);
  }

  private validatePatientOnModify(patient: Patient): void {
    this.validatePatientIsNotNull(patient);
    this.validatePatientId(patient.id);
    this.validatePatientRequiredFields(patient);
  }

  private validatePatientIsNotNull(patient: Patient): void {
    if (!patient) {
      throw new PatientValidationException('Patient is required');
    }
  }

  private validatePatientRequiredFields(patient: Patient): void {
    if (!patient.firstName?.trim()) {
      throw new PatientValidationException(
        'Patient first name is required',
        'firstName'
      );
    }

    if (!patient.lastName?.trim()) {
      throw new PatientValidationException(
        'Patient last name is required',
        'lastName'
      );
    }

    if (!patient.email?.trim()) {
      throw new PatientValidationException(
        'Patient email is required',
        'email'
      );
    }

    if (!patient.dateOfBirth?.trim()) {
      throw new PatientValidationException(
        'Patient date of birth is required',
        'dateOfBirth'
      );
    }
  }

  private validatePatientId(id: string): void {
    if (!id?.trim()) {
      throw new PatientValidationException(
        'Patient ID is required',
        'id'
      );
    }
  }

  // TryCatch wrapper for exception mapping
  private async tryCatch<T>(returningFunction: () => Promise<T>): Promise<T> {
    try {
      return await returningFunction();
    } catch (error) {
      if (error instanceof PatientValidationException) {
        throw error; // Validation exceptions pass through
      }

      if (this.isBrokerException(error)) {
        throw new PatientDependencyException(
          'Patient dependency error occurred, contact support',
          error as Error
        );
      }

      throw new PatientServiceException(
        'Patient service error occurred, contact support',
        error as Error
      );
    }
  }

  private isBrokerException(error: unknown): boolean {
    return error instanceof Error && 
           error.name.includes('BrokerException');
  }
}

// Exception types
export class PatientValidationException extends Error {
  constructor(message: string, public field?: string) {
    super(message);
    this.name = 'PatientValidationException';
  }
}

export class PatientDependencyException extends Error {
  constructor(message: string, public innerException: Error) {
    super(message);
    this.name = 'PatientDependencyException';
  }
}

export class PatientServiceException extends Error {
  constructor(message: string, public innerException: Error) {
    super(message);
    this.name = 'PatientServiceException';
  }
}

// Why this is correct:
// 1. Wraps exactly one broker (tsr-services-001)
// 2. All inputs validated (tsr-services-002)
// 3. Exceptions mapped via TryCatch (tsr-services-003, tsr-services-007)
// 4. Try-catch pattern for broker calls (tsr-services-004)
// 5. Broker injected (tsr-services-005)
// 6. Method names mirror broker (tsr-services-006)
// 7. Clear exception types (tsr-services-010)
// 8. Null validation (tsr-services-011)
// 9. Required fields validated (tsr-services-012)
// 10. ID validation (tsr-services-013)
// 11. Validation includes field names (tsr-services-015)
// 12. Validation before broker calls (tsr-services-016)
// 13. Inner exceptions preserved (tsr-services-020)
