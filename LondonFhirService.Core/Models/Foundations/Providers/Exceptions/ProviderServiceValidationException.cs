// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.Providers.Exceptions
{
    public class ProviderServiceValidationException : Xeption
    {
        public ProviderServiceValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}