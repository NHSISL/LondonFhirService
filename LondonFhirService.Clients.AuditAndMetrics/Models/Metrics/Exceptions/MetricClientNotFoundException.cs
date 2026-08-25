// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Clients.AuditAndMetrics.Models.Metrics.Exceptions
{
    /// <summary>
    /// The requested metric does not exist. Distinct from a validation failure so a caller can
    /// answer 404 rather than 400 without naming the library's internal categorization types.
    ///
    /// Mirrors AuditClientNotFoundException. Without it a missing span reached callers folded
    /// into MetricClientValidationException, whose inner NotFoundMetricException is internal to
    /// this library - so a consumer had no way to tell absent from malformed.
    /// </summary>
    public class MetricClientNotFoundException : Xeption
    {
        public MetricClientNotFoundException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
