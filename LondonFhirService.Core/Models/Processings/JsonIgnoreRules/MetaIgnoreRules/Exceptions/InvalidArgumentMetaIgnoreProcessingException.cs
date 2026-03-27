// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Processings.JsonIgnoreRules.MetaIgnoreRules.Exceptions
{
    public class InvalidArgumentMetaIgnoreProcessingException : Xeption
    {
        public InvalidArgumentMetaIgnoreProcessingException(string message)
            : base(message)
        { }
    }
}
