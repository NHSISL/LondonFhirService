# The Standard React TypeScript — Components — Rules

## Component Responsibility
**tsr-comp-001** [ERROR] Components MUST be pure rendering functions without side effects or business logic.
**tsr-comp-002** [ERROR] Components MUST receive all data via props, never fetch data directly.
**tsr-comp-003** [ERROR] Components MUST use TypeScript for props definition.
**tsr-comp-004** [ERROR] Components MUST be in src/components/{domain}/ directory.
**tsr-comp-005** [ERROR] Component files MUST follow pattern {Domain}{Purpose}.tsx (e.g., PatientCard.tsx).
**tsr-comp-006** [ERROR] Components MUST export as named exports, not default exports.
**tsr-comp-007** [ERROR] Components MUST handle loading/error states via props, not internal state.
**tsr-comp-008** [ERROR] Components MUST NOT call services, brokers, or APIs directly.
**tsr-comp-009** [ERROR] Components MUST delegate all business logic to services via page hooks.
**tsr-comp-010** [ERROR] Components MUST NOT use useEffect for data fetching.

## Component Structure
**tsr-comp-011** [ERROR] Components MUST use function component syntax, not class components.
**tsr-comp-012** [ERROR] Component props MUST be destructured in function signature.
**tsr-comp-013** [WARN] Components SHOULD be under 200 lines; split if larger.
**tsr-comp-014** [ERROR] Components MUST have single responsibility.
**tsr-comp-015** [ERROR] Event handlers MUST be passed as props, not defined inline when complex.

## Props and Types
**tsr-comp-016** [ERROR] Props type MUST be defined in models/components/{domain}/.
**tsr-comp-017** [ERROR] Optional props MUST use ? syntax.
**tsr-comp-018** [ERROR] Callback props MUST be optional unless always required.
**tsr-comp-019** [ERROR] Props MUST NOT include foundation models directly.
**tsr-comp-020** [WARN] Components SHOULD use view models for data props.
