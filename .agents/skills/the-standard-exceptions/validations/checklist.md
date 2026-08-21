# The Standard — Exceptions — Checklist

- [ ] Service exposes all three outer categories: Validation, Dependency, Service (ts-exceptions-001)
- [ ] DependencyValidationException exists for conflict/not-found scenarios (ts-exceptions-004)
- [ ] All input errors are wrapped in ValidationException (ts-exceptions-002)
- [ ] All infrastructure errors are wrapped in DependencyException (ts-exceptions-003)
- [ ] Unexpected errors are wrapped in ServiceException (ts-exceptions-005)
- [ ] Each layer defines its own exception types (ts-exceptions-006)
- [ ] Original exception is always passed as inner exception (ts-exceptions-007)
- [ ] Exception messages do not expose SQL, HTTP, or infrastructure details (ts-exceptions-008)
- [ ] No raw SqlException, HttpRequestException, or DbUpdateException leaks across layer boundaries (ts-exceptions-003)
- [ ] No exceptions are silently swallowed without logging or re-throwing
