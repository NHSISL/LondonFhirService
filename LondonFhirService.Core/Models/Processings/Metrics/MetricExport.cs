// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Globalization;
using LondonFhirService.Core.Abstractions.Models.Metrics;

namespace LondonFhirService.Core.Models.Processings.Metrics
{
    /// <summary>
    /// One measured request as the metrics export writes it: the root Request span, with the
    /// duration of its ProviderRequests span alongside so the proxy overhead can be read off the
    /// row. The same figures the management portal's master list and detail view show.
    ///
    /// Like the Metric it is projected from, this must never carry patient identifiable data.
    /// </summary>
    public class MetricExport
    {
        public DateTimeOffset Started { get; set; }

        /// <summary>
        /// Started as a UTC timestamp, which is what the CSV carries. The CSV writer formats with
        /// the invariant culture, and a DateTimeOffset written that way is month first -
        /// ambiguous to anyone reading it in the UK.
        /// </summary>
        public string StartedUtc =>
            Started.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);

        public Guid CorrelationId { get; set; }
        public string Method { get; set; }
        public string Name { get; set; }
        public MetricStatus Status { get; set; }
        public string ErrorCode { get; set; }
        public double DurationMs { get; set; }

        /// <summary>
        /// Null when the request never reached its providers - a failed access check, say - so
        /// there was no ProviderRequests span to read.
        /// </summary>
        public double? ProviderRequestsMs { get; set; }

        /// <summary>
        /// The request less its provider requests: the access check, consolidation and the
        /// proxy's own work. Null rather than zero when provider requests are unknown, and clamped
        /// at zero because the two spans are timed by separate stopwatches and the child can
        /// round to a hair longer than its parent. Rounded to four decimal places - the precision
        /// the durations themselves are recorded to - so a subtraction does not write binary
        /// floating point noise such as 0.310299999999188 into the file.
        /// </summary>
        public double? ProxyOverheadMs =>
            ProviderRequestsMs is null
                ? null
                : Math.Round(Math.Max(DurationMs - ProviderRequestsMs.Value, 0), 4);

        public string Consumer { get; set; }
        public string UserId { get; set; }
    }
}
