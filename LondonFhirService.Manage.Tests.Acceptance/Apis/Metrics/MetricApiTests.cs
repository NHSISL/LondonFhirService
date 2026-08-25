// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LondonFhirService.Manage.Tests.Acceptance.Brokers;
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
        /// Type and Status are pinned to real enum names. The broker stores both as text and EF
        /// converts on the way back, so a random string would round trip out of the database as a
        /// value Core cannot parse.
        ///
        /// ParentId is left null: it points at another span, and a random Guid would name one
        /// that does not exist.
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
                .OnProperty(metric => metric.Type).Use("Provider")
                .OnProperty(metric => metric.Status).Use("Succeeded")
                .OnProperty(metric => metric.Started).Use(now)
                .OnProperty(metric => metric.Completed).Use(now)
                .OnProperty(metric => metric.CreatedDate).Use(now);

            return filler;
        }
    }
}
