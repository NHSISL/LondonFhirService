# The Standard — Aggregation Services — Checklist

- [ ] Service depends only on orchestration services (ts-aggregations-001)
- [ ] Service exposes a single operation that fans out to orchestration services (ts-aggregations-002)
- [ ] All downstream exceptions are wrapped in aggregation-level exceptions (ts-aggregations-003)
- [ ] Interface follows I{Purpose}AggregationService naming pattern (ts-aggregations-004)
- [ ] Service is a partial class split by operation category (ts-aggregations-005)
- [ ] No business logic is implemented inside the aggregation service (ts-aggregations-002)
- [ ] No orchestration exception types are leaked upward (ts-aggregations-003)
