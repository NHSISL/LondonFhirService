# The Standard — Processing Services — Checklist

- [ ] Service calls only foundation services — no direct broker calls (ts-processings-001)
- [ ] Service serves only one entity type (ts-processings-002)
- [ ] Operations are higher-order combinations of foundation calls (ts-processings-003)
- [ ] All foundation exceptions are caught and wrapped (ts-processings-004)
- [ ] Interface follows I{Entity}ProcessingService naming pattern (ts-processings-005)
- [ ] Service is a partial class split by operation category (ts-processings-006)
- [ ] Service does not call another processing service (ts-processings-001)
- [ ] No duplicate validation that is already handled by the foundation service (ts-processings-002)
