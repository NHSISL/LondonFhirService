// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Foundations.PdsDatas.Exceptions
{
    public class NullPdsDataServiceException : Xeption
    {
        public NullPdsDataServiceException(string message)
            : base(message)
        { }
    }
}