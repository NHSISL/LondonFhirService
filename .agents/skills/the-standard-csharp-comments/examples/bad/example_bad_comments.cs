// ---
// skill: the-standard-csharp-comments
// type: example
// source-section: "12.1 Copyrights, 12.2 Methods"
// demonstrates: "tsc-csharp-comments-003, tsc-csharp-comments-004, tsc-csharp-comments-006"
// ---

// ❌ Block comment copyright — violates tsc-csharp-comments-003
/* 
 * ==============================================================
 * Copyright (c) Coalition of the Good-Hearted Engineers
 * FREE TO USE TO CONNECT THE WORLD
 * ==============================================================
 */

// ❌ XML copyright tag — violates tsc-csharp-comments-004
//----------------------------------------------------------------
// <copyright file="StudentService.cs" company="OpenSource">
//      Copyright (C) Coalition of the Good-Hearted Engineers
// </copyright>
//----------------------------------------------------------------

// ❌ Redundant comment — violates tsc-csharp-comments-001, tsc-csharp-comments-006
// increment i
i++;

// ❌ Missing method documentation for inaccessible logic — violates tsc-csharp-comments-005
private async ValueTask<string> RetrieveTokenAsync(Guid identityId) { ... }
