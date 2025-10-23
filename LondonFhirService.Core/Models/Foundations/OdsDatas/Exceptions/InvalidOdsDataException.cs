// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.OdsDatas.Exceptions
{
    public class InvalidOdsDataException : Xeption
    {
        public InvalidOdsDataException(string message)
            : base(message)
        { }
    }
}