// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.OdsDatas.Exceptions
{
    public class OdsDataServiceDependencyValidationException : Xeption
    {
        public OdsDataServiceDependencyValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}