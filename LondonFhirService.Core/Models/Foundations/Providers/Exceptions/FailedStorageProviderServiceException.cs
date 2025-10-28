// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.Providers.Exceptions
{
    public class FailedStorageProviderServiceException : Xeption
    {
        public FailedStorageProviderServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}