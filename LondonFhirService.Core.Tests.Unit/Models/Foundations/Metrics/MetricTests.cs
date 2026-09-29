// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using FluentAssertions;
using LondonFhirService.Core.Abstractions.Models.Metrics;
using LondonFhirService.Core.Models.Foundations.Metrics;

namespace LondonFhirService.Core.Tests.Unit.Models.Foundations.Metrics
{
    /// <summary>
    /// Metric adapts this host's span vocabulary to the library's contract. The library reads a
    /// span type only as text and takes the telemetry kind as given, so the classification of
    /// which spans are the incoming request and which are calls out now lives here - and is only
    /// ever exercised through the contract, which is how the library sees it.
    /// </summary>
    public class MetricTests
    {
        [Theory]
        [InlineData(MetricType.Request, MetricSpanKind.Server)]
        [InlineData(MetricType.Orchestration, MetricSpanKind.Internal)]
        [InlineData(MetricType.AccessCheck, MetricSpanKind.Client)]
        [InlineData(MetricType.ProviderRequests, MetricSpanKind.Internal)]
        [InlineData(MetricType.ProviderDiscovery, MetricSpanKind.Internal)]
        [InlineData(MetricType.Foundation, MetricSpanKind.Internal)]
        [InlineData(MetricType.ProviderFanOut, MetricSpanKind.Internal)]
        [InlineData(MetricType.Provider, MetricSpanKind.Client)]
        [InlineData(MetricType.ProviderCall, MetricSpanKind.Client)]
        [InlineData(MetricType.Persist, MetricSpanKind.Client)]
        [InlineData(MetricType.Consolidation, MetricSpanKind.Internal)]
        public void ShouldClassifyEachSpanTypeForTelemetry(
            MetricType metricType,
            MetricSpanKind expectedSpanKind)
        {
            // given
            IMetric metric = new Metric { Type = metricType };

            // when
            MetricSpanKind actualSpanKind = metric.SpanKind;

            // then
            actualSpanKind.Should().Be(expectedSpanKind);
        }

        [Fact]
        public void ShouldClassifyEverySpanType()
        {
            // given
            // Guards the theory above: a member added to MetricType without a row there would
            // quietly replay as Internal, whatever kind of work it actually measures.
            int classifiedSpanTypeCount = 11;

            // when
            int actualSpanTypeCount = Enum.GetValues<MetricType>().Length;

            // then
            actualSpanTypeCount.Should().Be(classifiedSpanTypeCount);
        }

        [Fact]
        public void ShouldExposeTheSpanTypeToTheContractAsItsName()
        {
            // given
            MetricType expectedMetricType = MetricType.Persist;
            IMetric metric = new Metric { Type = expectedMetricType };

            // when
            string actualMetricType = metric.Type;

            // then
            actualMetricType.Should().Be(expectedMetricType.ToString());
        }
    }
}
