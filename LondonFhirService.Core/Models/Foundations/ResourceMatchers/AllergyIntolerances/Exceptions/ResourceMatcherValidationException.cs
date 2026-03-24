// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.ResourceMatchers.AllergyIntolerances.Exceptions;

public class ResourceMatcherValidationException : Xeption
{
    public ResourceMatcherValidationException(string message, Exception innerException)
        : base(message, innerException)
    { }
}
