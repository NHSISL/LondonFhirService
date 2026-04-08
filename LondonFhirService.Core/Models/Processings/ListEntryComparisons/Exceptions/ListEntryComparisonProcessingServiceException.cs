// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Processings.ListEntryComparisons.Exceptions
{
    public class ListEntryComparisonProcessingServiceException : Xeption
    {
        public ListEntryComparisonProcessingServiceException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
