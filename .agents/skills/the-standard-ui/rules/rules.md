# The Standard — User Interfaces — Rules

## View Service Dependency

**ts-ui-001** [ERROR] UI components must delegate all data access and business calls to a view service — never call foundation, processing, or broker types directly.

## Single Concern

**ts-ui-002** [ERROR] UI components must have a single rendering concern — decompose if a component renders multiple distinct UI sections.

## View Service Mapping

**ts-ui-003** [ERROR] View services must map UI-layer exceptions to view-friendly errors and expose only view models, not domain models.

## Code-Behind Separation

**ts-ui-004** [ERROR] Pages must use a base component class (`{ComponentName}Base : ComponentBase`) to separate code-behind logic from markup.

## State Handling

**ts-ui-005** [ERROR] UI components must handle loading, error, and empty states explicitly — never leave the UI blank or crashing on async operations.

## Naming

**ts-ui-006** [ERROR] Component names must describe what they render, not how they render it.
