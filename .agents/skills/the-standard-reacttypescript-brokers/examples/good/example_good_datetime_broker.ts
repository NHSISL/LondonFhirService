---
skill: the-standard-reacttypescript-brokers
type: example
source-section: "Brokers"
demonstrates: "tsr-brokers-010, tsr-brokers-013"
---

// ✅ CORRECT: DateTime Broker implementation
// File: src/brokers/datetimes/DateTimeBroker.ts

export interface IDateTimeBroker {
  getCurrentDateTime(): Date;
  getCurrentDateTimeUtc(): Date;
  getCurrentTimestamp(): number;
  getCurrentIsoString(): string;
}

export class DateTimeBroker implements IDateTimeBroker {
  getCurrentDateTime(): Date {
    return new Date();
  }

  getCurrentDateTimeUtc(): Date {
    const now = new Date();
    return new Date(now.toISOString());
  }

  getCurrentTimestamp(): number {
    return Date.now();
  }

  getCurrentIsoString(): string {
    return new Date().toISOString();
  }
}

// Why this is correct:
// 1. Abstracts Date creation (tsr-brokers-010)
// 2. Makes services testable by allowing mock time injection
// 3. Single responsibility - date/time only (tsr-brokers-013)
// 4. Interface for testing (tsr-brokers-014)
// 5. No business logic (tsr-brokers-008)
// 6. Synchronous methods acceptable (no I/O)

// Usage in service:
/*
export class PatientService {
  constructor(
    private patientApiBroker: IPatientApiBroker,
    private dateTimeBroker: IDateTimeBroker
  ) {}

  async createPatientAsync(patient: Patient): Promise<Patient> {
    const now = this.dateTimeBroker.getCurrentIsoString();
    const patientWithTimestamp = {
      ...patient,
      createdAt: now,
      updatedAt: now
    };

    return await this.patientApiBroker.postPatientAsync(patientWithTimestamp);
  }
}
*/
