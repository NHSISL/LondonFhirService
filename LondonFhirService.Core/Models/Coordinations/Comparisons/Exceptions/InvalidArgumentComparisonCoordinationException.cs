// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Coordinations.Comparisons.Exceptions
{
    public class InvalidArgumentComparisonCoordinationException : Xeption
    {
        public InvalidArgumentComparisonCoordinationException(string message)
            : base(message)
        { }
    }
}
