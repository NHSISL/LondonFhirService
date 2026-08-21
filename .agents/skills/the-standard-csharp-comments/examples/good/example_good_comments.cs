// ---
// skill: the-standard-csharp-comments
// type: example
// source-section: "12.1 Copyrights, 12.2 Methods"
// demonstrates: "tsc-csharp-comments-002, tsc-csharp-comments-005"
// ---

// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE TO CONNECT THE WORLD
// ---------------------------------------------------------------

/// <summary>
/// Retrieves an encrypted token for the given identity.
/// Purposing: Exchange an identity credential for a bearer token.
/// Incomes: identityId — the unique identifier of the identity.
/// Outcomes: A signed JWT string.
/// Side Effects: Writes an audit log entry on each call.
/// </summary>
private async ValueTask<string> RetrieveTokenAsync(Guid identityId) { ... }
