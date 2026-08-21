// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LondonFhirService.Api.Workers
{
    /// <summary>
    /// Forwards the metric library's spans into Application Insights.
    ///
    /// The library publishes through an ActivitySource so it carries no telemetry vendor of its
    /// own. Nothing was listening: the classic Application Insights SDK this application uses
    /// tracks requests and dependencies through its own collectors and does not subscribe to
    /// arbitrary ActivitySources, so StartActivity returned null every time and every span was
    /// dropped. This registers the listener that makes them real.
    ///
    /// An ActivityListener rather than the OpenTelemetry Azure Monitor distro on purpose: the
    /// distro would run a second telemetry pipeline alongside the existing SDK and double-report
    /// requests and dependencies. This subscribes to one source and leaves everything else alone.
    /// </summary>
    public class MetricTelemetryPublisher : BackgroundService
    {
        private readonly TelemetryClient telemetryClient;
        private readonly ILogger<MetricTelemetryPublisher> logger;
        private readonly string activitySourceName;
        private ActivityListener activityListener;

        public MetricTelemetryPublisher(
            TelemetryClient telemetryClient,
            ILogger<MetricTelemetryPublisher> logger,
            string activitySourceName)
        {
            this.telemetryClient = telemetryClient;
            this.logger = logger;
            this.activitySourceName = activitySourceName;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Application Insights is switched off in some hosts - the acceptance runs exclude it
            // deliberately. Without a client there is nothing to forward to, and a hosted service
            // that throws while starting takes the whole host down with it.
            if (this.telemetryClient is null)
            {
                this.logger.LogInformation(
                    "MetricTelemetryPublisher idle: no telemetry client is registered. "
                        + "Spans are still persisted to the metrics table.");

                return Task.CompletedTask;
            }

            this.activityListener = new ActivityListener
            {
                ShouldListenTo = source => source.Name == this.activitySourceName,

                // AllDataAndRecorded, because the span has already happened and been persisted by
                // the time it is replayed here. Sampling it away would leave the metrics table and
                // the telemetry disagreeing about what ran.
                Sample = (ref ActivityCreationOptions<ActivityContext> _) =>
                    ActivitySamplingResult.AllDataAndRecorded,

                ActivityStopped = PublishActivity
            };

            ActivitySource.AddActivityListener(this.activityListener);

            this.logger.LogInformation(
                "MetricTelemetryPublisher listening to ActivitySource {ActivitySourceName}.",
                this.activitySourceName);

            return Task.CompletedTask;
        }

        private void PublishActivity(Activity activity)
        {
            try
            {
                var dependencyTelemetry = new DependencyTelemetry
                {
                    Name = activity.DisplayName,
                    Type = activity.GetTagItem("metric.type")?.ToString() ?? "Metric",
                    Target = activity.GetTagItem("metric.target")?.ToString(),
                    Duration = activity.Duration,
                    Timestamp = activity.StartTimeUtc,
                    Success = activity.Status != ActivityStatusCode.Error,
                    ResultCode = activity.GetTagItem("metric.errorCode")?.ToString(),
                    Id = activity.SpanId.ToHexString(),
                };

                foreach (KeyValuePair<string, string> tag in activity.Tags)
                {
                    if (tag.Value is not null)
                    {
                        dependencyTelemetry.Properties[tag.Key] = tag.Value;
                    }
                }

                // Correlation comes from Activity.Current, not from the telemetry item: version 3
                // of the SDK removed OperationContext.Id in favour of the W3C trace context its
                // initializers read off the ambient activity. The span being reported has already
                // stopped, so it is made current for the duration of the call - without this every
                // span lands as its own operation instead of under the request it belongs to.
                Activity previousActivity = Activity.Current;

                try
                {
                    Activity.Current = activity;
                    this.telemetryClient.TrackDependency(dependencyTelemetry);
                }
                finally
                {
                    Activity.Current = previousActivity;
                }
            }
            catch (Exception exception)
            {
                // Telemetry must never take the process down. The span is already persisted in
                // the metrics table, which stays the authoritative record.
                this.logger.LogError(exception, "Failed to publish a metric span to telemetry.");
            }
        }

        public override void Dispose()
        {
            this.activityListener?.Dispose();
            base.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
