---
name: the-standard-reacttypescript-brokers
version: 0.1.0
standard-version: v2.50.0
applies-to: ["src/brokers/**/*"]
depends-on: ["the-standard-reacttypescript-typescript", "the-standard-reacttypescript-models"]
---

# The Standard React TypeScript — Brokers

## 0/ Context (Purpose — WHY this skill exists)

0.0/ Where: React + TypeScript + Vite projects following The Standard broker layer pattern
0.1/ Who: Frontend engineers implementing external dependency abstraction
0.2/ What: Governs broker implementations for APIs, storage, logging, and external services
0.3/ Applies to: src/brokers/**/* — all broker implementation files
0.4/ Version: The Standard React TypeScript v0.1.0
0.5/ Depends on: the-standard-reacttypescript-typescript, the-standard-reacttypescript-models

## 1/ Actual (Dependency — WHAT the rules are and what they depend on)

1.0/ Dos:

1. Brokers MUST be the only layer that talks to external dependencies → see rules/rules.md#tsr-brokers-001
2. Broker methods MUST use async/await for all I/O operations → see rules/rules.md#tsr-brokers-002
3. Brokers MUST map external exceptions to broker-layer exceptions → see rules/rules.md#tsr-brokers-003
4. API brokers MUST use standard HTTP verbs → see rules/rules.md#tsr-brokers-004
5. Storage brokers MUST abstract localStorage/sessionStorage → see rules/rules.md#tsr-brokers-006
6. Brokers MUST NOT contain business logic → see rules/rules.md#tsr-brokers-008
7. Brokers MUST be injected via dependency injection → see rules/rules.md#tsr-brokers-009

1.1/ Don'ts:

1. Never put business logic in brokers → see validations/anti-patterns.md#business-logic-in-broker
2. Never call another broker from within a broker → see validations/anti-patterns.md#broker-calling-broker
3. Never catch and swallow exceptions without rethrowing → see validations/anti-patterns.md#swallowed-exceptions
4. Never mix multiple external dependencies in one broker → see validations/anti-patterns.md#mixed-dependencies

1.2/ Ask:

- When uncertain whether logic belongs in broker or service
- When considering creating a new broker type
- When external dependency requires complex configuration

1.3/ Defaults:

- API brokers in `brokers/apis/`
- Storage brokers in `brokers/storages/`
- Logging brokers in `brokers/loggings/`
- DateTime brokers in `brokers/datetimes/`
- All methods async for I/O
- Throw exceptions, don't return error codes

1.4/ Examples:

**✅ API Broker:**
```typescript
export class PatientApiBroker {
  private baseUrl = import.meta.env.VITE_API_BASE_URL;

  async getAllPatientsAsync(): Promise<Patient[]> {
    try {
      const response = await fetch(`${this.baseUrl}/patients`);
      return await response.json();
    } catch (error) {
      throw new PatientApiBrokerException(error);
    }
  }
}
```

**❌ Business logic in broker:**
```typescript
// WRONG
async getActivePatientsAsync(): Promise<Patient[]> {
  const patients = await this.getAllPatientsAsync();
  return patients.filter(p => p.status === 'active'); // Business logic
}
```

For complete examples, see:
- ✅ examples/good/example_good_api_broker.ts
- ✅ examples/good/example_good_storage_broker.ts
- ✅ examples/good/example_good_logging_broker.ts
- ❌ examples/bad/example_bad_business_logic.ts
- ❌ examples/bad/example_bad_broker_calling_broker.ts

1.5/ Templates:

- Broker templates: see templates/

1.6/ Checklists:

- Pre-review checklist: see validations/checklist.md

1.7/ Contracts:

- Broker method contracts: see contracts/contracts.json

## 2/ Expected (Exposure — WHAT comes out)

2.0/ Format: TypeScript classes implementing external dependency abstraction
2.1/ Outcome: Clean separation between application logic and external dependencies. All I/O isolated to brokers. No business logic in brokers.
2.2/ Tone: Direct. Cite rule IDs. Block business logic in brokers. Enforce single responsibility.
