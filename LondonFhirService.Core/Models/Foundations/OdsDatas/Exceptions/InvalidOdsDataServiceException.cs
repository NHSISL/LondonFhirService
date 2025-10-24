// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.OdsDatas.Exceptions
{
    public class InvalidOdsDataServiceException : Xeption
    {
        public InvalidOdsDataServiceException(string message)
            : base(message)
        { }
    }
}