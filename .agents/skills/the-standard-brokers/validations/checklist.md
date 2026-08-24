# The Standard — Brokers — Checklist

- [ ] Broker contains no business logic (ts-brokers-001)
- [ ] Broker contains no validation logic (ts-brokers-001)
- [ ] Broker interface name is technology-agnostic (ts-brokers-002)
- [ ] Storage broker methods use: InsertAsync, SelectAsync, UpdateAsync, DeleteAsync (ts-brokers-003)
- [ ] API broker methods use: PostAsync, GetAsync, PutAsync, DeleteAsync (ts-brokers-004)
- [ ] Queue broker methods use: EnqueueAsync, DequeueAsync (ts-brokers-005)
- [ ] Broker accepts only primitive types or native models as input (ts-brokers-006)
- [ ] Broker returns raw external response without transformation (ts-brokers-007)
- [ ] Configuration retrieved via IConfiguration injected in constructor (ts-brokers-008)
- [ ] Broker is a partial class when it supports multiple entities (ts-brokers-009)
- [ ] Broker does not call another broker (ts-brokers-001)
- [ ] No internal infrastructure types (DbContext, HttpClient) are exposed publicly (ts-brokers-001)
