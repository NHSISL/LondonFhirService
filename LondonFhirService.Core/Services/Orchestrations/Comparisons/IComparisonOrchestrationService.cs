// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading.Tasks;
using LondonFhirService.Core.Models.Orchestrations.Comparisons;

public interface IComparisonOrchestrationService
{
    ValueTask<ComparisonResult> CompareAsync(
            string correlationId,
            string source1Json,
            string source2Json);
}