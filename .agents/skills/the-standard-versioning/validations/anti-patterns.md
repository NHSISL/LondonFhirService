# The Standard — Versioning — Anti-Patterns

## breaking-minor

**Violates:** ts-versioning-002
**What happens:** A public method signature changes (parameter removed or renamed) in a release versioned as `1.1.0`.
**Why it's wrong:** MINOR releases must be backward-compatible. Changing a public signature is a breaking change that requires a MAJOR bump to `2.0.0`.
**Fix:** Bump to `2.0.0` and maintain the old signature in a deprecated route or overload for the transition period.

## silent-breaking-change

**Violates:** ts-versioning-002
**What happens:** A public property is renamed from `StudentName` to `Name` without a version bump.
**Why it's wrong:** Consumers silently break at runtime or compile time with no version signal. They cannot protect themselves via version pinning.
**Fix:** Keep `StudentName` as a deprecated alias or bump to a new MAJOR version with a documented migration guide.

## bad-version-string

**Violates:** ts-versioning-001
**What happens:** A `.csproj` contains `<Version>1.0.0-alpha-final-v2-hotfix</Version>`.
**Why it's wrong:** Non-semantic version strings cannot be compared, sorted, or range-pinned by package managers.
**Fix:** Use `1.0.0-alpha.1` or `1.0.0-rc.2` per the SemVer 2.0 pre-release spec.

## internal-versioning

**Violates:** ts-versioning-001
**What happens:** A private helper method or internal class is versioned in its name (e.g., `ValidateStudentV2`).
**Why it's wrong:** Versioning internal implementation details is noise. Only public API surfaces need versioning.
**Fix:** Rename to a meaningful name without a version suffix. If the behavior differs, use a private method with a descriptive name.
