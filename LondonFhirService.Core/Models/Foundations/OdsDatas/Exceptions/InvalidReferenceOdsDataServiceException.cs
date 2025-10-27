// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.OdsDatas.Exceptions
{
    public class InvalidReferenceOdsDataServiceException : Xeption
    {
        public InvalidReferenceOdsDataServiceException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}