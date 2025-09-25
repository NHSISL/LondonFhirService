# Copilot Instructions

## Code Style Rules

### Line Length
- All `.cs` source files must adhere to the following rule:
  - No line of code should exceed **120 characters** in length.
  - This includes comments, string literals, and code.
  - Exception: automatically generated files may be ignored if they cannot be reformatted safely.
- **How to measure (raw file characters, per-line only):**
  - Count based on **raw file characters**, not editor rendering.
  - Tabs must always be converted to spaces (`indent_style = space`).
  - Trailing whitespace must be removed.
  - **Per-physical-line measurement ONLY.** The unit of measurement is a **single newline-delimited line**.
    - **Never** add or aggregate the lengths of multiple lines (e.g., “when considering the full parameter”). That is **not allowed**.
    - A wrapped invocation is compliant if **each** physical line is ≤ 120 characters, even if the total characters across lines exceed 120.
  - Ignore soft wrapping (on-screen wrapping that doesn’t insert a newline).

### Code Formatting
- Single-line instructions must follow each other with **no blank lines** in between.
- Multi-line instructions must always be preceded by **exactly one blank line**.
- If a multi-line instruction is followed by further instructions, it must also be followed by **exactly one blank line**.
- **Exception (block first statement):** If a statement (single- or multi-line) is the **first statement inside a block**—i.e., it immediately follows an opening `{` for a method, constructor, property, type, namespace, or control-flow block—**do not require a preceding blank line**.
- Any C# `return` statement must be preceded by **exactly one blank line** if it comes after other instructions.
- If a constructor/method name would push a line past **120 columns**, move `new`, the **method call**, or the **arguments** to the **next line**.
- Always format so that **no single physical line exceeds 120 characters**, even when calls span multiple lines.
- **Definition of a blank line**: a line must contain **no characters at all** (no spaces, no tabs). A line with only whitespace does **not** count as a blank line.
- **Method separation**: Method declarations (single-line or multi-line) must be preceded by **exactly one blank line** after the closing brace `}` of the previous member.
- **Argument indentation**:  
  - When a method or constructor call spans multiple lines, the first line ends before the first argument.  
  - Each wrapped argument line must be indented **exactly one additional indentation level** (typically 4 spaces) relative to the starting line.  
  - Do **not** use multiple extra indentation levels.  
  - The closing `)` must align with the start of the method/constructor call.

### Enforcement
- Copilot should **not generate or suggest code** that exceeds the 120-character line limit (as measured above).
- When writing new C# code, Copilot should:
  - Break up long method/constructor calls across multiple lines.
  - Use string interpolation or verbatim strings with proper line breaks if a literal would otherwise exceed 120 characters.
  - Format long LINQ queries across multiple lines.
  - Suggest wrapping parameters and arguments for readability.
  - Insert a blank line before any `return` statement that follows other instructions.
  - Prefer moving `new` (or method invocation) to the next line if the type or method name is long.

### Review Guidelines (strict)
- When reviewing or completing code suggestions, Copilot must:
  - Evaluate **each physical line independently**. Do **not** aggregate the lengths of adjacent lines or entire argument lists.
  - Scan `.cs` files for lines longer than 120 characters based on **raw file characters** (tabs already converted to spaces, trailing whitespace removed).
  - Only flag a violation if a **single physical line** exceeds 120 characters **under these rules**.
  - When flagging, include the **line number** and the **measured character count** (e.g., “Line 42: 128 chars”), and show the exact line being measured (truncated if needed).
  - Recommend a multiline formatting fix for flagged lines (e.g., move `new`, method call, or arguments) **only if the offending single line exceeds 120**.
  - Do **not** recommend changes (e.g., “move `new` to the next line”) if every involved line is already ≤ 120.
  - Flag missing blank lines before `return` statements.
  - Flag **whitespace-only lines** as invalid blank lines.
- **Operator lines**: For wrapped binary operators (`+`, `&&`, `||`, `??`, ternary `?:`), measure compliance **per physical line only**.  
  - Do **not** merge an operator-ending line with its continuation when counting characters.  
  - Both operator-at-end and operator-at-start styles are valid.  
  - Operator placement at the end of a line is the preferred style.
- **Block-first statement exemption**: Do **not** flag “missing blank line before multi-line instruction” when the previous **non-empty trimmed** line ends with an opening brace `{`.  
  - In other words, the **first statement inside a block** does **not** require a leading blank line.

### Examples

#### ✅ Correct (first statement inside a block; no blank line required)
```csharp
public void Foo()
{
    DoSomething(
        x,
        y);
}
```

#### ❌ Incorrect (blank line required between two statements)
```csharp
DoSomething();
DoSomethingElse(
    x,
    y);
```

#### ✅ Correct (wrapped invocation; each line ≤ 120; do not aggregate)
```csharp
Validate(
    createException: () => new InvalidDecisionPollException(
        message: "Invalid decisionPoll. Please correct the errors and try again."),
    (Rule: IsInvalid(decisionPoll.Id), Parameter: nameof(DecisionPoll.Id)));
```

#### ❌ Incorrect (single line > 120)
```csharp
Validate(createException: () => new InvalidDecisionPollException(message: "Invalid decisionPoll. Please correct the errors and try again."));
```

---

### Code Formatting Rule Examples

#### ✅ Correct (return with blank line)
```csharp
var user = users.FirstOrDefault(u => u.Id == id);

return user;
```

#### ❌ Incorrect (missing blank line before return)
```csharp
var user = users.FirstOrDefault(u => u.Id == id);
return user;
```

---

### Argument Indentation Examples

#### ✅ Correct
```csharp
DoSomething(
    firstArgument: "value1",
    secondArgument: "value2",
    thirdArgument: "value3");
```

#### ❌ Incorrect (unnecessary extra indentation)
```csharp
DoSomething(
        firstArgument: "value1",
        secondArgument: "value2",
        thirdArgument: "value3");
```

#### ❌ Incorrect (closing parenthesis misaligned)
```csharp
DoSomething(
    firstArgument: "value1",
    secondArgument: "value2",
    thirdArgument: "value3"
    );
```

---

### More Formatting Examples

#### ✅ Correct
```csharp
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
```csharp
var filteredUsers = users.Where(u => u.IsActive && u.LastLoginDate >= DateTime.UtcNow.AddDays(-30)).OrderByDescending(u => u.LastLoginDate).Select(u => new { u.Id, u.Name, u.Email, LastSeen = u.LastLoginDate.ToString("yyyy-MM-dd HH:mm:ss") }).ToList();
```

---

### Rationale

- **Per-line measurement**: Counting each physical line independently avoids false positives in wrapped invocations. Statements often exceed 120 characters **as a whole**, but are perfectly readable and compliant when wrapped.
- **Raw characters**: Counting raw file characters (with tabs converted to spaces and trailing whitespace removed) ensures consistent results across tools.
- **Blank lines**: Defining blank lines as truly empty eliminates whitespace-only edge cases.
- **Return visibility**: A blank line before `return` improves scanning and reduces review misses.
- **Long identifiers**: Allowing `new`/method/arguments to move to the next line guarantees a compliant, readable layout.
- **Argument indentation**: Consistent indentation (one level for wrapped arguments, aligned closing `)`) prevents unnecessary shifts that harm readability.
- **Block-first statement**: Not requiring a blank line after an opening `{` reduces noise and matches common C# conventions.

---

## Supporting .editorconfig Settings

To ensure consistency across editors and to avoid false positives due to tabs/whitespace, use the following in your `.editorconfig`:

```ini
# C# or VB (and optionally TS/TSX) files
[*.{cs,vb,ts,tsx}]
guidelines = 120
indent_style = space
indent_size = 4
trim_trailing_whitespace = true
end_of_line = crlf

#### Core EditorConfig Options ####

# Sort System.* using directives alphabetically, and place them before other usings
dotnet_sort_system_directives_first = true
```
