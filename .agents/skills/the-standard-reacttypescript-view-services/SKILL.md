---
name: the-standard-reacttypescript-view-services
version: 0.1.0
standard-version: v2.50.0
applies-to: ["src/services/views/**/*"]
depends-on: ["the-standard-reacttypescript-typescript", "the-standard-reacttypescript-models", "the-standard-reacttypescript-services"]
---

# The Standard React TypeScript — View Services

## 0/ Context (Purpose — WHY this skill exists)

0.0/ Where: React + TypeScript + Vite projects following The Standard view service layer
0.1/ Who: Frontend engineers transforming domain models into UI-ready view models
0.2/ What: Governs view service implementations that transform foundation models to view models for UI consumption
0.3/ Applies to: src/services/views/**/* — all view service files
0.4/ Version: The Standard React TypeScript v0.1.0
0.5/ Depends on: the-standard-reacttypescript-typescript, the-standard-reacttypescript-models, the-standard-reacttypescript-services

## 1/ Actual (Dependency — WHAT the rules are and what they depend on)

1.0/ Dos:

1. View services MUST transform foundation models to view models → see rules/rules.md#tsr-view-001
2. View services MUST inject foundation services, not brokers → see rules/rules.md#tsr-view-002
3. View services MUST NOT contain business logic → see rules/rules.md#tsr-view-003
4. View services MUST handle UI-specific transformations → see rules/rules.md#tsr-view-004
5. View services MUST map foundation service exceptions to view-layer exceptions → see rules/rules.md#tsr-view-005
6. View service methods MUST return view models, not foundation models → see rules/rules.md#tsr-view-006

1.1/ Don'ts:

1. Never return foundation models directly to UI → see validations/anti-patterns.md#foundation-model-leak
2. Never put business validation in view services → see validations/anti-patterns.md#business-logic-in-view
3. Never call brokers from view services → see validations/anti-patterns.md#broker-in-view-service
4. Never mix view transformation with business rules → see validations/anti-patterns.md#mixed-concerns

1.2/ Ask:

- When uncertain whether logic is business or presentation
- When deciding between view service and component transformation
- When transformation complexity grows large

1.3/ Defaults:

- View services in `services/views/{domain}/`
- One view service per domain
- Transform foundation models → view models
- Format dates, combine fields, calculate display values
- Map status codes to UI-friendly labels
- Generate className values for styling

1.4/ Examples:

**✅ View Service:**
```typescript
export class PatientViewService {
  constructor(private patientService: IPatientService) {}

  async getPatientCardViewAsync(id: string): Promise<PatientCardView> {
    const patient = await this.patientService.retrievePatientByIdAsync(id);

    return {
      id: patient.id,
      displayName: `${patient.firstName} ${patient.lastName}`,
      age: this.calculateAge(patient.dateOfBirth),
      statusClassName: this.mapStatusToClassName(patient.status)
    };
  }

  private calculateAge(dateOfBirth: string): number {
    const today = new Date();
    const birthDate = new Date(dateOfBirth);
    return today.getFullYear() - birthDate.getFullYear();
  }
}
```

For complete examples, see:
- ✅ examples/good/example_good_view_service.ts
- ✅ examples/good/example_good_transformations.ts
- ❌ examples/bad/example_bad_foundation_model_leak.ts
- ❌ examples/bad/example_bad_business_logic.ts

1.5/ Templates:

- View service template: see templates/

1.6/ Checklists:

- Pre-review checklist: see validations/checklist.md

1.7/ Contracts:

- View service contracts: see contracts/contracts.json

## 2/ Expected (Exposure — WHAT comes out)

2.0/ Format: TypeScript classes transforming foundation models to view models
2.1/ Outcome: UI-ready data. Foundation models never exposed to UI. All display logic centralized. Components receive pre-formatted data.
2.2/ Tone: Direct. Cite rule IDs. Block foundation model leaks. Enforce view model returns. Reject business logic in view layer.
