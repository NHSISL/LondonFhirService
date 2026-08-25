# The Standard — Practices — Anti-Patterns

## secrets-in-source

**Violates:** ts-practices-006
**What happens:** A connection string or API key is hardcoded in `appsettings.json` or committed in a `.env` file.
**Why it's wrong:** Credentials in source control are permanently exposed in git history even after deletion. They are a critical security vulnerability.
**Fix:** Use environment variables, Azure Key Vault, GitHub Secrets, or a `.env` file excluded by `.gitignore`. Reference via `IConfiguration`.

## failing-pr

**Violates:** ts-practices-005
**What happens:** A pull request is opened with one or more failing tests or a broken build.
**Why it's wrong:** A failing PR blocks other contributors and signals the work is not complete. Reviews on broken code are wasted effort.
**Fix:** Fix all failures locally before opening the PR. CI must be green before requesting review.

## vague-commits

**Violates:** ts-practices-003b
**What happens:** A commit message reads "fix", "update stuff", or "wip".
**Why it's wrong:** Vague messages make git history unreadable and block automated tooling that parses commit types.
**Fix:** Use the format `{CATEGORY}: {Imperative Description}` — e.g., `BROKERS: Fix Student Insert Null Check`.

## mixed-pr

**Violates:** ts-practices-004
**What happens:** A single PR adds a new broker, refactors a controller, and updates documentation.
**Why it's wrong:** Mixed-concern PRs are difficult to review, hard to revert, and conflate unrelated history.
**Fix:** Split into separate PRs, each targeting one concern with one clear commit history.
