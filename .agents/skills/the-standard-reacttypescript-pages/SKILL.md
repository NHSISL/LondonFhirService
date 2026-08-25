---
name: the-standard-reacttypescript-pages
version: 0.1.0
applies-to: [""src/pages/**/*.tsx""]
depends-on: [""the-standard-reacttypescript-components"", ""the-standard-reacttypescript-hooks""]
---
# Pages
## 0/ Context
0.0/ Where: Page layer
0.1/ Who: Frontend engineers
0.2/ What: Route-level pages using hooks
0.3/ Applies to: src/pages/**/*.tsx
0.4/ Version: v0.1.0
0.5/ Depends on: components, hooks
## 1/ Actual
1.0/ Dos:
1. Pages MUST use page hooks
2. Pages compose components
3. Named exports
1.1/ Don'ts:
1. No business logic in pages
2. No data fetching in pages
1.4/ Examples: see examples/
1.6/ Checklists: see validations/checklist.md
## 2/ Expected
2.1/ Outcome: Clean pages using hooks and components
