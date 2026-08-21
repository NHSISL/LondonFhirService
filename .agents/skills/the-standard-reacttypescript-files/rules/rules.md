# The Standard React TypeScript — Files — Rules

## File Responsibility

**tsr-files-001** [ERROR] A file MUST contain one primary responsibility.

**tsr-files-002** [ERROR] File names MUST describe architectural role using standard patterns.

## File Extensions

**tsr-files-003** [ERROR] React component files MUST use `.tsx` extension.

**tsr-files-004** [ERROR] Non-rendering TypeScript files MUST use `.ts` extension.

## Naming Patterns

**tsr-files-005** [ERROR] Broker files MUST follow pattern `{domain}{Kind}Broker.ts` (e.g., `patientApiBroker.ts`).

**tsr-files-006** [ERROR] Broker interface files MUST follow pattern `i{domain}{Kind}Broker.ts` (e.g., `iPatientApiBroker.ts`).

**tsr-files-007** [ERROR] Foundation service files MUST follow pattern `{domain}Service.ts` (e.g., `patientService.ts`).

**tsr-files-008** [ERROR] Service interface files MUST follow pattern `i{domain}Service.ts` (e.g., `iPatientService.ts`).

**tsr-files-009** [ERROR] View service files MUST follow pattern `{domain}ViewService.ts` (e.g., `dashboardViewService.ts`).

**tsr-files-010** [ERROR] Page files MUST follow pattern `{Domain}Page.tsx` (e.g., `DashboardPage.tsx`).

**tsr-files-011** [ERROR] Component files MUST follow pattern `{Domain}{Purpose}.tsx` (e.g., `PatientCard.tsx`).

**tsr-files-012** [ERROR] Hook files MUST follow pattern `use{Purpose}.ts` (e.g., `useDashboardPage.ts`).

**tsr-files-013** [ERROR] Model files MUST follow pattern `{Domain}.ts` or `{Domain}{Purpose}.ts` (e.g., `Patient.ts`, `PatientCardProps.ts`).

## Prohibited Patterns

**tsr-files-014** [ERROR] Do NOT create generic `utils.ts`, `helpers.ts`, or `common.ts` files.

**tsr-files-015** [ERROR] Do NOT use vague names like `stuff.tsx`, `temp.ts`, `misc.ts`.

**tsr-files-016** [WARN] Avoid deeply nested directory structures beyond 4 levels unless architecturally justified.
