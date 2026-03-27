// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.ResourceMatchers.Conditions.Exceptions
{
    public class FailedConditionMatcherServiceException : Xeption
    {
        public FailedConditionMatcherServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
