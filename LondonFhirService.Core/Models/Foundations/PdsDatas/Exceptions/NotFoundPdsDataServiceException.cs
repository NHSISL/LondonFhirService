// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.PdsDatas.Exceptions
{
    public class NotFoundPdsDataServiceException : Xeption
    {
        public NotFoundPdsDataServiceException(string message)
            : base(message)
        { }
    }
}