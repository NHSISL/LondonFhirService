// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Processings.ListEntryComparisons.Exceptions
{
    public class ListEntryComparisonProcessingValidationException : Xeption
    {
        public ListEntryComparisonProcessingValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
