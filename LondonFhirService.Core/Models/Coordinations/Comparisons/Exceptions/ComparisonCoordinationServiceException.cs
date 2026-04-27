// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Coordinations.Comparisons.Exceptions
{
    public class ComparisonCoordinationServiceException : Xeption
    {
        public ComparisonCoordinationServiceException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
