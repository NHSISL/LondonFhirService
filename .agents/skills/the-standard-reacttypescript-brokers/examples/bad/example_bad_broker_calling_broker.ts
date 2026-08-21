---
skill: the-standard-reacttypescript-brokers
type: example
source-section: "Brokers"
demonstrates: "tsr-brokers-019 violation"
---

// ❌ WRONG: Broker calling another broker

export class PatientApiBroker {
  // ❌ WRONG: Injecting another broker
  constructor(
    private loggingBroker: LoggingBroker,
    private dateTimeBroker: DateTimeBroker
  ) {}

  async getPatientByIdAsync(id: string): Promise<Patient> {
    // ❌ WRONG: Calling logging broker from API broker
    this.loggingBroker.logInfo(`Fetching patient ${id}`);

    try {
      const response = await fetch(`${this.baseUrl}/api/patients/${id}`);
      const patient = await response.json();

      // ❌ WRONG: Calling datetime broker from API broker
      const fetchedAt = this.dateTimeBroker.getCurrentIsoString();

      // ❌ WRONG: Adding fetched timestamp (business logic)
      return {
        ...patient,
        fetchedAt
      };
    } catch (error) {
      this.loggingBroker.logError(error as Error); // ❌ WRONG
      throw error;
    }
  }

  async getAllPatientsAsync(): Promise<Patient[]> {
    this.loggingBroker.logInfo('Fetching all patients'); // ❌ WRONG

    const response = await fetch(`${this.baseUrl}/api/patients`);
    return await response.json();
  }

  private baseUrl = import.meta.env.VITE_API_BASE_URL;
}

// Why this is wrong:
// 1. Creates coupling between brokers
// 2. Makes mocking harder in tests
// 3. Violates single responsibility
// 4. Composition should happen in services, not brokers
// 5. Can't test brokers in isolation

// Fix: Compose brokers in service layer
export class PatientApiBrokerCorrect {
  private baseUrl = import.meta.env.VITE_API_BASE_URL;

  async getPatientByIdAsync(id: string): Promise<Patient> {
    const response = await fetch(`${this.baseUrl}/api/patients/${id}`);
    return await response.json();
  }

  async getAllPatientsAsync(): Promise<Patient[]> {
    const response = await fetch(`${this.baseUrl}/api/patients`);
    return await response.json();
  }
}

// ✅ CORRECT: Service composes brokers
export class PatientService {
  constructor(
    private patientApiBroker: PatientApiBrokerCorrect,
    private loggingBroker: LoggingBroker,
    private dateTimeBroker: DateTimeBroker
  ) {}

  async getPatientByIdAsync(id: string): Promise<Patient> {
    this.loggingBroker.logInfo(`Fetching patient ${id}`);

    try {
      const patient = await this.patientApiBroker.getPatientByIdAsync(id);
      const fetchedAt = this.dateTimeBroker.getCurrentIsoString();

      return {
        ...patient,
        fetchedAt
      };
    } catch (error) {
      this.loggingBroker.logError(error as Error);
      throw new PatientServiceException(`Failed to get patient ${id}`, error as Error);
    }
  }

  async getAllPatientsAsync(): Promise<Patient[]> {
    this.loggingBroker.logInfo('Fetching all patients');
    return await this.patientApiBroker.getAllPatientsAsync();
  }
}

type Patient = {
  id: string;
  firstName: string;
  lastName: string;
  fetchedAt?: string;
};

class LoggingBroker {
  logInfo(message: string): void {}
  logError(error: Error): void {}
}

class DateTimeBroker {
  getCurrentIsoString(): string {
    return new Date().toISOString();
  }
}

class PatientServiceException extends Error {
  constructor(message: string, public innerException: Error) {
    super(message);
  }
}
