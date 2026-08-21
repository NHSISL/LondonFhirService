# The Standard React TypeScript — Foundation Services — Anti-Patterns

## Multiple Brokers

**Violates:** tsr-services-001

**What happens:** Foundation service injects and uses multiple brokers.

Example:
```typescript
// ❌ WRONG
export class PatientService {
  constructor(
    private patientApiBroker: IPatientApiBroker,
    private loggingBroker: ILoggingBroker,
    private storageBroker: IStorageBroker
  ) {}

  async addPatientAsync(patient: Patient): Promise<Patient> {
    this.loggingBroker.logInfo('Adding patient');
    const result = await this.patientApiBroker.postPatientAsync(patient);
    await this.storageBroker.setItemAsync('lastPatient', result);
    return result;
  }
}
```

**Why it's wrong:** Foundation services wrap ONE broker. Multi-broker orchestration happens at processing/orchestration layer.

**Fix:**
```typescript
// ✅ Foundation Service - one broker
export class PatientService {
  constructor(private patientApiBroker: IPatientApiBroker) {}

  async addPatientAsync(patient: Patient): Promise<Patient> {
    this.validatePatient(patient);
    return await this.tryCatch(() => 
      this.patientApiBroker.postPatientAsync(patient)
    );
  }
}

// ✅ Processing Service - orchestrates multiple foundation services
export class PatientProcessingService {
  constructor(
    private patientService: IPatientService,
    private loggingService: ILoggingService,
    private storageService: IStorageService
  ) {}

  async processPatientCreationAsync(patient: Patient): Promise<Patient> {
    this.loggingService.logInfo('Adding patient');
    const result = await this.patientService.addPatientAsync(patient);
    await this.storageService.saveLastPatientAsync(result);
    return result;
  }
}
```

---

## No Validation

**Violates:** tsr-services-002, tsr-services-011, tsr-services-012

**What happens:** Service calls broker without validating inputs.

Example:
```typescript
// ❌ WRONG
export class PatientService {
  async addPatientAsync(patient: Patient): Promise<Patient> {
    // No validation!
    return await this.patientApiBroker.postPatientAsync(patient);
  }

  async retrievePatientByIdAsync(id: string): Promise<Patient> {
    // No ID validation!
    return await this.patientApiBroker.getPatientByIdAsync(id);
  }
}
```

**Why it's wrong:** Services must validate inputs before calling brokers. Prevents invalid data from reaching external systems.

**Fix:**
```typescript
// ✅ CORRECT
export class PatientService {
  async addPatientAsync(patient: Patient): Promise<Patient> {
    this.validatePatient(patient);

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

  private validatePatient(patient: Patient): void {
    if (!patient) {
      throw new PatientValidationException('Patient is required');
    }
    if (!patient.firstName?.trim()) {
      throw new PatientValidationException('First name is required');
    }
    if (!patient.lastName?.trim()) {
      throw new PatientValidationException('Last name is required');
    }
  }

  private validatePatientId(id: string): void {
    if (!id?.trim()) {
      throw new PatientValidationException('Patient ID is required');
    }
  }
}
```

---

## Unwrapped Exceptions

**Violates:** tsr-services-003, tsr-services-017

**What happens:** Broker exceptions escape to caller without mapping.

Example:
```typescript
// ❌ WRONG
export class PatientService {
  async addPatientAsync(patient: Patient): Promise<Patient> {
    this.validatePatient(patient);

    // Broker exception escapes unwrapped!
    return await this.patientApiBroker.postPatientAsync(patient);
  }
}
```

**Why it's wrong:** Caller receives broker-layer exceptions instead of service-layer exceptions. Breaks abstraction and makes error handling inconsistent.

**Fix:**
```typescript
// ✅ CORRECT
export class PatientService {
  async addPatientAsync(patient: Patient): Promise<Patient> {
    this.validatePatient(patient);

    return await this.tryCatch(async () => {
      return await this.patientApiBroker.postPatientAsync(patient);
    });
  }

  private async tryCatch<T>(returningFunction: () => Promise<T>): Promise<T> {
    try {
      return await returningFunction();
    } catch (error) {
      if (error instanceof PatientValidationException) {
        throw error; // Validation exceptions pass through
      }

      // Wrap broker exceptions
      throw new PatientDependencyException(
        'Patient dependency error occurred',
        error as Error
      );
    }
  }
}
```

---

## UI Logic in Service

**Violates:** tsr-services-008

**What happens:** Foundation service contains UI formatting or display logic.

Example:
```typescript
// ❌ WRONG
export class PatientService {
  async getPatientDisplayAsync(id: string): Promise<string> {
    const patient = await this.patientApiBroker.getPatientByIdAsync(id);

    // UI logic in foundation service
    return `${patient.firstName} ${patient.lastName} (Age: ${this.calculateAge(patient.dateOfBirth)})`;
  }

  async getPatientsWithClassNamesAsync(): Promise<PatientWithUI[]> {
    const patients = await this.patientApiBroker.getAllPatientsAsync();

    // UI logic
    return patients.map(p => ({
      ...p,
      statusClassName: p.status === 'active' ? 'text-green-500' : 'text-gray-400'
    }));
  }
}
```

**Why it's wrong:** UI concerns belong in view services, not foundation services. Foundation services handle domain logic only.

**Fix:**
```typescript
// ✅ Foundation Service - domain logic only
export class PatientService {
  async retrievePatientByIdAsync(id: string): Promise<Patient> {
    this.validatePatientId(id);
    return await this.tryCatch(() =>
      this.patientApiBroker.getPatientByIdAsync(id)
    );
  }

  async retrieveAllPatientsAsync(): Promise<Patient[]> {
    return await this.tryCatch(() =>
      this.patientApiBroker.getAllPatientsAsync()
    );
  }
}

// ✅ View Service - UI transformations
export class PatientViewService {
  constructor(private patientService: IPatientService) {}

  async getPatientCardViewAsync(id: string): Promise<PatientCardView> {
    const patient = await this.patientService.retrievePatientByIdAsync(id);

    return {
      id: patient.id,
      displayName: `${patient.firstName} ${patient.lastName}`,
      age: this.calculateAge(patient.dateOfBirth),
      statusClassName: patient.status === 'active' ? 'text-green-500' : 'text-gray-400'
    };
  }
}
```
