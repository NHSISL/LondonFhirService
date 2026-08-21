// ---
// skill: the-standard-csharp-methods
// type: template
// source-section: "1. Methods"
// ---

// One-liner method template
public {ReturnType} {VerbEntityAction}({ParamType} {entityParam}) =>
    this.{dependency}.{Action}({entityParam});

// Async one-liner method template
public async ValueTask<{ReturnType}> {VerbEntityActionAsync}({ParamType} {entityParam}) =>
    await this.{dependency}.{ActionAsync}({entityParam});

// Multi-liner method template
public async ValueTask<{ReturnType}> {VerbEntityActionAsync}({ParamType} {entityParam})
{
    // preparation logic
    {LocalType} {localVar} = await this.{dependency}.{ActionAsync}({entityParam});

    return {localVar};
}

// Long declaration template (> 120 chars)
public async ValueTask<{ReturnType}> {VerbEntityActionAsync}(
    {Param1Type} {param1},
    {Param2Type} {param2})
{
    // body
}

// Method chain — uglification template
{collection}.{Method1}(...)
    .{Method2}(...)
        .{Method3}(...);
