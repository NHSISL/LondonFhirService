// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.Providers.Exceptions
{
    public class NotFoundProviderServiceException : Xeption
    {
        public NotFoundProviderServiceException(string message)
            : base(message)
        { }
    }
}