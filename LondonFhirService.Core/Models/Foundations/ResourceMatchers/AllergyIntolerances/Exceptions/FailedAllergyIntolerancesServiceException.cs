// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.ResourceMatchers.AllergyIntolerances.Exceptions;

public class FailedAllergyIntolerancesServiceException : Xeption
{
    public FailedAllergyIntolerancesServiceException(string message, Exception innerException)
        : base(message, innerException)
    { }
}
