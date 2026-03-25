// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.ResourceMatchers.Exceptions;

public class FailedListMatcherServiceException : Xeption
{
    public FailedListMatcherServiceException(string message, Exception innerException)
        : base(message, innerException)
    { }
}