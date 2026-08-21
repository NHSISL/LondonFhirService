# The Standard — User Interfaces — Checklist

- [ ] Component delegates data access to a view service — no direct service/broker calls (ts-ui-001)
- [ ] Component has a single rendering concern (ts-ui-002)
- [ ] View service maps exceptions to view-friendly errors (ts-ui-003)
- [ ] View service exposes view models, not domain models (ts-ui-003)
- [ ] Page uses a base class (*Base : ComponentBase) to separate code-behind (ts-ui-004)
- [ ] Loading state is handled (spinner, skeleton) (ts-ui-005)
- [ ] Error state is handled (user-friendly message) (ts-ui-005)
- [ ] Empty state is handled (empty list message, placeholder) (ts-ui-005)
- [ ] Component name describes what it renders (ts-ui-006)
