// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.Providers.Exceptions
{
    public class ProviderServiceDependencyException : Xeption
    {
        public ProviderServiceDependencyException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}