// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.Providers.Exceptions
{
    internal class LockedProviderServiceException : Xeption
    {
        public LockedProviderServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}