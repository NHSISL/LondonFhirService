---
skill: the-standard-reacttypescript-typescript
type: example
source-section: "2. TypeScript Rules"
demonstrates: "tsr-typescript-004"
---

// ✅ CORRECT: Using `unknown` at unsafe boundaries, never `any`

export class PatientApiBroker implements IPatientApiBroker {
  private readonly baseUrl: string;

  public constructor(baseUrl: string) {
    this.baseUrl = baseUrl;
  }

  public async getPatientAsync(patientId: string): Promise<Patient> {
    const response = await fetch(`${this.baseUrl}/patients/${patientId}`);

    if (!response.ok) {
      throw new Error(`Failed to retrieve patient with id '${patientId}'.`);
    }

    // ✅ CORRECT: Use unknown when receiving external data
    const data: unknown = await response.json();

    // ✅ CORRECT: Validate before using
    return this.mapToPatient(data);
  }

  // ✅ CORRECT: Accept unknown and validate
  private mapToPatient(data: unknown): Patient {
    // Type guard
    if (!this.isPatientData(data)) {
      throw new Error('Invalid patient data received from API.');
    }

    return {
      id: data.id,
      firstName: data.firstName,
      lastName: data.lastName,
      dateOfBirth: data.dateOfBirth
    };
  }

  // ✅ CORRECT: Type guard function
  private isPatientData(data: unknown): data is Patient {
    return (
      typeof data === 'object' &&
      data !== null &&
      'id' in data &&
      typeof (data as any).id === 'string' &&
      'firstName' in data &&
      typeof (data as any).firstName === 'string' &&
      'lastName' in data &&
      typeof (data as any).lastName === 'string' &&
      'dateOfBirth' in data &&
      typeof (data as any).dateOfBirth === 'string'
    );
  }

  // ✅ CORRECT: Handle errors with unknown
  public async handleErrorAsync(error: unknown): Promise<void> {
    if (error instanceof Error) {
      console.error('Error:', error.message);
    } else if (typeof error === 'string') {
      console.error('Error:', error);
    } else {
      console.error('Unknown error:', error);
    }
  }
}

export interface IPatientApiBroker {
  getPatientAsync(patientId: string): Promise<Patient>;
  handleErrorAsync(error: unknown): Promise<void>;
}

export type Patient = {
  id: string;
  firstName: string;
  lastName: string;
  dateOfBirth: string;
};
