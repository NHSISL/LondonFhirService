// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections;
using Xeptions;

namespace LondonFhirService.Core.Models.Processings.ListEntryComparisons.Exceptions
{
    public class FailedListEntryComparisonProcessingException : Xeption
    {
        public FailedListEntryComparisonProcessingException(
            string message,
            Exception innerException,
            IDictionary data)
            : base(message, innerException, data)
        { }
    }
}
