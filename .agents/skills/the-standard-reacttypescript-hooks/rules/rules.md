# The Standard React TypeScript — Hooks — Rules

## Hook Responsibility
**tsr-hooks-001** [ERROR] Page hooks MUST call view services for data, never brokers or foundation services.
**tsr-hooks-002** [ERROR] Hook files MUST follow pattern use{Purpose}.ts (e.g., useDashboardPage.ts).
**tsr-hooks-003** [ERROR] Page hooks MUST manage loading and error state.
**tsr-hooks-004** [ERROR] Hooks MUST return object with data, loading, error, and handlers.
**tsr-hooks-005** [ERROR] Hooks MUST use useEffect for data fetching on mount.
**tsr-hooks-006** [ERROR] Hooks MUST be in src/hooks/ directory.
**tsr-hooks-007** [ERROR] Hooks MUST NOT contain UI rendering logic.
**tsr-hooks-008** [ERROR] Hooks MUST NOT call brokers directly.
**tsr-hooks-009** [ERROR] Hooks MUST handle exceptions from view services.
**tsr-hooks-010** [ERROR] Page hooks MUST fetch data on mount and when dependencies change.

## Hook Structure
**tsr-hooks-011** [ERROR] Hooks MUST use TypeScript for all types.
**tsr-hooks-012** [ERROR] Hooks MUST export as named exports.
**tsr-hooks-013** [WARN] Hooks SHOULD be under 100 lines.
**tsr-hooks-014** [ERROR] Event handlers MUST use useCallback when passed to components.
**tsr-hooks-015** [ERROR] Hooks MUST clean up effects properly.
