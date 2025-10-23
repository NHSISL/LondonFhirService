// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections;
using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.OdsDatas.Exceptions
{
    public class AlreadyExistsOdsDataException : Xeption
    {
        public AlreadyExistsOdsDataException(string message, Exception innerException, IDictionary data)
            : base(message, innerException, data)
        { }
    }
}