---
skill: the-standard-team-commits
type: template
source-section: "4.1.3 Commits"
---

# Commit Message Templates

## TDD Commit (FOUNDATIONS, PROCESSINGS, ORCHESTRATIONS, COORDINATIONS, MANAGEMENTS, AGGREGATIONS, VIEWS, COMPONENTS, PAGES, ACCEPTANCE, INTEGRATION)

When writing a failing test:
```
[Test Method Name] -> FAIL
```

When writing the passing implementation:
```
[Test Method Name] -> PASS
```

Examples:
```
ShouldAddStudentAsync -> FAIL
ShouldAddStudentAsync -> PASS
```

---

## Non-TDD Commit (DATA, BROKERS, CONTROLLERS, INFRA, CONFIG, DOCUMENTATION, DESIGN, IMPORT, STATUS, PROVISION, RELEASE)

```
[CATEGORY]: [Description Of Work Completed In Pascal Case]
```

Examples:
```
BROKERS: Insert Student
DATA: Add Student Model
CONTROLLERS: POST Student
CONFIG: Add Storage Connection String Settings
```
