// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.JsonElements.Exceptions
{
    public class JsonElementServiceException : Xeption
    {
        public JsonElementServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}