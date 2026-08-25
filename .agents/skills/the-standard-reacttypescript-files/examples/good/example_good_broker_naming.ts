---
skill: the-standard-reacttypescript-files
type: example
source-section: "1. Project Structure"
demonstrates: "tsr-files-005, tsr-files-006"
---

// ✅ CORRECT: Broker naming pattern
// File: src/brokers/apis/patientApiBroker.ts

export interface IPatientApiBroker {
  getPatientAsync(patientId: string): Promise<Patient>;
  postPatientAsync(patient: Patient): Promise<Patient>;
}

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

    return await response.json() as Patient;
  }

  public async postPatientAsync(patient: Patient): Promise<Patient> {
    const response = await fetch(`${this.baseUrl}/patients`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(patient)
    });

    if (!response.ok) {
      throw new Error('Failed to create patient.');
    }

    return await response.json() as Patient;
  }
}

export type Patient = {
  id: string;
  firstName: string;
  lastName: string;
  dateOfBirth: string;
};
