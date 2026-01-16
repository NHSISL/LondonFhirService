// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

namespace LondonFhirService.Core.Models.Orchestrations.Accesses
{
    public class AccessConfigurations
    {
        public bool UseHashedNhsNumber { get; set; } = true;
        public string HashPepper { get; set; }
    }
}
