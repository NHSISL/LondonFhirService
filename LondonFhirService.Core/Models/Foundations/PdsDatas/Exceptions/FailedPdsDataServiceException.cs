// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.PdsDatas.Exceptions
{
    public class FailedPdsDataServiceException : Xeption
    {
        public FailedPdsDataServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}