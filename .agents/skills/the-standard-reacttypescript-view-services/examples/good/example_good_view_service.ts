---
skill: the-standard-reacttypescript-view-services
type: example
source-section: "View Services"
demonstrates: "tsr-view-001, tsr-view-002, tsr-view-004, tsr-view-006"
---

// ✅ CORRECT: View service transforming foundation models to view models

import type { Patient } from '../../../models/foundations/patients/Patient';
import type { PatientCardView } from '../../../models/views/patients/PatientCardView';
import type { IPatientService } from '../../foundations/patients/IPatientService';

export class PatientViewService {
  constructor(private patientService: IPatientService) {}

  async getPatientCardViewAsync(id: string): Promise<PatientCardView> {
    const patient = await this.patientService.retrievePatientByIdAsync(id);

    return {
      id: patient.id,
      displayName: `${patient.firstName} ${patient.lastName}`,
      age: this.calculateAge(patient.dateOfBirth),
      statusClassName: this.mapStatusToClassName(patient.status),
      statusDisplayText: this.mapStatusToDisplayText(patient.status),
      contactInfo: this.formatContactInfo(patient.email, patient.phoneNumber)
    };
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

  private mapStatusToClassName(status: string): string {
    const statusMap: Record<string, string> = {
      'active': 'text-green-600 bg-green-50',
      'inactive': 'text-gray-600 bg-gray-50',
      'archived': 'text-red-600 bg-red-50'
    };

    return statusMap[status] || 'text-gray-600 bg-gray-50';
  }

  private mapStatusToDisplayText(status: string): string {
    const statusMap: Record<string, string> = {
      'active': 'Active',
      'inactive': 'Inactive',
      'archived': 'Archived'
    };

    return statusMap[status] || 'Unknown';
  }

  private formatContactInfo(email: string, phone?: string): string {
    return phone ? `${email} • ${phone}` : email;
  }
}
