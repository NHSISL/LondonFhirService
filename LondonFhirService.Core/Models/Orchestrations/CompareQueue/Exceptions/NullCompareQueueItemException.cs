// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Core.Models.Orchestrations.CompareQueue.Exceptions
{
    public class NullCompareQueueItemException : Xeption
    {
        public NullCompareQueueItemException(string message)
            : base(message)
        { }
    }
}
