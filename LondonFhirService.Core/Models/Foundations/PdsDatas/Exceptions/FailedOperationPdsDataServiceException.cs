// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.PdsDatas.Exceptions
{
    public class FailedOperationPdsDataServiceException : Xeption
    {
        public FailedOperationPdsDataServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
