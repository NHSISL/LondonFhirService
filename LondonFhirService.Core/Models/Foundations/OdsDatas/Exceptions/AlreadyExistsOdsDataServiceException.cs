// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections;
using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.OdsDatas.Exceptions
{
    public class AlreadyExistsOdsDataServiceException : Xeption
    {
        public AlreadyExistsOdsDataServiceException(string message, Exception innerException, IDictionary data)
            : base(message, innerException, data)
        { }
    }
}