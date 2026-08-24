# The Standard — Versioning — Checklist

- [ ] Version follows MAJOR.MINOR.PATCH format (ts-versioning-001)
- [ ] Breaking changes increment MAJOR (ts-versioning-002)
- [ ] New features increment MINOR (ts-versioning-003)
- [ ] Bug fixes increment PATCH (ts-versioning-004)
- [ ] API version is in the URL path: /api/v{major}/ (ts-versioning-005)
- [ ] Previous API version is still available if consumers exist (ts-versioning-006)
- [ ] Version is declared in <Version> element of .csproj (ts-versioning-007)
- [ ] No public API members removed or renamed without a MAJOR bump
