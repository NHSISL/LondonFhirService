// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.PdsDatas.Exceptions
{
    public class PdsDataServiceDependencyValidationException : Xeption
    {
        public PdsDataServiceDependencyValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}