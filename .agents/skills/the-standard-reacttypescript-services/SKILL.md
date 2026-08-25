---
name: the-standard-reacttypescript-services
version: 0.1.0
standard-version: v2.50.0
applies-to: ["src/services/foundations/**/*"]
depends-on: ["the-standard-reacttypescript-typescript", "the-standard-reacttypescript-models", "the-standard-reacttypescript-brokers"]
---

# The Standard React TypeScript — Foundation Services

## 0/ Context (Purpose — WHY this skill exists)

0.0/ Where: React + TypeScript + Vite projects following The Standard foundation service layer
0.1/ Who: Frontend engineers implementing business logic and orchestrating brokers
0.2/ What: Governs foundation service implementations that wrap brokers with validation, exception mapping, and business rules
0.3/ Applies to: src/services/foundations/**/* — all foundation service files
0.4/ Version: The Standard React TypeScript v0.1.0
0.5/ Depends on: the-standard-reacttypescript-typescript, the-standard-reacttypescript-models, the-standard-reacttypescript-brokers

## 1/ Actual (Dependency — WHAT the rules are and what they depend on)

1.0/ Dos:

1. Foundation services MUST wrap exactly one broker → see rules/rules.md#tsr-services-001
2. Foundation services MUST validate all inputs → see rules/rules.md#tsr-services-002
3. Foundation services MUST map broker exceptions to service exceptions → see rules/rules.md#tsr-services-003
4. Service methods MUST use try-catch pattern → see rules/rules.md#tsr-services-004
5. Foundation services MUST use dependency injection for brokers → see rules/rules.md#tsr-services-005
6. Service method names MUST match broker method names with added validation → see rules/rules.md#tsr-services-006
7. Services MUST implement TryCatch wrapper for exception mapping → see rules/rules.md#tsr-services-007

1.1/ Don'ts:

1. Never call multiple brokers from one foundation service → see validations/anti-patterns.md#multiple-brokers
2. Never skip input validation → see validations/anti-patterns.md#no-validation
3. Never let broker exceptions escape unwrapped → see validations/anti-patterns.md#unwrapped-exceptions
4. Never put UI logic in foundation services → see validations/anti-patterns.md#ui-logic-in-service

1.2/ Ask:

- When uncertain whether validation belongs in service or component
- When deciding between foundation service and processing service
- When broker exception mapping is unclear

1.3/ Defaults:

- Foundation services in `services/foundations/{domain}/`
- One service per broker
- All methods async
- TryCatch pattern for all broker calls
- Validation exceptions for invalid inputs
- Dependency exceptions for broker failures

1.4/ Examples:

**✅ Foundation Service:**
```typescript
export class PatientService {
  constructor(private patientApiBroker: IPatientApiBroker) {}

  async addPatientAsync(patient: Patient): Promise<Patient> {
    this.validatePatient(patient);

    return await this.tryCatch(async () => {
      return await this.patientApiBroker.postPatientAsync(patient);
    });
  }

  private validatePatient(patient: Patient): void {
    if (!patient) {
      throw new PatientValidationException('Patient is required');
    }
    if (!patient.firstName) {
      throw new PatientValidationException('First name is required');
    }
  }
}
```

For complete examples, see:
- ✅ examples/good/example_good_foundation_service.ts
- ✅ examples/good/example_good_trycatch_pattern.ts
- ❌ examples/bad/example_bad_no_validation.ts
- ❌ examples/bad/example_bad_multiple_brokers.ts

1.5/ Templates:

- Foundation service template: see templates/

1.6/ Checklists:

- Pre-review checklist: see validations/checklist.md

1.7/ Contracts:

- Service method contracts: see contracts/contracts.json

## 2/ Expected (Exposure — WHAT comes out)

2.0/ Format: TypeScript classes implementing business logic over brokers
2.1/ Outcome: Validated, exception-mapped domain operations. One service per broker. All inputs validated. All broker exceptions wrapped.
2.2/ Tone: Direct. Cite rule IDs. Block missing validation. Enforce TryCatch pattern. Require exception mapping.
