---
skill: the-standard-reacttypescript-brokers
type: example
source-section: "Brokers"
demonstrates: "tsr-brokers-008 violation"
---

// ❌ WRONG: Business logic in broker

export class PatientApiBroker {
  private baseUrl = import.meta.env.VITE_API_BASE_URL;

  // ❌ WRONG: Filtering (business logic) in broker
  async getActivePatientsAsync(): Promise<Patient[]> {
    const response = await fetch(`${this.baseUrl}/api/patients`);
    const patients: Patient[] = await response.json();

    return patients.filter(p => p.status === 'active'); // Business logic
  }

  // ❌ WRONG: Validation (business logic) in broker
  async createPatientAsync(patient: Patient): Promise<Patient> {
    if (!patient.email.includes('@')) { // Validation logic
      throw new Error('Invalid email');
    }

    const response = await fetch(`${this.baseUrl}/api/patients`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(patient)
    });

    return await response.json();
  }

  // ❌ WRONG: Transformation (business logic) in broker
  async getPatientDisplayNamesAsync(): Promise<string[]> {
    const response = await fetch(`${this.baseUrl}/api/patients`);
    const patients: Patient[] = await response.json();

    // Transformation logic
    return patients.map(p => `${p.firstName} ${p.lastName}`);
  }

  // ❌ WRONG: Calculated field (business logic) in broker
  async getPatientsWithAgesAsync(): Promise<PatientWithAge[]> {
    const response = await fetch(`${this.baseUrl}/api/patients`);
    const patients: Patient[] = await response.json();

    // Calculation logic
    return patients.map(p => ({
      ...p,
      age: this.calculateAge(p.dateOfBirth)
    }));
  }

  private calculateAge(dateOfBirth: string): number {
    const today = new Date();
    const birthDate = new Date(dateOfBirth);
    let age = today.getFullYear() - birthDate.getFullYear();
    const monthDiff = today.getMonth() - birthDate.getMonth();

    if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birthDate.getDate())) {
      age--;
    }

    return age;
  }
}

// Why this is wrong:
// 1. Filtering is business logic - belongs in foundation service
// 2. Validation is business logic - belongs in foundation service
// 3. Transformation is business logic - belongs in view service
// 4. Calculations are business logic - belong in service
// 5. Makes broker untestable in isolation
// 6. Violates single responsibility (I/O + logic)

// Fix: Move all logic to services
export class PatientApiBrokerCorrect {
  private baseUrl = import.meta.env.VITE_API_BASE_URL;

  // ✅ CORRECT: I/O only
  async getAllPatientsAsync(): Promise<Patient[]> {
    const response = await fetch(`${this.baseUrl}/api/patients`);
    return await response.json();
  }

  async postPatientAsync(patient: Patient): Promise<Patient> {
    const response = await fetch(`${this.baseUrl}/api/patients`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(patient)
    });

    return await response.json();
  }
}

// ✅ CORRECT: Business logic in service
export class PatientService {
  constructor(private patientApiBroker: PatientApiBrokerCorrect) {}

  async getActivePatientsAsync(): Promise<Patient[]> {
    const patients = await this.patientApiBroker.getAllPatientsAsync();
    return patients.filter(p => p.status === 'active');
  }

  async createPatientAsync(patient: Patient): Promise<Patient> {
    this.validatePatient(patient);
    return await this.patientApiBroker.postPatientAsync(patient);
  }

  private validatePatient(patient: Patient): void {
    if (!patient.email.includes('@')) {
      throw new PatientValidationException('Invalid email');
    }
  }
}

type Patient = {
  id: string;
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  email: string;
  status: 'active' | 'inactive';
};

type PatientWithAge = Patient & { age: number };

class PatientValidationException extends Error {}
