// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.ResourceMatchers.Exceptions;

public class InvalidListMatcherServiceException : Xeption
{
    public InvalidListMatcherServiceException()
        : base(message: "Invalid List matcher service arguments. Please correct the errors and try again.")
    { }
}