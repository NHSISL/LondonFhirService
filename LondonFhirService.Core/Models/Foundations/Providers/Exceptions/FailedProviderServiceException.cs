// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.Providers.Exceptions
{
    public class FailedProviderServiceException : Xeption
    {
        public FailedProviderServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}