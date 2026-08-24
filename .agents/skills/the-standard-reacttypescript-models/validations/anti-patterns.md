# The Standard React TypeScript — Models — Anti-Patterns

## Direct API Props

**Violates:** tsr-models-004

**What happens:** API response models are passed directly to components without transformation.

Example:
```typescript
// API response
type PatientApiResponse = {
  id: string;
  first_name: string; // snake_case from API
  last_name: string;
  dob: string;
  status: number; // numeric code
};

// Component using API model directly
<PatientCard patient={apiResponse} />
```

**Why it's wrong:** API contracts and UI contracts serve different purposes. The UI needs display-ready data, not raw API responses. This creates tight coupling between API format and UI.

**Fix:** Transform in a view service:
```typescript
// Foundation model
type Patient = {
  id: string;
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  status: number;
};

// View model
type PatientCardView = {
  id: string;
  displayName: string;
  age: number;
  statusClassName: string;
};

// View service transforms
function transformToPatientCardView(patient: Patient): PatientCardView {
  return {
    id: patient.id,
    displayName: `${patient.firstName} ${patient.lastName}`,
    age: calculateAge(patient.dateOfBirth),
    statusClassName: getStatusClassName(patient.status)
  };
}
```

---

## Models With Behavior

**Violates:** tsr-models-005

**What happens:** Models contain methods, functions, or API calls.

Example:
```typescript
// ❌ WRONG
export type Patient = {
  id: string;
  firstName: string;
  lastName: string;

  // Behavior in model
  getFullName(): string {
    return `${this.firstName} ${this.lastName}`;
  }

  async save(): Promise<void> {
    await fetch('/api/patients', {
      method: 'POST',
      body: JSON.stringify(this)
    });
  }
};
```

**Why it's wrong:** Models are data contracts, not objects with behavior. Behavior belongs in services. This violates separation of concerns and makes models untestable.

**Fix:** Move behavior to services:
```typescript
// Model - data only
export type Patient = {
  id: string;
  firstName: string;
  lastName: string;
};

// Service - behavior
export class PatientService {
  public getFullName(patient: Patient): string {
    return `${patient.firstName} ${patient.lastName}`;
  }

  public async savePatientAsync(patient: Patient): Promise<void> {
    await this.patientApiBroker.postPatientAsync(patient);
  }
}
```

---

## Mixed Model Concerns

**Violates:** tsr-models-001, tsr-models-002, tsr-models-006

**What happens:** Foundation and view model concerns are mixed in one type.

Example:
```typescript
// ❌ WRONG - mixing domain and UI concerns
export type Patient = {
  // Domain fields
  id: string;
  firstName: string;
  lastName: string;
  dateOfBirth: string;

  // UI fields mixed in
  displayName: string;
  statusClassName: string;
  isSelected: boolean;
};
```

**Why it's wrong:** This mixes architectural layers. The domain model now depends on UI concerns, making it impossible to use in non-UI contexts and creating maintenance confusion.

**Fix:** Separate into foundation and view models:
```typescript
// Foundation model - domain only
export type Patient = {
  id: string;
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  status: PatientStatus;
};

// View model - UI-ready
export type PatientCardView = {
  id: string;
  displayName: string;
  age: number;
  statusClassName: string;
  isSelected: boolean;
};
```

---

## Incorrect Optionality

**Violates:** tsr-models-011

**What happens:** Properties marked optional when they're always required at runtime.

Example:
```typescript
// ❌ WRONG
export type Patient = {
  id?: string; // Always required
  firstName?: string; // Always required
  lastName?: string; // Always required
  middleName?: string; // Correctly optional
};

// Leads to this:
function renderPatient(patient: Patient) {
  // Have to check fields that are never actually missing
  return patient.firstName ? patient.firstName : 'Unknown';
}
```

**Why it's wrong:** Incorrect optionality forces unnecessary null checks and hides true requirements. It makes the code harder to reason about.

**Fix:** Mark only truly optional fields:
```typescript
// ✅ CORRECT
export type Patient = {
  id: string; // Required
  firstName: string; // Required
  lastName: string; // Required
  middleName?: string; // Truly optional
  suffix?: string; // Truly optional
};
```

---

## Any in Models

**Violates:** tsr-models-012

**What happens:** Model properties use `any` type.

Example:
```typescript
export type Patient = {
  id: string;
  firstName: string;
  metadata: any; // What structure does this have?
  options: any;
};
```

**Why it's wrong:** `any` defeats type safety and provides no documentation about the expected structure.

**Fix:** Use explicit types:
```typescript
export type Patient = {
  id: string;
  firstName: string;
  metadata: {
    createdAt: string;
    createdBy: string;
    version: number;
  };
  options: PatientOptions;
};

export type PatientOptions = {
  notificationsEnabled: boolean;
  preferredLanguage: string;
};
```
