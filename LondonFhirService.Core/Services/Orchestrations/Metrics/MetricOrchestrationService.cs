// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

#nullable enable annotations

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Core.Brokers.CsvHelpers;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Models.Processings.Metrics;
using LondonFhirService.Core.Services.Processings.Metrics;

namespace LondonFhirService.Core.Services.Orchestrations.Metrics
{
    internal partial class MetricOrchestrationService : IMetricOrchestrationService
    {
        private readonly IMetricProcessingService metricProcessingService;
        private readonly ICsvHelperBroker csvHelperBroker;
        private readonly ILoggingBroker loggingBroker;

        public MetricOrchestrationService(
            IMetricProcessingService metricProcessingService,
            ICsvHelperBroker csvHelperBroker,
            ILoggingBroker loggingBroker)
        {
            this.metricProcessingService = metricProcessingService;
            this.csvHelperBroker = csvHelperBroker;
            this.loggingBroker = loggingBroker;
        }

        /// <summary>
        /// The columns, in order. Named explicitly rather than left to the writer, so the file
        /// does not change shape when a property is added to MetricExport, and so Started goes out
        /// as its unambiguous UTC text rather than a culture formatted DateTimeOffset.
        /// </summary>
        internal static readonly Dictionary<string, int> ExportFieldMappings = new()
        {
            { nameof(MetricExport.StartedUtc), 0 },
            { nameof(MetricExport.CorrelationId), 1 },
            { nameof(MetricExport.Method), 2 },
            { nameof(MetricExport.Name), 3 },
            { nameof(MetricExport.Status), 4 },
            { nameof(MetricExport.ErrorCode), 5 },
            { nameof(MetricExport.DurationMs), 6 },
            { nameof(MetricExport.ProviderRequestsMs), 7 },
            { nameof(MetricExport.ProxyOverheadMs), 8 },
            { nameof(MetricExport.Consumer), 9 },
            { nameof(MetricExport.UserId), 10 }
        };

        public ValueTask<Stream> ExportRequestMetricsToCsvAsync(
            Guid? correlationId,
            string? userId,
            DateTimeOffset? fromDate,
            DateTimeOffset? toDate,
            CancellationToken cancellationToken = default) =>
        TryCatch(async () =>
        {
            IQueryable<MetricExport> metricExports =
                await this.metricProcessingService.RetrieveRequestMetricExportsAsync(
                    correlationId,
                    userId,
                    fromDate,
                    toDate,
                    cancellationToken);

            // Read asynchronously when the query supports it, as the database backed one does, so
            // a wide export does not park a thread-pool worker in reader I/O for its duration.
            IAsyncEnumerable<MetricExport> metricExportRows =
                metricExports as IAsyncEnumerable<MetricExport> ?? metricExports.ToAsyncEnumerable();

            var csvStream = new MemoryStream();

            await this.csvHelperBroker.MapObjectToCsvAsync(
                @object: metricExportRows,
                outputStream: csvStream,
                addHeaderRecord: true,
                fieldMappings: ExportFieldMappings,
                shouldAddTrailingComma: false,
                cancellationToken: cancellationToken);

            csvStream.Position = 0;

            return (Stream)csvStream;
        });
    }
}
