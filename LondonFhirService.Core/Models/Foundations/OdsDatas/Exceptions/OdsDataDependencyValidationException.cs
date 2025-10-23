// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.OdsDatas.Exceptions
{
    public class OdsDataDependencyValidationException : Xeption
    {
        public OdsDataDependencyValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}