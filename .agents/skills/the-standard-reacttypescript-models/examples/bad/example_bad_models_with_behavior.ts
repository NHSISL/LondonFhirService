---
skill: the-standard-reacttypescript-models
type: example
source-section: "3. Models"
demonstrates: "tsr-models-005 violation"
---

// ❌ WRONG: Models with behavior (methods, functions, API calls)

export type Patient = {
  id: string;
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  email: string;

  // ❌ Methods in model
  getFullName(): string {
    return `${this.firstName} ${this.lastName}`;
  }

  getAge(): number {
    const today = new Date();
    const birthDate = new Date(this.dateOfBirth);
    let age = today.getFullYear() - birthDate.getFullYear();
    const monthDiff = today.getMonth() - birthDate.getMonth();

    if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birthDate.getDate())) {
      age--;
    }

    return age;
  }

  // ❌ API call in model
  async save(): Promise<void> {
    await fetch('/api/patients', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(this)
    });
  }

  // ❌ Business logic in model
  isEligibleForDiscount(): boolean {
    return this.getAge() >= 65;
  }
};

// Why this is wrong:
// 1. Models should be data-only contracts
// 2. Behavior belongs in services
// 3. Can't serialize models with methods
// 4. Makes testing difficult
// 5. Violates single responsibility
// 6. Can't use as plain TypeScript type

// Fix: Move behavior to services
// Model - data only
export type PatientCorrect = {
  id: string;
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  email: string;
};

// View service - transformations
export class PatientViewService {
  public getFullName(patient: PatientCorrect): string {
    return `${patient.firstName} ${patient.lastName}`;
  }

  public calculateAge(patient: PatientCorrect): number {
    const today = new Date();
    const birthDate = new Date(patient.dateOfBirth);
    let age = today.getFullYear() - birthDate.getFullYear();
    const monthDiff = today.getMonth() - birthDate.getMonth();

    if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birthDate.getDate())) {
      age--;
    }

    return age;
  }

  public transformToPatientCardView(patient: PatientCorrect): PatientCardView {
    return {
      id: patient.id,
      displayName: this.getFullName(patient),
      age: this.calculateAge(patient),
      // ... other view transformations
    };
  }
}

// Foundation service - business logic
export class PatientService {
  constructor(private patientApiBroker: PatientApiBroker) {}

  public async savePatientAsync(patient: PatientCorrect): Promise<void> {
    await this.patientApiBroker.postPatientAsync(patient);
  }

  public isEligibleForDiscount(patient: PatientCorrect): boolean {
    const age = new PatientViewService().calculateAge(patient);
    return age >= 65;
  }
}

export type PatientCardView = {
  id: string;
  displayName: string;
  age: number;
};

interface PatientApiBroker {
  postPatientAsync(patient: PatientCorrect): Promise<void>;
}
