// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.Providers.Exceptions
{
    public class ProviderServiceDependencyValidationException : Xeption
    {
        public ProviderServiceDependencyValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}