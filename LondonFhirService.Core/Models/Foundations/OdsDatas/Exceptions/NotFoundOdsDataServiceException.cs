// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.OdsDatas.Exceptions
{
    public class NotFoundOdsDataServiceException : Xeption
    {
        public NotFoundOdsDataServiceException(string message)
            : base(message)
        { }
    }
}