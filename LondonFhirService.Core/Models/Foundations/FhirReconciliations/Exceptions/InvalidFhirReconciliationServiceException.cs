// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.FhirReconciliations.Exceptions
{
    public class ResourceNotFoundException : Xeption
    {
        public ResourceNotFoundException(string message)
            : base(message)
        { }
    }
}