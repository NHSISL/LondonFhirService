// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LondonFhirService.Manage.Tests.Acceptance.Brokers;
using LondonFhirService.Core.Abstractions.Models.Metrics;
using LondonFhirService.Manage.Tests.Acceptance.Models.Metrics;
using Tynamix.ObjectFiller;

namespace LondonFhirService.Manage.Tests.Acceptance.Apis.Metrics
{
    /// <summary>
    /// The metric counterpart to AuditApiTests. Reads are the surface; create and delete carry
    /// [InvisibleApi] and exist so this suite can seed a span and clear it again.
    ///
    /// There is no PUT to cover - a metric is a span of work that already happened, so the
    /// controller offers no update.
    /// </summary>
    [Collection(nameof(ApiTestCollection))]
    public partial class MetricApiTests
    {
        private readonly ApiBroker apiBroker;

        public MetricApiTests(ApiBroker apiBroker) =>
            this.apiBroker = apiBroker;

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static string GetRandomStringWithLengthOf(int length)
        {
            string result = new MnemonicString(wordCount: 1, wordMinLength: length, wordMaxLength: length).GetValue();

            return result.Length > length ? result.Substring(0, length) : result;
        }

        private async ValueTask<Metric> PostRandomMetricAsync()
        {
            Metric randomMetric = CreateRandomMetric();

            return await this.apiBroker.PostMetricAsync(randomMetric);
        }

        private async ValueTask<List<Metric>> PostRandomMetricsAsync()
        {
            int randomNumber = GetRandomNumber();
            var randomMetrics = new List<Metric>();

            for (int i = 0; i < randomNumber; i++)
            {
                randomMetrics.Add(await PostRandomMetricAsync());
            }

            return randomMetrics;
        }

        private static Metric CreateRandomMetric() =>
            CreateRandomMetricFiller().Create();

        /// <summary>
        /// Type and Status are pinned to real enum values. The host registers no
        /// JsonStringEnumConverter, so these travel as ordinals - the earlier string form failed
        /// model binding with a 400 before the controller ever ran.
        ///
        /// ParentId is left null: it points at another span, and a random Guid would name one
        /// that does not exist.
        ///
        /// DurationMs and PayloadBytes are pinned non-negative. The library rejects negative
        /// values on both, and an unpinned filler hands out random doubles and longs - which made
        /// this suite fail roughly half the time rather than never.
        /// </summary>
        private static Filler<Metric> CreateRandomMetricFiller()
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            var filler = new Filler<Metric>();

            filler.Setup()
                .OnType<DateTimeOffset>().Use(now)
                .OnType<DateTimeOffset?>().Use(now)
                .OnProperty(metric => metric.ParentId).IgnoreIt()
                .OnProperty(metric => metric.Method).Use(GetRandomStringWithLengthOf(255))
                .OnProperty(metric => metric.Name).Use(GetRandomStringWithLengthOf(255))
                .OnProperty(metric => metric.Target).Use(GetRandomStringWithLengthOf(255))
                .OnProperty(metric => metric.Consumer).Use(GetRandomStringWithLengthOf(255))
                .OnProperty(metric => metric.ErrorCode).Use(GetRandomStringWithLengthOf(100))
                .OnProperty(metric => metric.Description).Use(GetRandomStringWithLengthOf(1000))
                .OnProperty(metric => metric.Type).Use(MetricType.Provider)
                .OnProperty(metric => metric.Status).Use(MetricStatus.Succeeded)
                .OnProperty(metric => metric.DurationMs).Use((double)GetRandomNumber())
                .OnProperty(metric => metric.PayloadBytes).Use((long?)GetRandomNumber())
                .OnProperty(metric => metric.Started).Use(now)
                .OnProperty(metric => metric.Completed).Use(now)
                .OnProperty(metric => metric.CreatedDate).Use(now);

            return filler;
        }
    }
}
