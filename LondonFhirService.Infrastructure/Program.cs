// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using LondonFhirService.Infrastructure.Services;

namespace LondonFhirService.Infrastructure
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var scriptGenerationService = new ScriptGenerationService();

            scriptGenerationService.GenerateBuildScript(
                branchName: "main",
                projectName: "LondonFhirService.Core",
                dotNetVersion: "10.0.100");

            scriptGenerationService.GeneratePrLintScript(branchName: "main");
        }
    }
}
