// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.Providers.Exceptions
{
    public class ProviderServiceException : Xeption
    {
        public ProviderServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}