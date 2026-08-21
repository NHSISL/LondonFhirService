---
skill: the-standard-reacttypescript-files
type: template
source-section: "1. Project Structure"
---

# React TypeScript Project Structure Template

This template provides the recommended folder structure for a React + TypeScript + Vite project following The Standard.

```
src/
  app/
    App.tsx                 # Root application component
    main.tsx                # Application entry point
    routes.tsx              # Route definitions

  brokers/
    apis/                   # API communication brokers
      {domain}ApiBroker.ts
      i{domain}ApiBroker.ts
    dateTimes/              # Date/time abstraction
      dateTimeBroker.ts
      iDateTimeBroker.ts
    loggings/               # Logging abstraction
      loggingBroker.ts
      iLoggingBroker.ts
    navigations/            # Navigation/routing abstraction
      navigationBroker.ts
      iNavigationBroker.ts
    storages/               # Storage abstraction (localStorage, sessionStorage)
      localStorageBroker.ts
      iLocalStorageBroker.ts

  components/
    shared/                 # Reusable UI components
      LoadingIndicator.tsx
      ErrorSummary.tsx
      EmptyState.tsx
    layouts/                # Layout components
      MainLayout.tsx
      AuthLayout.tsx
    {domain}/               # Domain-specific components
      {Domain}Card.tsx
      {Domain}List.tsx
      {Domain}Form.tsx

  models/
    configurations/         # Configuration models
      AppConfig.ts
    foundations/            # Foundation domain models
      {domain}/
        {Domain}.ts
        {Domain}Error.ts
    views/                  # View models for UI
      {domain}/
        {Domain}View.ts
    components/             # Component prop types
      {domain}/
        {Domain}CardProps.ts

  pages/
    {domain}/               # Domain pages
      {Domain}Page.tsx
      use{Domain}Page.ts

  services/
    foundations/            # Foundation services (business logic)
      {domain}/
        {domain}Service.ts
        {domain}Service.validations.ts
        {domain}Service.exceptions.ts
        i{domain}Service.ts
    views/                  # View services (UI orchestration)
      {domain}/
        {domain}ViewService.ts
        i{domain}ViewService.ts

  hooks/
    pages/                  # Page-level hooks
      use{Domain}Page.ts
    components/             # Component-level hooks
      useModal.ts
      usePagination.ts

  styles/
    bootstrap/              # Bootstrap customization
      bootstrap.scss
    global/                 # Global styles
      global.css

  tests/
    fixtures/               # Test data
    utilities/              # Test utilities
```

## Naming Conventions

| Architectural Role | File Pattern | Example |
|---|---|---|
| API Broker | `{domain}ApiBroker.ts` | `patientApiBroker.ts` |
| Broker Interface | `i{domain}{Kind}Broker.ts` | `iPatientApiBroker.ts` |
| Foundation Service | `{domain}Service.ts` | `patientService.ts` |
| Service Interface | `i{domain}Service.ts` | `iPatientService.ts` |
| View Service | `{domain}ViewService.ts` | `dashboardViewService.ts` |
| Component | `{Domain}{Purpose}.tsx` | `PatientCard.tsx` |
| Page | `{Domain}Page.tsx` | `DashboardPage.tsx` |
| Hook | `use{Purpose}.ts` | `useDashboardPage.ts` |
| Model | `{Domain}.ts` | `Patient.ts` |

## Rules

1. Each file contains one primary responsibility
2. File names describe architectural role
3. No generic `utils`, `helpers`, or `common` files
4. Components use `.tsx`, non-rendering files use `.ts`
5. Maximum nesting depth: 4 levels
