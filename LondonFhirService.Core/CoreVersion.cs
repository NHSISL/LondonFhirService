// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

namespace LondonFhirService.Core
{
    /// <summary>
    /// The release both hosts report - read from this assembly, so it is the Version in
    /// LondonFhirService.Core.csproj that a release bumps, and neither host carries a copy of its
    /// own that could fall behind.
    /// </summary>
    public static class CoreVersion
    {
        public static string Value { get; } =
            typeof(CoreVersion).Assembly.GetName().Version?.ToString() ?? "unknown";
    }
}
