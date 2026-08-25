---
skill: the-standard-team-branching
type: template
source-section: "4.1.1.2 Branch Name Conventions, 4.1.1.3 Category List"
---

# Branch Naming Guide

## Pattern

```
users/[username]/[category]-[entity]-[action]
```

## Variables

| Variable | Description | Example |
|---|---|---|
| `[username]` | Your GitHub username | `jsmith` |
| `[category]` | Approved category (lowercase) | `foundations` |
| `[entity]` | Domain entity (lowercase) | `student` |
| `[action]` | Action performed (lowercase) | `add` |

## Examples

```
users/jsmith/foundations-student-add
users/jsmith/brokers-student-storage
users/jsmith/data-student-model
users/jsmith/controllers-student-post
```

## Approved Categories

| Category | Use When |
|---|---|
| `infra` | Initial project setup, build scripts |
| `data` | Creating a data model |
| `brokers` | Creating a broker |
| `foundations` | Creating a Foundation Service |
| `processings` | Creating a Processing Service |
| `orchestrations` | Creating an Orchestration Service |
| `coordinations` | Creating a Coordination Service |
| `managements` | Creating a Management Service |
| `aggregations` | Creating an Aggregation Service |
| `controllers` | Creating a Controller |
| `views` | Creating a View Service |
| `components` | Creating a Component |
| `pages` | Creating a Page |
| `acceptance` | Writing an Acceptance Test |
| `integration` | Writing an Integration Test |
| `config` | Configuration changes |
| `documentation` | General documentation |
| `design` | Architecture design documentation |
| `fix` | Bug fix |
| `import` | Copying code from another system |
| `release` | Release infrastructure |
| `provision` | Provision project/scripts |
