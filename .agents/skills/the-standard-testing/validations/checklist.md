# The Standard — Testing — Checklist

- [ ] Failing test was committed before implementation (ts-testing-001)
- [ ] Failing test commit message follows `{TestName} -> FAIL` (ts-testing-005)
- [ ] Passing implementation commit message follows `{TestName} -> PASS` (ts-testing-006)
- [ ] Test method name follows `Should{Outcome}_{Condition}Async` pattern (ts-testing-002)
- [ ] Test class name follows `{SubjectUnderTest}Tests` pattern (ts-testing-007)
- [ ] Each test asserts one specific behavior (ts-testing-003)
- [ ] All dependencies are mocked — no real database or HTTP calls (ts-testing-004)
- [ ] Happy path test exists (ts-testing-008)
- [ ] Validation failure test exists (ts-testing-008)
- [ ] Dependency failure test exists (ts-testing-008)
- [ ] Service exception test exists (ts-testing-008)
