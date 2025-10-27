// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.PdsDatas.Exceptions
{
    public class PdsDataServiceDependencyException : Xeption
    {
        public PdsDataServiceDependencyException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}