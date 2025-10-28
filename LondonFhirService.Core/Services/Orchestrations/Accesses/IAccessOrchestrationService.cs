// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading.Tasks;

namespace LondonFhirService.Core.Services.Orchestrations.Accesses
{
    public interface IAccessOrchestrationService
    {
        public ValueTask ValidateAccess(string nhsNumber);
    }
}
