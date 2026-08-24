# The Standard — Orchestration Services — Checklist

- [ ] Service depends only on processing services — no foundation services or brokers (ts-orchestrations-001)
- [ ] Each entity type is managed through its own dedicated processing service (ts-orchestrations-002)
- [ ] All downstream exceptions are caught and wrapped in orchestration-level exceptions (ts-orchestrations-003)
- [ ] Interface follows I{Purpose}OrchestrationService naming pattern (ts-orchestrations-004)
- [ ] Service is a partial class split by operation category (ts-orchestrations-005)
- [ ] No business logic that belongs in a processing service exists here (ts-orchestrations-001)
- [ ] No foundation-level exception types are leaked upward (ts-orchestrations-003)
