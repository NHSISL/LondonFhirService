// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using LondonFhirService.Core.Abstractions.Models.Metrics;
using LondonFhirService.Core.Models.Foundations.Metrics;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace LondonFhirService.Manage.Tests.Unit.Controllers.Metrics
{
    /// <summary>
    /// The management client drives the metrics master list entirely from OData query options, so
    /// the exact syntax it sends has to be one this host will parse. These exercise that syntax
    /// against the same query pipeline [EnableQuery] uses, rather than leaving it to be found out
    /// as a 400 in the browser.
    ///
    /// Type is the interesting one: the host registers no JsonStringEnumConverter, so the value
    /// travels as an ordinal, and it was not obvious without checking whether a filter could name
    /// the member as a plain string.
    /// </summary>
    public class MetricsControllerTestsODataFilters
    {
        private static readonly IEdmModel EdmModel = BuildEdmModel();

        private static IEdmModel BuildEdmModel()
        {
            ODataConventionModelBuilder builder = new();
            builder.EntitySet<Metric>("Metrics");

            return builder.GetEdmModel();
        }

        private static List<Metric> CreateSpanTree()
        {
            Guid firstCorrelationId = Guid.NewGuid();
            Guid secondCorrelationId = Guid.NewGuid();

            return new List<Metric>
            {
                CreateMetric(firstCorrelationId, MetricType.Request, null, minutesAgo: 10),
                CreateMetric(firstCorrelationId, MetricType.Orchestration, Guid.NewGuid(), minutesAgo: 10),
                CreateMetric(firstCorrelationId, MetricType.Provider, Guid.NewGuid(), minutesAgo: 10),
                CreateMetric(secondCorrelationId, MetricType.Request, null, minutesAgo: 5),
                CreateMetric(secondCorrelationId, MetricType.Consolidation, Guid.NewGuid(), minutesAgo: 5)
            };
        }

        private static Metric CreateMetric(
            Guid correlationId,
            MetricType metricType,
            Guid? parentId,
            int minutesAgo)
        {
            DateTimeOffset started = DateTimeOffset.UtcNow.AddMinutes(-minutesAgo);

            return new Metric
            {
                Id = Guid.NewGuid(),
                CorrelationId = correlationId,
                ParentId = parentId,
                Type = metricType,
                Method = "STU3-Patient-GetStructuredRecord",
                Name = "name",
                Started = started,
                Completed = started.AddMilliseconds(100),
                DurationMs = 100,
                Status = MetricStatus.Succeeded,
                CreatedDate = started
            };
        }

        private static IQueryable<Metric> ApplyQuery(string queryString, IEnumerable<Metric> metrics)
        {
            ODataQueryContext queryContext = new(EdmModel, typeof(Metric), path: null);

            Microsoft.AspNetCore.Http.DefaultHttpContext httpContext = new();
            httpContext.Request.QueryString = new Microsoft.AspNetCore.Http.QueryString(queryString);

            ODataQueryOptions<Metric> queryOptions = new(queryContext, httpContext.Request);

            return queryOptions.ApplyTo(metrics.AsQueryable()).Cast<Metric>();
        }

        [Fact]
        public void ShouldFilterToRequestSpansWithAnUnqualifiedEnumLiteral()
        {
            // given
            List<Metric> metrics = CreateSpanTree();

            // when
            List<Metric> actualMetrics =
                ApplyQuery("?$filter=Type eq 'Request'", metrics).ToList();

            // then
            actualMetrics.Should().HaveCount(2);
            actualMetrics.Should().OnlyContain(metric => metric.Type == MetricType.Request);
        }

        [Fact]
        public void ShouldOrderRequestSpansNewestFirst()
        {
            // given
            List<Metric> metrics = CreateSpanTree();

            // when
            List<Metric> actualMetrics =
                ApplyQuery("?$filter=Type eq 'Request'&$orderby=Started desc", metrics).ToList();

            // then
            actualMetrics.Should().HaveCount(2);

            actualMetrics[0].Started.Should()
                .BeAfter(actualMetrics[1].Started);
        }

        [Fact]
        public void ShouldPageRequestSpans()
        {
            // given
            List<Metric> metrics = CreateSpanTree();

            // when
            List<Metric> actualMetrics =
                ApplyQuery("?$filter=Type eq 'Request'&$orderby=Started desc&$skip=1&$top=1", metrics)
                    .ToList();

            // then
            actualMetrics.Should().HaveCount(1);
        }

        [Fact]
        public void ShouldFilterEverySpanOfOneCorrelation()
        {
            // given
            List<Metric> metrics = CreateSpanTree();
            Guid correlationId = metrics[0].CorrelationId;

            // when
            List<Metric> actualMetrics = ApplyQuery(
                $"?$filter=CorrelationId eq {correlationId}&$orderby=Started asc",
                metrics)
                    .ToList();

            // then
            actualMetrics.Should().HaveCount(3);
            actualMetrics.Should().OnlyContain(metric => metric.CorrelationId == correlationId);
        }
    }
}
