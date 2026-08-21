# The Standard React TypeScript — Brokers — Anti-Patterns

## Business Logic in Broker

**Violates:** tsr-brokers-008

**What happens:** Broker contains filtering, validation, or business rules.

Example:
```typescript
// ❌ WRONG
export class PatientApiBroker {
  async getActivePatientsAsync(): Promise<Patient[]> {
    const response = await fetch(`${this.baseUrl}/patients`);
    const patients: Patient[] = await response.json();

    // Business logic in broker
    return patients.filter(p => p.status === 'active');
  }
}
```

**Why it's wrong:** Brokers handle I/O only. Business logic belongs in services. This violates single responsibility and makes testing harder.

**Fix:**
```typescript
// Broker - I/O only
export class PatientApiBroker {
  async getAllPatientsAsync(): Promise<Patient[]> {
    const response = await fetch(`${this.baseUrl}/patients`);
    return await response.json();
  }
}

// Service - business logic
export class PatientService {
  async getActivePatientsAsync(): Promise<Patient[]> {
    const patients = await this.patientApiBroker.getAllPatientsAsync();
    return patients.filter(p => p.status === 'active');
  }
}
```

---

## Broker Calling Broker

**Violates:** tsr-brokers-019

**What happens:** One broker calls another broker.

Example:
```typescript
// ❌ WRONG
export class PatientApiBroker {
  constructor(private loggingBroker: LoggingBroker) {}

  async getPatientAsync(id: string): Promise<Patient> {
    this.loggingBroker.logInfo(`Fetching patient ${id}`); // Broker calling broker
    const response = await fetch(`${this.baseUrl}/patients/${id}`);
    return await response.json();
  }
}
```

**Why it's wrong:** Brokers should be independent. Composition happens in services, not brokers.

**Fix:**
```typescript
// Broker - independent
export class PatientApiBroker {
  async getPatientAsync(id: string): Promise<Patient> {
    const response = await fetch(`${this.baseUrl}/patients/${id}`);
    return await response.json();
  }
}

// Service - composes brokers
export class PatientService {
  constructor(
    private patientApiBroker: PatientApiBroker,
    private loggingBroker: LoggingBroker
  ) {}

  async getPatientByIdAsync(id: string): Promise<Patient> {
    this.loggingBroker.logInfo(`Fetching patient ${id}`);
    return await this.patientApiBroker.getPatientAsync(id);
  }
}
```

---

## Swallowed Exceptions

**Violates:** tsr-brokers-017

**What happens:** Broker catches exception but doesn't rethrow.

Example:
```typescript
// ❌ WRONG
async getPatientAsync(id: string): Promise<Patient | null> {
  try {
    const response = await fetch(`${this.baseUrl}/patients/${id}`);
    return await response.json();
  } catch (error) {
    console.error(error); // Swallowed
    return null; // Hides failure
  }
}
```

**Why it's wrong:** Hides failures from calling services. Makes debugging impossible. Violates fail-fast principle.

**Fix:**
```typescript
async getPatientAsync(id: string): Promise<Patient> {
  try {
    const response = await fetch(`${this.baseUrl}/patients/${id}`);
    return await response.json();
  } catch (error) {
    throw new PatientApiBrokerException(
      `Failed to fetch patient ${id}`,
      error as Error
    );
  }
}
```

---

## Mixed Dependencies

**Violates:** tsr-brokers-013

**What happens:** Single broker handles multiple external dependencies.

Example:
```typescript
// ❌ WRONG
export class DataBroker {
  async getPatientFromApiAsync(id: string): Promise<Patient> {
    // API dependency
  }

  async saveToLocalStorageAsync(key: string, value: string): Promise<void> {
    // Storage dependency
  }

  logInfoAsync(message: string): Promise<void> {
    // Logging dependency
  }
}
```

**Why it's wrong:** Violates single responsibility. Makes testing and mocking difficult.

**Fix:** Separate brokers per dependency type.
