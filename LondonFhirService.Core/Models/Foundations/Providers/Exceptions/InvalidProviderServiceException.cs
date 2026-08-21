// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.Providers.Exceptions
{
    internal class InvalidProviderServiceException : Xeption
    {
        public InvalidProviderServiceException(string message)
            : base(message)
        { }
    }
}
