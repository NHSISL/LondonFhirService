---
skill: the-standard-reacttypescript-view-services
type: example
source-section: "View Services"
demonstrates: "tsr-view-006 violation"
---

// ❌ WRONG: Returning foundation model directly to UI

export class PatientViewService {
  async getPatientAsync(id: string): Promise<Patient> {
    // ❌ Foundation model returned directly
    return await this.patientService.retrievePatientByIdAsync(id);
  }
}

// ✅ CORRECT: Transform to view model
export class PatientViewServiceCorrect {
  async getPatientCardViewAsync(id: string): Promise<PatientCardView> {
    const patient = await this.patientService.retrievePatientByIdAsync(id);

    return {
      id: patient.id,
      displayName: `${patient.firstName} ${patient.lastName}`,
      age: this.calculateAge(patient.dateOfBirth)
    };
  }
}
