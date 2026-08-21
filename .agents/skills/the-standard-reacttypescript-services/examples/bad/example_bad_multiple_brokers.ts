---
skill: the-standard-reacttypescript-services
type: example
source-section: "Foundation Services"
demonstrates: "tsr-services-001 violation"
---

// ❌ WRONG: Foundation service using multiple brokers

export class PatientService {
  // ❌ WRONG: Multiple brokers injected
  constructor(
    private patientApiBroker: IPatientApiBroker,
    private loggingBroker: ILoggingBroker,
    private storageBroker: IStorageBroker,
    private dateTimeBroker: IDateTimeBroker
  ) {}

  async addPatientAsync(patient: Patient): Promise<Patient> {
    // ❌ Using multiple brokers in foundation service
    this.loggingBroker.logInfo(`Adding patient: ${patient.firstName}`);

    const now = this.dateTimeBroker.getCurrentIsoString();
    const patientWithTimestamp = { ...patient, createdAt: now };

    const result = await this.patientApiBroker.postPatientAsync(patientWithTimestamp);

    await this.storageBroker.setItemAsync('lastPatient', result);
    this.loggingBroker.logInfo(`Patient added: ${result.id}`);

    return result;
  }

  async retrievePatientByIdAsync(id: string): Promise<Patient> {
    this.loggingBroker.logInfo(`Retrieving patient: ${id}`);

    const patient = await this.patientApiBroker.getPatientByIdAsync(id);

    // ❌ Orchestrating multiple brokers
    await this.storageBroker.setItemAsync(`patient_${id}`, patient);

    return patient;
  }
}

// Why this is wrong:
// 1. Foundation service wraps multiple brokers (violates tsr-services-001)
// 2. Orchestration belongs in processing/orchestration layer
// 3. Makes service harder to test
// 4. Violates single responsibility
// 5. Mixes concerns (domain logic + logging + storage)

// Fix: Separate into layers
// ✅ Foundation Service - wraps ONE broker
export class PatientServiceCorrect {
  constructor(private patientApiBroker: IPatientApiBroker) {}

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
  }

  private validatePatientId(id: string): void {
    if (!id) {
      throw new PatientValidationException('Patient ID is required');
    }
  }

  private async tryCatch<T>(fn: () => Promise<T>): Promise<T> {
    try {
      return await fn();
    } catch (error) {
      if (error instanceof PatientValidationException) throw error;
      throw new PatientDependencyException('Dependency error', error as Error);
    }
  }
}

// ✅ Foundation Service for Logging
export class LoggingService {
  constructor(private loggingBroker: ILoggingBroker) {}

  logInfo(message: string): void {
    this.loggingBroker.logInfo(message);
  }
}

// ✅ Foundation Service for Storage
export class StorageService {
  constructor(private storageBroker: IStorageBroker) {}

  async saveItemAsync<T>(key: string, value: T): Promise<void> {
    await this.storageBroker.setItemAsync(key, value);
  }
}

// ✅ Foundation Service for DateTime
export class DateTimeService {
  constructor(private dateTimeBroker: IDateTimeBroker) {}

  getCurrentIsoString(): string {
    return this.dateTimeBroker.getCurrentIsoString();
  }
}

// ✅ Processing Service - orchestrates multiple foundation services
export class PatientProcessingService {
  constructor(
    private patientService: IPatientService,
    private loggingService: ILoggingService,
    private storageService: IStorageService,
    private dateTimeService: IDateTimeService
  ) {}

  async processPatientCreationAsync(patient: Patient): Promise<Patient> {
    this.loggingService.logInfo(`Adding patient: ${patient.firstName}`);

    const now = this.dateTimeService.getCurrentIsoString();
    const patientWithTimestamp = { ...patient, createdAt: now };

    const result = await this.patientService.addPatientAsync(patientWithTimestamp);

    await this.storageService.saveItemAsync('lastPatient', result);
    this.loggingService.logInfo(`Patient added: ${result.id}`);

    return result;
  }
}

type Patient = {
  id: string;
  firstName: string;
  lastName: string;
  createdAt?: string;
};

interface IPatientApiBroker {
  postPatientAsync(patient: Patient): Promise<Patient>;
  getPatientByIdAsync(id: string): Promise<Patient>;
}

interface ILoggingBroker {
  logInfo(message: string): void;
}

interface IStorageBroker {
  setItemAsync<T>(key: string, value: T): Promise<void>;
}

interface IDateTimeBroker {
  getCurrentIsoString(): string;
}

interface IPatientService {
  addPatientAsync(patient: Patient): Promise<Patient>;
}

interface ILoggingService {
  logInfo(message: string): void;
}

interface IStorageService {
  saveItemAsync<T>(key: string, value: T): Promise<void>;
}

interface IDateTimeService {
  getCurrentIsoString(): string;
}

class PatientValidationException extends Error {}
class PatientDependencyException extends Error {
  constructor(message: string, public innerException: Error) {
    super(message);
  }
}
