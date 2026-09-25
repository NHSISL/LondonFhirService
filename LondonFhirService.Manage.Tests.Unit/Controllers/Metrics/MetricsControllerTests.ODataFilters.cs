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
using Microsoft.OData;
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
        /// <summary>
        /// The Status filter on the master list, named as a plain string like Type.
        /// </summary>
        [Fact]
        public void ShouldFilterRequestSpansByStatusWithAnUnqualifiedEnumLiteral()
        {
            // given
            List<Metric> metrics = CreateSpanTree();
            Metric failedRequest = metrics.First(metric => metric.Type == MetricType.Request);
            failedRequest.Status = MetricStatus.Failed;

            // when
            List<Metric> actualMetrics =
                ApplyQuery("?$filter=Type eq 'Request' and Status eq 'Failed'", metrics).ToList();

            // then
            actualMetrics.Should().ContainSingle().Which.Id.Should().Be(failedRequest.Id);
        }

        /// <summary>
        /// The master list shows each request's proxy overhead, which needs the ProviderRequests
        /// span of every request on the page. The client asks for all of them in one call with an
        /// in list of bare guid literals, rather than one call per row.
        /// </summary>
        [Fact]
        public void ShouldFilterSpansOfOneTypeAcrossSeveralCorrelations()
        {
            // given
            List<Metric> metrics = CreateSpanTree();
            Guid firstCorrelationId = metrics[0].CorrelationId;
            Guid secondCorrelationId = metrics[3].CorrelationId;
            metrics.Add(CreateMetric(Guid.NewGuid(), MetricType.Request, null, minutesAgo: 1));

            // when
            List<Metric> actualMetrics = ApplyQuery(
                "?$filter=Type eq 'Request' and CorrelationId in "
                    + $"({firstCorrelationId},{secondCorrelationId})",
                metrics)
                    .ToList();

            // then
            actualMetrics.Should().HaveCount(2);
            actualMetrics.Should().OnlyContain(metric => metric.Type == MetricType.Request);

            actualMetrics.Select(metric => metric.CorrelationId).Should()
                .BeEquivalentTo(new[] { firstCorrelationId, secondCorrelationId });
        }

        /// <summary>
        /// Why the client dashes a correlation id before it puts one in a metrics link.
        ///
        /// A FhirRecord stores its correlation as a 32 character string with no dashes, and that
        /// is the form the structured record screen and the comparisons list both hold. A Metric
        /// stores the same value as a uniqueidentifier. An OData guid literal is defined with its
        /// dashes, so the compact form reaching this filter is not a query that matches nothing -
        /// it is one the parser refuses, and the screen behind it answers 400 rather than empty.
        ///
        /// The conversion lives in the client's correlationIds helper, which is the only place
        /// that knows. This is here so that if a later OData version starts accepting the compact
        /// form - or this assumption was wrong to begin with - it is a failing test rather than a
        /// piece of folklore in a comment.
        /// </summary>
        [Fact]
        public void ShouldRefuseACorrelationIdThatHasLostItsDashes()
        {
            // given
            List<Metric> metrics = CreateSpanTree();
            string undashedCorrelationId = metrics[0].CorrelationId.ToString("N");

            // when
            Action applyUndashedFilter = () =>
                ApplyQuery($"?$filter=CorrelationId eq {undashedCorrelationId}", metrics).ToList();

            // then
            applyUndashedFilter.Should().Throw<ODataException>();

            // The dashed form of the very same value is the one that works.
            ApplyQuery($"?$filter=CorrelationId eq {metrics[0].CorrelationId}", metrics)
                .Should().HaveCount(3);
        }
    }
}
