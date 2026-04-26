// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Coordinations.Comparisons.Exceptions
{
    public class ComparisonCoordinationDependencyException : Xeption
    {
        public ComparisonCoordinationDependencyException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
