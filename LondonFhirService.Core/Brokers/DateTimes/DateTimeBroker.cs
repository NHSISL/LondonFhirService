// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;

namespace LondonFhirService.Core.Brokers.DateTimes
{
    public class DateTimeBroker : IDateTimeBroker
    {
        public async ValueTask<DateTimeOffset> GetCurrentDateTimeOffsetAsync() =>
            DateTimeOffset.UtcNow;
    }
}