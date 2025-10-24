// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.OdsDatas.Exceptions
{
    public class NullOdsDataException : Xeption
    {
        public NullOdsDataException(string message)
            : base(message)
        { }
    }
}