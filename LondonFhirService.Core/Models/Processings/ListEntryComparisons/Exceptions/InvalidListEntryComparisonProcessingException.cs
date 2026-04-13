// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Processings.ListEntryComparisons.Exceptions
{
    public class InvalidListEntryComparisonProcessingException : Xeption
    {
        public InvalidListEntryComparisonProcessingException(string message)
            : base(message)
        { }
    }
}
