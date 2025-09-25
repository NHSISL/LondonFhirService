// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;

namespace LondonFhirService.Core.Brokers.DateTimes
{
    public interface IDateTimeBroker
    {
        ValueTask<DateTimeOffset> GetCurrentDateTimeOffsetAsync();
    }
}