# The Standard React TypeScript — Pages — Rules

## Page Responsibility
**tsr-pages-001** [ERROR] Pages MUST use page hooks for data fetching and business logic.
**tsr-pages-002** [ERROR] Pages MUST be in src/pages/ directory.
**tsr-pages-003** [ERROR] Page files MUST follow pattern {Domain}Page.tsx (e.g., DashboardPage.tsx).
**tsr-pages-004** [ERROR] Pages MUST compose reusable components, not duplicate rendering logic.
**tsr-pages-005** [ERROR] Pages MUST handle route-level concerns only (layout, navigation, hook orchestration).
**tsr-pages-006** [ERROR] Pages MUST export as named exports, not default exports.
**tsr-pages-007** [ERROR] Pages MUST NOT contain business logic.
**tsr-pages-008** [ERROR] Pages MUST NOT fetch data directly (use hooks).
**tsr-pages-009** [ERROR] Pages MUST NOT call services directly.
**tsr-pages-010** [ERROR] Pages MUST handle loading/error states from hooks.

## Page Structure
**tsr-pages-011** [ERROR] Pages MUST use exactly one page hook.
**tsr-pages-012** [WARN] Pages SHOULD be under 150 lines.
**tsr-pages-013** [ERROR] Pages MUST use early returns for loading/error states.
**tsr-pages-014** [ERROR] Pages MUST compose components, not inline all JSX.
**tsr-pages-015** [ERROR] Pages MUST NOT duplicate component logic.
