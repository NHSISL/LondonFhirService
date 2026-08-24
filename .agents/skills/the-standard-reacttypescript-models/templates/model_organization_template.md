---
skill: the-standard-reacttypescript-models
type: template
source-section: "3. Models"
---

# Model Organization Template

Use this structure to organize models in a Standard-compliant React + TypeScript project.

## Directory Structure

```
src/models/
├── foundations/           # Domain/API models
│   ├── patients/
│   │   ├── Patient.ts
│   │   └── PatientStatus.ts
│   ├── appointments/
│   │   ├── Appointment.ts
│   │   └── AppointmentType.ts
│   └── users/
│       └── User.ts
│
├── views/                 # UI-ready transformed models
│   ├── patients/
│   │   ├── PatientCardView.ts
│   │   ├── PatientProfileView.ts
│   │   └── PatientListView.ts
│   ├── appointments/
│   │   ├── AppointmentCardView.ts
│   │   └── CalendarEventView.ts
│   └── users/
│       └── UserProfileView.ts
│
├── components/            # Component prop definitions
│   ├── patients/
│   │   ├── PatientCardProps.ts
│   │   ├── PatientFormProps.ts
│   │   └── PatientListProps.ts
│   ├── appointments/
│   │   ├── AppointmentCardProps.ts
│   │   └── CalendarProps.ts
│   └── common/
│       ├── ButtonProps.ts
│       └── ModalProps.ts
│
├── configurations/        # App configuration models
│   ├── ApiConfig.ts
│   ├── FeatureFlags.ts
│   └── ThemeConfig.ts
│
└── errors/               # Error models
    ├── ApiError.ts
    ├── ValidationError.ts
    └── ErrorResponse.ts
```

## Naming Conventions

### Foundation Models
- Pattern: `{Domain}.ts`
- Examples: `Patient.ts`, `Appointment.ts`, `User.ts`
- Content: Domain/API data shapes

### View Models
- Pattern: `{Domain}{Purpose}View.ts`
- Examples: `PatientCardView.ts`, `PatientProfileView.ts`, `PatientListView.ts`
- Content: UI-ready transformed data

### Component Props
- Pattern: `{Component}Props.ts`
- Examples: `PatientCardProps.ts`, `PatientFormProps.ts`, `ModalProps.ts`
- Content: Component rendering input

### Configuration Models
- Pattern: `{Purpose}Config.ts` or `{Purpose}Settings.ts`
- Examples: `ApiConfig.ts`, `FeatureFlags.ts`, `ThemeConfig.ts`
- Content: Application configuration shapes

### Error Models
- Pattern: `{Type}Error.ts` or `ErrorResponse.ts`
- Examples: `ApiError.ts`, `ValidationError.ts`
- Content: Error structures

## Model Rules

1. **Foundation models**: Domain concepts, match API when possible
2. **View models**: UI-ready, created by view services
3. **Component props**: Rendering input only
4. **No behavior**: Models are data-only (no methods)
5. **Use `type`**: Prefer `type` keyword over `interface`
6. **Explicit types**: No `any`, explicit optionality
7. **Separation**: Never mix concerns across layers

## Example Model File

```typescript
// src/models/foundations/patients/Patient.ts
export type Patient = {
  id: string;
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  email: string;
  status: PatientStatus;
  insuranceId?: string;
};

export type PatientStatus = 'active' | 'inactive' | 'archived';
```

```typescript
// src/models/views/patients/PatientCardView.ts
export type PatientCardView = {
  id: string;
  displayName: string;
  age: number;
  statusClassName: string;
  statusDisplayText: string;
  contactInfo: string;
};
```

```typescript
// src/models/components/patients/PatientCardProps.ts
import type { PatientCardView } from '../../views/patients/PatientCardView';

export type PatientCardProps = {
  patient: PatientCardView;
  onSelect?: (patientId: string) => void;
  isSelected?: boolean;
  className?: string;
};
```

## Cross-Layer Flow

```
API Response (foundation)
    ↓
Foundation Model (models/foundations/)
    ↓
Service Layer (processes foundation model)
    ↓
View Service (transforms to view model)
    ↓
View Model (models/views/)
    ↓
Component Props (models/components/)
    ↓
Component (renders view)
```

Never skip layers or mix concerns.
