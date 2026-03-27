// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections;
using Xeptions;

namespace LondonFhirService.Core.Models.Processings.JsonIgnoreRules.MetaIgnoreRules.Exceptions
{
    public class FailedMetaIgnoreProcessingException : Xeption
    {
        public FailedMetaIgnoreProcessingException(string message, Exception innerException, IDictionary data)
            : base(message, innerException, data)
        { }
    }
}
