# The Standard — User Interfaces — Anti-Patterns

## razor-calls-service

**Violates:** ts-ui-001
**What happens:** A `.razor` file injects `IStudentService` directly and calls it in `OnInitializedAsync`.
**Why it's wrong:** Components must not depend on domain services. This tightly couples the UI to the business layer and makes the component untestable without the full service stack.
**Fix:** Create a view service (`IStudentViewService`) and inject that instead.

## mixed-concerns

**Violates:** ts-ui-002
**What happens:** A single Razor component renders a student list, a course panel, and a teacher directory all in one file.
**Why it's wrong:** Single responsibility requires each component to render one distinct UI section.
**Fix:** Decompose into `StudentListComponent`, `CoursePanelComponent`, and `TeacherDirectoryComponent`, then compose them in the page.

## code-behind-logic

**Violates:** ts-ui-004
**What happens:** All logic lives inside an `@code {}` block in the `.razor` file with `HttpClient` calls or complex state management.
**Why it's wrong:** Markup and logic mixed in one file cannot be tested independently. Logic in `@code {}` is untestable via unit tests.
**Fix:** Move all logic to a `{ComponentName}Base : ComponentBase` class. The `.razor` file inherits it and contains only markup.

## mega-component

**Violates:** ts-ui-002
**What happens:** A single `StudentPage.razor` contains 400 lines of markup covering the full CRUD flow in one file.
**Why it's wrong:** A mega-component is impossible to maintain, test, or reuse. It violates single responsibility.
**Fix:** Extract each section (StudentList, StudentForm, StudentDetails) into dedicated components composed by the page.
