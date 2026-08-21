---
skill: the-standard-reacttypescript-brokers
type: example
source-section: "Brokers"
demonstrates: "tsr-brokers-001, tsr-brokers-002, tsr-brokers-003, tsr-brokers-004, tsr-brokers-005"
---

// ✅ CORRECT: API Broker implementation
// File: src/brokers/apis/PatientApiBroker.ts

import type { Patient } from '../../models/foundations/patients/Patient';

export interface IPatientApiBroker {
  getAllPatientsAsync(signal?: AbortSignal): Promise<Patient[]>;
  getPatientByIdAsync(id: string, signal?: AbortSignal): Promise<Patient>;
  postPatientAsync(patient: Patient, signal?: AbortSignal): Promise<Patient>;
  putPatientAsync(patient: Patient, signal?: AbortSignal): Promise<Patient>;
  deletePatientByIdAsync(id: string, signal?: AbortSignal): Promise<void>;
}

export class PatientApiBroker implements IPatientApiBroker {
  private baseUrl: string;

  constructor() {
    this.baseUrl = import.meta.env.VITE_API_BASE_URL;
  }

  async getAllPatientsAsync(signal?: AbortSignal): Promise<Patient[]> {
    try {
      const response = await fetch(`${this.baseUrl}/api/patients`, { signal });

      if (!response.ok) {
        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
      }

      return await response.json();
    } catch (error) {
      throw new PatientApiBrokerException(
        'Failed to retrieve patients',
        error as Error
      );
    }
  }

  async getPatientByIdAsync(id: string, signal?: AbortSignal): Promise<Patient> {
    try {
      const response = await fetch(`${this.baseUrl}/api/patients/${id}`, { signal });

      if (!response.ok) {
        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
      }

      return await response.json();
    } catch (error) {
      throw new PatientApiBrokerException(
        `Failed to retrieve patient ${id}`,
        error as Error
      );
    }
  }

  async postPatientAsync(patient: Patient, signal?: AbortSignal): Promise<Patient> {
    try {
      const response = await fetch(`${this.baseUrl}/api/patients`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(patient),
        signal
      });

      if (!response.ok) {
        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
      }

      return await response.json();
    } catch (error) {
      throw new PatientApiBrokerException(
        'Failed to create patient',
        error as Error
      );
    }
  }

  async putPatientAsync(patient: Patient, signal?: AbortSignal): Promise<Patient> {
    try {
      const response = await fetch(`${this.baseUrl}/api/patients/${patient.id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(patient),
        signal
      });

      if (!response.ok) {
        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
      }

      return await response.json();
    } catch (error) {
      throw new PatientApiBrokerException(
        `Failed to update patient ${patient.id}`,
        error as Error
      );
    }
  }

  async deletePatientByIdAsync(id: string, signal?: AbortSignal): Promise<void> {
    try {
      const response = await fetch(`${this.baseUrl}/api/patients/${id}`, {
        method: 'DELETE',
        signal
      });

      if (!response.ok) {
        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
      }
    } catch (error) {
      throw new PatientApiBrokerException(
        `Failed to delete patient ${id}`,
        error as Error
      );
    }
  }
}

export class PatientApiBrokerException extends Error {
  constructor(message: string, public innerException: Error) {
    super(message);
    this.name = 'PatientApiBrokerException';
  }
}

// Why this is correct:
// 1. Interface for testing/mocking (tsr-brokers-014)
// 2. All methods async (tsr-brokers-002)
// 3. Standard HTTP verbs (tsr-brokers-004)
// 4. Method naming pattern {verb}{Entity}Async (tsr-brokers-005)
// 5. Exceptions wrapped with inner exception (tsr-brokers-003, tsr-brokers-016)
// 6. AbortSignal for cancellation (tsr-brokers-021)
// 7. No business logic (tsr-brokers-008)
// 8. Single responsibility - API only (tsr-brokers-013)
