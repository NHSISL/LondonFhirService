// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections;
using Xeptions;

namespace LondonFhirService.Core.Models.Processings.ResourceMatchings.Exceptions
{
    public class FailedResourceMatcherProcessingException : Xeption
    {
        public FailedResourceMatcherProcessingException(
            string message,
            Exception innerException,
            IDictionary data)
            : base(message, innerException, data)
        { }
    }
}
