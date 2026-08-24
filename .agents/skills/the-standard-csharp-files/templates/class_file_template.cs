// ---
// skill: the-standard-csharp-files
// type: template
// source-section: "0. Files"
// ---

// File: {EntityName}.cs
namespace {Namespace}
{
    public class {EntityName}
    {
        // Primary class members
    }
}

// ---- Partial class root file ----

// File: {EntityName}Service.cs
namespace {Namespace}
{
    public partial class {EntityName}Service : I{EntityName}Service
    {
        // Core dependencies and constructor
    }
}

// ---- Partial class validations file ----

// File: {EntityName}Service.Validations.cs
namespace {Namespace}
{
    public partial class {EntityName}Service
    {
        // Validation methods
    }
}

// ---- Partial class exceptions file ----

// File: {EntityName}Service.Exceptions.cs
namespace {Namespace}
{
    public partial class {EntityName}Service
    {
        // Exception handling methods
    }
}
