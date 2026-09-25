// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

#nullable enable annotations

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Models.Processings.Metrics;

namespace LondonFhirService.Core.Services.Processings.Metrics
{
    public interface IMetricProcessingService
    {
        /// <summary>
        /// Every root Request span matching the filter, newest first, each paired with the
        /// duration of its ProviderRequests span. Deferred: nothing is read until the result is
        /// enumerated, so a caller can stream it rather than hold every row at once.
        ///
        /// The dates bound CreatedDate, inclusive at both ends, as the portal's master list does.
        /// A user id restricts the export to the requests that one caller made.
        /// </summary>
        ValueTask<IQueryable<MetricExport>> RetrieveRequestMetricExportsAsync(
            Guid? correlationId,
            string? userId,
            DateTimeOffset? fromDate,
            DateTimeOffset? toDate,
            CancellationToken cancellationToken = default);
    }
}
