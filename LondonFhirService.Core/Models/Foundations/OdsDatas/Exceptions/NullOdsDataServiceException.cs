// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.OdsDatas.Exceptions
{
    public class NullOdsDataServiceException : Xeption
    {
        public NullOdsDataServiceException(string message)
            : base(message)
        { }
    }
}