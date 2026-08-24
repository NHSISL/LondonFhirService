---
skill: the-standard-reacttypescript-typescript
type: example
source-section: "2. TypeScript Rules"
demonstrates: "tsr-typescript-011 violation"
---

// ❌ WRONG: Unsafe type assertions without validation

export class PatientService {
  public async retrievePatientAsync(patientId: string): Promise<Patient> {
    const response = await fetch(`/api/patients/${patientId}`);
    const data = await response.json();

    // ❌ WRONG: Asserting without validation
    const patient = data as Patient;

    // Runtime error if data doesn't have firstName
    return patient;
  }

  public processPatientData(data: unknown): void {
    // ❌ WRONG: Double assertion (extra dangerous)
    const patient = data as any as Patient;

    // This will crash if data is wrong shape
    console.log(patient.firstName.toUpperCase());
  }

  public getPatientFromStorage(): Patient {
    const stored = localStorage.getItem('patient');

    // ❌ WRONG: Asserting JSON.parse result
    return JSON.parse(stored!) as Patient;
    // - Non-null assertion (!) is also unsafe
    // - No validation that stored is a valid JSON
    // - No validation that result matches Patient shape
  }

  // ❌ WRONG: Type assertion in array map
  public mapPatients(data: unknown[]): Patient[] {
    return data.map(item => item as Patient);
    // No validation that each item is actually a Patient
  }
}

export type Patient = {
  id: string;
  firstName: string;
  lastName: string;
};

// Why this is wrong:
// 1. Type assertions bypass type checking
// 2. Runtime data might not match the asserted type
// 3. Leads to runtime errors that TypeScript can't catch
// 4. False sense of type safety

// Fix: Validate before asserting
function isPatient(data: unknown): data is Patient {
  return (
    typeof data === 'object' &&
    data !== null &&
    'id' in data &&
    'firstName' in data &&
    'lastName' in data
  );
}

export class PatientServiceCorrect {
  public async retrievePatientAsync(patientId: string): Promise<Patient> {
    const response = await fetch(`/api/patients/${patientId}`);
    const data: unknown = await response.json();

    // ✅ Validate before using
    if (!isPatient(data)) {
      throw new Error('Invalid patient data received.');
    }

    return data; // Now safe
  }
}
