// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.ResourceMatchers.Exceptions;

public class ConditionMatcherServiceValidationException : Xeption
{
    public ConditionMatcherServiceValidationException(string message, Exception innerException)
        : base(message, innerException)
    { }
}