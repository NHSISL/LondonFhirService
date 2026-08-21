// ---
// skill: the-standard-csharp-directives
// type: example
// source-section: "3. Directives"
// demonstrates: "tsc-csharp-directives-001, tsc-csharp-directives-002, tsc-csharp-directives-003, tsc-csharp-directives-005"
// ---

// ❌ Missing blank lines between groups — violates tsc-csharp-directives-003
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using FluentAssertions;
using MyProject.Models.Students;

// ❌ Wrong group order (project-local before System) — violates tsc-csharp-directives-002
using MyProject.Models.Students;
using System;
using Microsoft.AspNetCore.Mvc;

// ❌ Directives inside namespace — violates tsc-csharp-directives-001
namespace MyProject.Services
{
    using System;
    using MyProject.Models;

    public class StudentService { }
}

// ❌ Unused directive — violates tsc-csharp-directives-005
using System.Text.Json; // never used in this file
