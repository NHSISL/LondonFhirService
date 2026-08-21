// ---
// skill: the-standard-brokers
// type: template
// source-section: "1. Brokers"
// ---

// IDateTimeBroker.cs — interface
using System;
using System.Threading.Tasks;

namespace {Namespace}.Brokers.DateTimes
{
    public interface IDateTimeBroker
    {
        ValueTask<DateTimeOffset> GetCurrentDateTimeOffsetAsync();
    }
}

// DateTimeBroker.cs — implementation
using System;
using System.Threading.Tasks;

namespace {Namespace}.Brokers.DateTimes
{
    public class DateTimeBroker : IDateTimeBroker
    {
        public async ValueTask<DateTimeOffset> GetCurrentDateTimeOffsetAsync() =>
            DateTimeOffset.UtcNow;
    }
}
