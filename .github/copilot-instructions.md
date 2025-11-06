# Copilot Instructions

## Code Style Rules

### Line Length

-   All `.cs` source files must adhere to the following rule:
    -   No line of code should exceed **120 characters** in length.
    -   This includes comments, string literals, and code.
    -   Exception: automatically generated files may be ignored if they
        cannot be reformatted safely.
-   **How to measure (raw file characters, per-line only):**
    -   Count based on **raw file characters**, not editor rendering.
    -   **Tabs count as 4 characters** for measurement.
    -   Trailing whitespace must be removed.
    -   **Per-physical-line measurement ONLY.** The unit of measurement
        is a **single newline-delimited line**.
        -   **Never** add or aggregate the lengths of multiple lines.
        -   A wrapped invocation is compliant if **each** physical line
            is ≤ 120 characters.
    -   Ignore soft wrapping (on-screen wrapping that doesn't insert a
        newline).

### Code Formatting

-   Single-line instructions must follow each other with **no blank
    lines** in between.
-   Multi-line instructions must always be preceded by **exactly one
    blank line**.
-   If a multi-line instruction is followed by further instructions, it
    must also be followed by **exactly one blank line**.
-   **Exception (block first statement):** If a statement is the **first
    statement inside a block**---i.e., directly after `{`---**no
    preceding blank line** is required.
-   Any C# `return` statement must be preceded by **exactly one blank
    line** unless it is the first statement in a block.
-   If a constructor/method name would push a line past 120 characters,
    move `new`, the call, or the arguments to the next line.
-   Always format so that **no single physical line exceeds 120
    characters**, even when calls span multiple lines.
-   **Definition of a blank line (updated):**\
    A blank line is any physical line that contains **no visible
    characters**.\
    After trimming whitespace, the line must be empty.\
    Lines containing only spaces or tabs **are valid blank lines**.
-   **Method separation:** Method declarations must be preceded by
    **exactly one blank line** after the closing brace of the previous
    member.
-   **Argument indentation:**
    -   For multi-line method or constructor calls, the first line ends
        before the first argument.\
    -   Each wrapped argument line must be indented **one additional
        indentation level** (usually 4 spaces).\
    -   Do **not** use extra indentation levels.\
    -   The closing `)` must align with the start of the call.

### Enforcement

-   Copilot should **not generate code** that exceeds the 120-character
    line limit.
-   When writing new C# code, Copilot should:
    -   Break up long method/constructor calls across multiple lines.
    -   Use string interpolation or verbatim strings with proper line
        breaks where needed.
    -   Format long LINQ queries across multiple lines.
    -   Wrap parameters and arguments for readability.
    -   Insert a blank line before any `return` following other
        statements.
    -   Prefer moving `new` or the method invocation to the next line
        when appropriate.

### Review Guidelines (strict)

-   Copilot must:
    -   Evaluate **each physical line independently**.
    -   Flag a violation only when a **single physical line** exceeds
        120 characters.
    -   When flagging, include line number and measured character count.
    -   Suggest multiline formatting only when the offending line
        exceeds 120.
    -   **Not** flag whitespace-only lines; they are valid blank lines.
    -   Flag missing blank lines before `return`.
-   **Operator lines:**
    -   Measure compliance per physical line.\
    -   Do not combine operator lines with continuations.\
    -   Operator-at-end style is preferred.
-   **Block-first statement exemption:**
    -   Do not require a preceding blank line if the previous meaningful
        line ends with `{`.

### Examples

#### ✅ Correct (first statement inside a block; no blank line required)

``` csharp
public void Foo()
{
    DoSomething(
        x,
        y);
}
```

#### ❌ Incorrect (blank line required between two statements)

``` csharp
DoSomething();
DoSomethingElse(
    x,
    y);
```

#### ✅ Correct (wrapped invocation; each line ≤ 120)

``` csharp
Validate(
    createException: () => new InvalidDecisionPollException(
        message: "Invalid decisionPoll. Please correct the errors and try again."),

    (Rule: IsInvalid(decisionPoll.Id), Parameter: nameof(DecisionPoll.Id)));
```

#### ❌ Incorrect (single line \> 120)

``` csharp
Validate(createException: () => new InvalidDecisionPollException(message: "Invalid decisionPoll. Please correct the errors and try again."));
```

------------------------------------------------------------------------

### Code Formatting Rule Examples

#### ✅ Correct (return with blank line)

``` csharp
var user = users.FirstOrDefault(u => u.Id == id);

return user;
```

#### ❌ Incorrect

``` csharp
var user = users.FirstOrDefault(u => u.Id == id);
return user;
```

------------------------------------------------------------------------

### Argument Indentation Examples

#### ✅ Correct

``` csharp
DoSomething(
    firstArgument: "value1",
    secondArgument: "value2",
    thirdArgument: "value3");
```

#### ❌ Incorrect (extra indentation)

``` csharp
DoSomething(
        firstArgument: "value1",
        secondArgument: "value2",
        thirdArgument: "value3");
```

#### ❌ Incorrect (misaligned closing parenthesis)

``` csharp
DoSomething(
    firstArgument: "value1",
    secondArgument: "value2",
    thirdArgument: "value3"
    );
```

------------------------------------------------------------------------

### More Formatting Examples

#### ✅ Correct

``` csharp
var filteredUsers = users
    .Where(u => u.IsActive && u.LastLoginDate >= DateTime.UtcNow.AddDays(-30))
    .OrderByDescending(u => u.LastLoginDate)
    .Select(u => new
    {
        u.Id,
        u.Name,
        u.Email,
        LastSeen = u.LastLoginDate.ToString("yyyy-MM-dd HH:mm:ss")
    })
    .ToList();
```

#### ❌ Incorrect

``` csharp
var filteredUsers = users.Where(u => u.IsActive && u.LastLoginDate >= DateTime.UtcNow.AddDays(-30)).OrderByDescending(u => u.LastLoginDate).Select(u => new { u.Id, u.Name, u.Email, LastSeen = u.LastLoginDate.ToString("yyyy-MM-dd HH:mm:ss") }).ToList();
```

------------------------------------------------------------------------

### Rationale

-   **Per-line measurement** prevents false positives in wrapped calls.
-   **Tabs count as 4 characters** ensures consistent line-length
    calculation.
-   **Whitespace-only blank lines count as blank** and match VS
    behaviour.
-   **Return visibility** improves readability.
-   **Argument indentation** improves consistency.
-   **Block-first patterns** avoid unnecessary whitespace noise.

------------------------------------------------------------------------

## Supporting .editorconfig Settings

``` ini
[*.{cs,vb,ts,tsx}]
guidelines = 120
indent_style = space
indent_size = 4
trim_trailing_whitespace = true
end_of_line = crlf

dotnet_sort_system_directives_first = true
```
