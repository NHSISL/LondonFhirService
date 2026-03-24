// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.ResourceMatchers.Exceptions;

public class InvalidConditionMatcherServiceException : Xeption
{
    public InvalidConditionMatcherServiceException()
        : base(message: "Invalid Condition matcher service arguments. Please correct the errors and try again.")
    { }
}