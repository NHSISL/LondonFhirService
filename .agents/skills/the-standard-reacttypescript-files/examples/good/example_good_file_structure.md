---
skill: the-standard-reacttypescript-files
type: example
source-section: "1. Project Structure"
demonstrates: "tsr-files-001, tsr-files-002, tsr-files-005 through tsr-files-013"
---

# ✅ Good File Structure

This example demonstrates proper file organization and naming for a React TypeScript project following The Standard.

```
src/
  app/
    App.tsx
    main.tsx
    routes.tsx

  brokers/
    apis/
      patientApiBroker.ts
      iPatientApiBroker.ts
      appointmentApiBroker.ts
      iAppointmentApiBroker.ts
    dateTimes/
      dateTimeBroker.ts
      iDateTimeBroker.ts
    loggings/
      loggingBroker.ts
      iLoggingBroker.ts
    navigations/
      navigationBroker.ts
      iNavigationBroker.ts
    storages/
      localStorageBroker.ts
      iLocalStorageBroker.ts

  components/
    shared/
      LoadingIndicator.tsx
      ErrorSummary.tsx
      EmptyState.tsx
    patients/
      PatientCard.tsx
      PatientList.tsx
      PatientForm.tsx
    appointments/
      AppointmentCard.tsx
      AppointmentCalendar.tsx

  models/
    foundations/
      patients/
        Patient.ts
        PatientError.ts
      appointments/
        Appointment.ts
    views/
      dashboard/
        DashboardView.ts
      patients/
        PatientProfileView.ts
    components/
      patients/
        PatientCardProps.ts
      appointments/
        AppointmentCardProps.ts

  pages/
    home/
      HomePage.tsx
      useHomePage.ts
    dashboard/
      DashboardPage.tsx
      useDashboardPage.ts
    patients/
      PatientProfilePage.tsx
      usePatientProfilePage.ts

  services/
    foundations/
      patients/
        patientService.ts
        patientService.validations.ts
        patientService.exceptions.ts
        iPatientService.ts
      appointments/
        appointmentService.ts
        iAppointmentService.ts
    views/
      dashboard/
        dashboardViewService.ts
        iDashboardViewService.ts
      patients/
        patientProfileViewService.ts
        iPatientProfileViewService.ts

  hooks/
    pages/
      useDashboardPage.ts
      usePatientProfilePage.ts
    components/
      useModal.ts
      usePagination.ts

  styles/
    bootstrap/
      bootstrap.scss
    global/
      global.css
```

**Why this is correct:**

1. **Clear architectural layers** — brokers, services, components, pages are separate
2. **Consistent naming** — each file follows its architectural pattern
3. **Single responsibility** — each file has one clear purpose
4. **Descriptive names** — file names reveal their role and domain
5. **Proper extensions** — `.tsx` for components, `.ts` for logic
6. **No generic files** — no `utils.ts`, `helpers.ts`, or `common.ts`
