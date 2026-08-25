// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Abstractions.Models;
using LondonFhirService.Core.Abstractions.Models.Metrics;
using LondonFhirService.Core.Models.Foundations.Metrics;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Brokers.AuditAndMetrics
{
    /// <summary>
    /// The storage port accepts any IMetric, because the library that calls it holds no concrete
    /// implementation and must not need one. Storage only knows the Core entity, so anything else
    /// has to be copied onto one - a cast would throw on a call the contract says is legal.
    ///
    /// Every metric write in the application passes through this mapping, so a dropped field here
    /// is a column that silently stops being written.
    /// </summary>
    public partial class MetricBrokerTests
    {
        [Fact]
        public async Task ShouldCopyEveryMetricFieldWhenTheContractIsNotTheEntityAsync()
        {
            // given
            var foreignMetric = new ForeignMetric
            {
                Id = Guid.NewGuid(),
                ParentId = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid(),
                Method = GetRandomString(),
                Type = MetricType.Provider,
                Name = GetRandomString(),
                Target = GetRandomString(),
                Started = GetRandomDateTimeOffset(),
                Completed = GetRandomDateTimeOffset(),
                DurationMs = GetRandomNumber(),
                Status = MetricStatus.TimedOut,
                ErrorCode = GetRandomString(),
                PayloadBytes = GetRandomNumber(),
                Consumer = GetRandomString(),
                Description = GetRandomString(),
                CreatedDate = GetRandomDateTimeOffset()
            };

            Metric capturedMetric = null;

            this.storageBrokerMock.Setup(broker =>
                broker.InsertMetricAsync(It.IsAny<IMetric>(), It.IsAny<CancellationToken>()))
                    .Callback<IMetric, CancellationToken>((metric, _) => capturedMetric = metric as Metric)
                    .ReturnsAsync(foreignMetric);

            // when
            await this.metricBroker.InsertMetricAsync(
                foreignMetric, TestContext.Current.CancellationToken);

            // then
            capturedMetric.Should().NotBeNull();
            capturedMetric.Should().BeOfType<Metric>();
            capturedMetric.Id.Should().Be(foreignMetric.Id);
            capturedMetric.ParentId.Should().Be(foreignMetric.ParentId);
            capturedMetric.CorrelationId.Should().Be(foreignMetric.CorrelationId);
            capturedMetric.Method.Should().Be(foreignMetric.Method);
            capturedMetric.Type.Should().Be(foreignMetric.Type);
            capturedMetric.Name.Should().Be(foreignMetric.Name);
            capturedMetric.Target.Should().Be(foreignMetric.Target);
            capturedMetric.Started.Should().Be(foreignMetric.Started);
            capturedMetric.Completed.Should().Be(foreignMetric.Completed);
            capturedMetric.DurationMs.Should().Be(foreignMetric.DurationMs);
            capturedMetric.Status.Should().Be(foreignMetric.Status);
            capturedMetric.ErrorCode.Should().Be(foreignMetric.ErrorCode);
            capturedMetric.PayloadBytes.Should().Be(foreignMetric.PayloadBytes);
            capturedMetric.Consumer.Should().Be(foreignMetric.Consumer);
            capturedMetric.Description.Should().Be(foreignMetric.Description);
            capturedMetric.CreatedDate.Should().Be(foreignMetric.CreatedDate);
        }

        [Fact]
        public async Task ShouldPassTheSameMetricInstanceThroughWhenItIsAlreadyTheEntityAsync()
        {
            // given
            var metric = new Metric { Id = Guid.NewGuid(), Method = GetRandomString() };
            Metric capturedMetric = null;

            this.storageBrokerMock.Setup(broker =>
                broker.InsertMetricAsync(It.IsAny<IMetric>(), It.IsAny<CancellationToken>()))
                    .Callback<IMetric, CancellationToken>((inserted, _) => capturedMetric = inserted as Metric)
                    .ReturnsAsync(metric);

            // when
            await this.metricBroker.InsertMetricAsync(
                metric, TestContext.Current.CancellationToken);

            // then
            capturedMetric.Should().BeSameAs(metric);
        }

        /// <summary>
        /// An IMetric that is not Core's Metric. Exactly what the port promises to accept and
        /// what the old cast would have thrown on.
        /// </summary>
        private class ForeignMetric : IMetric, IKey
        {
            public Guid Id { get; set; }
            public Guid? ParentId { get; set; }
            public Guid CorrelationId { get; set; }
            public string Method { get; set; }
            public MetricType Type { get; set; }
            public string Name { get; set; }
            public string Target { get; set; }
            public DateTimeOffset Started { get; set; }
            public DateTimeOffset Completed { get; set; }
            public double DurationMs { get; set; }
            public MetricStatus Status { get; set; }
            public string ErrorCode { get; set; }
            public long? PayloadBytes { get; set; }
            public string Consumer { get; set; }
            public string Description { get; set; }
            public DateTimeOffset CreatedDate { get; set; }
        }
    }
}
