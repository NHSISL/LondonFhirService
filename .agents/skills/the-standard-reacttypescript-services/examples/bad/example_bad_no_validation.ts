---
skill: the-standard-reacttypescript-services
type: example
source-section: "Foundation Services"
demonstrates: "tsr-services-002, tsr-services-011, tsr-services-012, tsr-services-013 violation"
---

// ❌ WRONG: No input validation

export class PatientService {
  constructor(private patientApiBroker: IPatientApiBroker) {}

  // ❌ WRONG: No validation at all
  async addPatientAsync(patient: Patient): Promise<Patient> {
    return await this.patientApiBroker.postPatientAsync(patient);
  }

  // ❌ WRONG: No ID validation
  async retrievePatientByIdAsync(id: string): Promise<Patient> {
    return await this.patientApiBroker.getPatientByIdAsync(id);
  }

  // ❌ WRONG: No null check
  async modifyPatientAsync(patient: Patient): Promise<Patient> {
    return await this.patientApiBroker.putPatientAsync(patient);
  }

  // ❌ WRONG: Partial validation (missing required fields check)
  async partialValidationAsync(patient: Patient): Promise<Patient> {
    if (!patient) {
      throw new Error('Patient required');
    }

    // Missing validation for firstName, lastName, email, etc.
    return await this.patientApiBroker.postPatientAsync(patient);
  }
}

// Why this is wrong:
// 1. Allows null/undefined to reach broker (tsr-services-011)
// 2. Allows empty strings for required fields (tsr-services-012)
// 3. Allows empty IDs for retrieval (tsr-services-013)
// 4. No validation before broker calls (tsr-services-016)
// 5. Invalid data can corrupt external systems
// 6. Poor error messages for users

// Fix: Add comprehensive validation
export class PatientServiceCorrect {
  constructor(private patientApiBroker: IPatientApiBroker) {}

  async addPatientAsync(patient: Patient): Promise<Patient> {
    this.validatePatientOnAdd(patient);

    return await this.tryCatch(() =>
      this.patientApiBroker.postPatientAsync(patient)
    );
  }

  async retrievePatientByIdAsync(id: string): Promise<Patient> {
    this.validatePatientId(id);

    return await this.tryCatch(() =>
      this.patientApiBroker.getPatientByIdAsync(id)
    );
  }

  async modifyPatientAsync(patient: Patient): Promise<Patient> {
    this.validatePatientOnModify(patient);

    return await this.tryCatch(() =>
      this.patientApiBroker.putPatientAsync(patient)
    );
  }

  private validatePatientOnAdd(patient: Patient): void {
    if (!patient) {
      throw new PatientValidationException('Patient is required');
    }

    if (!patient.firstName?.trim()) {
      throw new PatientValidationException('First name is required', 'firstName');
    }

    if (!patient.lastName?.trim()) {
      throw new PatientValidationException('Last name is required', 'lastName');
    }

    if (!patient.email?.trim()) {
      throw new PatientValidationException('Email is required', 'email');
    }
  }

  private validatePatientOnModify(patient: Patient): void {
    this.validatePatientOnAdd(patient);
    this.validatePatientId(patient.id);
  }

  private validatePatientId(id: string): void {
    if (!id?.trim()) {
      throw new PatientValidationException('Patient ID is required', 'id');
    }
  }

  private async tryCatch<T>(returningFunction: () => Promise<T>): Promise<T> {
    try {
      return await returningFunction();
    } catch (error) {
      if (error instanceof PatientValidationException) {
        throw error;
      }
      throw new PatientDependencyException(
        'Patient dependency error occurred',
        error as Error
      );
    }
  }
}

type Patient = {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
};

interface IPatientApiBroker {
  postPatientAsync(patient: Patient): Promise<Patient>;
  getPatientByIdAsync(id: string): Promise<Patient>;
  putPatientAsync(patient: Patient): Promise<Patient>;
}

class PatientValidationException extends Error {
  constructor(message: string, public field?: string) {
    super(message);
  }
}

class PatientDependencyException extends Error {
  constructor(message: string, public innerException: Error) {
    super(message);
  }
}
