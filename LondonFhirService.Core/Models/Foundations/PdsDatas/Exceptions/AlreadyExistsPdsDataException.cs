// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections;
using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.PdsDatas.Exceptions
{
    public class AlreadyExistsPdsDataException : Xeption
    {
        public AlreadyExistsPdsDataException(string message, Exception innerException, IDictionary data)
            : base(message, innerException, data)
        { }
    }
}