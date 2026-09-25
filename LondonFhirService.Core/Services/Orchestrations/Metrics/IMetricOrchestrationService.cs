// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

#nullable enable annotations

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Abstractions.Models.Metrics;

namespace LondonFhirService.Core.Services.Orchestrations.Metrics
{
    public interface IMetricOrchestrationService
    {
        /// <summary>
        /// Every request matching the filter as CSV - not a page of them - with its provider
        /// requests and proxy overhead alongside. The stream is positioned at the start, ready to
        /// be returned to a caller.
        /// </summary>
        ValueTask<Stream> ExportRequestMetricsToCsvAsync(
            Guid? correlationId,
            string? userId,
            MetricStatus? status,
            DateTimeOffset? fromDate,
            DateTimeOffset? toDate,
            CancellationToken cancellationToken = default);
    }
}
