---
name: the-standard-reacttypescript-state
version: 0.1.0
applies-to: ["src/**/*.ts", "src/**/*.tsx"]
depends-on: ["the-standard-reacttypescript-hooks"]
---
# State Management
## 0/ Context
0.0/ Where: State management layer
0.1/ Who: Frontend engineers
0.2/ What: State management patterns
0.3/ Applies to: All state usage
0.4/ Version: v0.1.0
0.5/ Depends on: hooks
## 1/ Actual
1.0/ Dos:
1. Use useState for component state → see rules/rules.md#tsr-state-001
2. Use useReducer for complex state → see rules/rules.md#tsr-state-002
3. Lift state to appropriate level → see rules/rules.md#tsr-state-003
4. Use context for global state → see rules/rules.md#tsr-state-004
1.1/ Don'ts:
1. No prop drilling → see validations/anti-patterns.md
2. No global mutable state → see validations/anti-patterns.md
1.4/ Examples: see examples/
1.6/ Checklists: see validations/checklist.md
## 2/ Expected
2.1/ Outcome: Clean state management using React patterns
