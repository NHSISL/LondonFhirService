# The Standard — Foundation Services — Checklist

- [ ] Service serves only one entity type (ts-foundations-001)
- [ ] All incoming models are structurally validated (null, empty strings, default Guid/DateTime) (ts-foundations-002)
- [ ] Each operation calls only one broker method (ts-foundations-003)
- [ ] No model transformation occurs before the broker call (ts-foundations-004)
- [ ] Broker exceptions are caught and wrapped in local exception types (ts-foundations-005)
- [ ] All broker calls are wrapped in try-catch (ts-foundations-006)
- [ ] Service is a partial class split by operation category (ts-foundations-007)
- [ ] Interface follows I{Entity}Service naming pattern (ts-foundations-008)
- [ ] Service does not call another service (ts-foundations-003)
- [ ] No business logic combining multiple entities exists in this service (ts-foundations-001)
