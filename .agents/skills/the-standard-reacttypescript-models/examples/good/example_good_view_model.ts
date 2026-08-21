---
skill: the-standard-reacttypescript-models
type: example
source-section: "3. Models"
demonstrates: "tsr-models-002, tsr-models-010"
---

// ✅ CORRECT: View model representing UI-ready data
// File: src/models/views/patients/PatientCardView.ts

export type PatientCardView = {
  id: string;
  displayName: string; // Transformed by view service
  age: number; // Calculated by view service
  statusClassName: string; // Mapped by view service
  statusDisplayText: string; // Mapped by view service
  contactInfo: string; // Formatted by view service
};

export type PatientProfileView = {
  id: string;
  fullName: string;
  age: number;
  email: string;
  phone: string;
  statusBadgeVariant: 'success' | 'warning' | 'danger';
  statusText: string;
  insuranceDisplay: string;
  formattedDateOfBirth: string;
};

export type PatientListView = {
  patients: PatientCardView[];
  totalCount: number;
  currentPage: number;
  pageSize: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
};

// Why this is correct:
// 1. UI-ready data (displayName, not firstName + lastName)
// 2. Calculated fields (age from dateOfBirth)
// 3. Presentation-specific fields (className, badgeVariant)
// 4. Created by view service transformation, not in components
// 5. No domain logic embedded
// 6. Specific to UI needs
