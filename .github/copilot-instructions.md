# Copilot Code Review Instructions

## ✅ Line Length (Top Priority)
- **Maximum: 120 characters per physical line.**
- Measure **raw file characters only**.
- **Tabs = 4 characters** for measurement.
- Each newline-delimited line is measured **independently**.
- **Ignore soft wrapping**.
- Whitespace-only lines count as **valid blank lines**.
- Auto-generated files may be ignored.

### ✅ Correct wrapping
```csharp
public async Task DoSomethingAsync(
    string input)
```

### ❌ Incorrect (>120 chars)
```csharp
public async Task DoSomethingAsync(string input, string message, string somethingElse, int count, bool enabled)
```

---

## ✅ Blank Line Rules
- Separate statements must follow these rules:
  - **Single-line → single-line:** no blank line required.
  - **Multi-line statements:**  
    - Preceded by **exactly one blank line** when starting a **new** statement.  
    - **NOT** required when the line is a continuation.
- First statement after `{` never requires a blank line.
- A `return` must have **exactly one blank line before it**, unless it is the first statement inside a block.

### ✅ Correct return
```csharp
var x = 1;

return x;
```

### ❌ Incorrect return
```csharp
var x = 1;
return x;
```

---

## ✅ Continuation Detection (Exact Rules)
A line **is a continuation** when:

1. The previous trimmed line **does not end with** `;` or `}`, **and**
2. The current trimmed line begins with:  
   `.`, `??`, `?`, `:`, `+`, `-`, `*`, `/`, `%`, `&&`, `||`, `=>`, `,`, `)`, `]`  
   **or** begins an argument of an open `(`, interpolation `$"`, LINQ chain, method call, or other incomplete constructs.

➡ **Continuation lines NEVER require a preceding blank line.**

---

## ✅ Argument / Invocation Formatting
- First line ends before the first argument.
- Wrapped lines are indented **one level (4 spaces)**.
- Closing `)` aligns with the start of the invocation.
- Do not use deeper indentation levels.

### ✅ Correct
```csharp
DoSomething(
    first,
    second);
```

### ❌ Incorrect (extra indentation)
```csharp
DoSomething(
        first,
        second);
```

---

## ✅ More Correct Examples

### ✅ Wrapped invocation, all lines ≤120
```csharp
Validate(
    () => new InvalidDecisionPollException(
        message: "Invalid decisionPoll, fix errors."),
    (Rule: rule, Parameter: name));
```

### ✅ Proper LINQ layout
```csharp
var users = allUsers
    .Where(u => u.IsActive)
    .OrderBy(u => u.Name)
    .ToList();
```

### ❌ Single overlong line
```csharp
var users = allUsers.Where(u => u.IsActive).OrderBy(u => u.Name).ToList();
```

---

## ✅ Enforcement Rules for Copilot
During PR review Copilot must:

- Measure **each physical line individually**.
- Flag violations **only** when a single physical line exceeds 120 characters.
- Suggest wrapping only when a violation exists.
- Never flag whitespace-only lines.
- Never require a blank line before a continuation.
- Enforce blank line before `return` as per rules.
- Apply the **block-first statement** exemption (no blank line required if previous meaningful line ends with `{`).

---

## ✅ Supporting .editorconfig Settings
```ini
[*.cs]
guidelines = 120
indent_style = space
indent_size = 4
tab_width = 4
trim_trailing_whitespace = true
end_of_line = crlf
```
