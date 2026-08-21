---
name: the-standard-reacttypescript-hooks
version: 0.1.0
standard-version: v2.50.0
applies-to: ["src/hooks/**/*.ts"]
depends-on: ["the-standard-reacttypescript-typescript", "the-standard-reacttypescript-view-services"]
---

# The Standard React TypeScript — Hooks

## 0/ Context (Purpose — WHY this skill exists)

0.0/ Where: React + TypeScript + Vite projects following The Standard hooks layer
0.1/ Who: Frontend engineers implementing page hooks for data/logic orchestration
0.2/ What: Governs hook implementations that bridge pages and view services
0.3/ Applies to: src/hooks/**/*.ts — all hook files
0.4/ Version: The Standard React TypeScript v0.1.0
0.5/ Depends on: the-standard-reacttypescript-typescript, the-standard-reacttypescript-view-services

## 1/ Actual (Dependency — WHAT the rules are and what they depend on)

1.0/ Dos:

1. Page hooks MUST call view services → see rules/rules.md#tsr-hooks-001
2. Hooks MUST follow use{Purpose} pattern → see rules/rules.md#tsr-hooks-002
3. Page hooks MUST manage loading/error state → see rules/rules.md#tsr-hooks-003
4. Hooks MUST return data and handlers → see rules/rules.md#tsr-hooks-004
5. Hooks MUST use useEffect for data fetching → see rules/rules.md#tsr-hooks-005

1.1/ Don'ts:

1. Never call brokers from hooks → see validations/anti-patterns.md#broker-in-hook
2. Never put UI logic in hooks → see validations/anti-patterns.md#ui-logic-in-hook

1.2/ Ask:
- When uncertain if logic belongs in hook or service
- When deciding hook granularity

1.3/ Defaults:
- Page hooks in src/hooks/
- Call view services for data
- Manage state with useState
- Handle effects with useEffect
- Return {data, loading, error, handlers}

1.4/ Examples: see examples/

1.5/ Templates: see templates/

1.6/ Checklists: see validations/checklist.md

1.7/ Contracts: see contracts/contracts.json

## 2/ Expected (Exposure — WHAT comes out)

2.0/ Format: TypeScript custom hooks (.ts)
2.1/ Outcome: Clean page hooks orchestrating view services. State managed. Effects handled. Data and handlers exposed.
2.2/ Tone: Direct. Cite rule IDs. Block brokers. Require view services. Enforce patterns.
