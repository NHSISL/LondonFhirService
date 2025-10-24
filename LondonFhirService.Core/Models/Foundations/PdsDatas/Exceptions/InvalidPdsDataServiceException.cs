// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.PdsDatas.Exceptions
{
    public class InvalidPdsDataServiceException : Xeption
    {
        public InvalidPdsDataServiceException(string message)
            : base(message)
        { }
    }
}