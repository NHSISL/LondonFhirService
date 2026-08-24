// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using LondonFhirService.Core.Abstractions.Models.Metrics;
using LondonFhirService.Core.Models.Foundations.Metrics;
using Moq;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Core.Tests.Unit.Services.Coordinations.Patients.STU3
{
    /// <summary>
    /// How the root Request span classifies a failure. Every abort used to land in Failed, which
    /// put a client that hung up and a genuine fault in the same bucket - and MetricStatus exists
    /// precisely so durations are only ever compared within one status. A 130-second timeout
    /// averaged against a fast validation failure tells nobody anything.
    ///
    /// The nested cases are the realistic ones: by the time an abort reaches the coordination
    /// layer it has already been localised into a dependency exception, so the cause survives only
    /// in the inner chain.
    /// </summary>
    public partial class Stu3PatientCoordinationServiceTests
    {
        [Theory]
        [MemberData(nameof(FailureClassifications))]
        public async Task ShouldClassifyTheRootRequestSpanByTheCauseOfTheFailureAsync(
            Exception thrownException,
            MetricStatus expectedStatus)
        {
            // given
            string inputNhsNumber = GetRandomString();
            CancellationToken cancellationToken = CancellationToken.None;
            Guid correlationId = Guid.NewGuid();
            Guid requestSpanId = Guid.NewGuid();
            var recordedMetrics = new List<Metric>();

            this.identifierBrokerMock.SetupSequence(broker => broker.GetIdentifierAsync())
                .ReturnsAsync(correlationId)
                .ReturnsAsync(requestSpanId);

            this.patientOrchestrationServiceMock.Setup(service =>
                service.GetStructuredRecordSerialisedAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<CancellationToken>()))
                        .ThrowsAsync(thrownException);

            this.auditAndMetricBrokerMock.Setup(broker =>
                broker.LogMetricAsync(It.IsAny<Metric>(), It.IsAny<CancellationToken>()))
                    .Callback<Metric, CancellationToken>((metric, _) => recordedMetrics.Add(metric));

            // when
            Func<Task> getStructuredRecord = async () =>
                await this.patientCoordinationService.GetStructuredRecordSerialisedAsync(
                    inputNhsNumber,
                    cancellationToken: cancellationToken);

            await getStructuredRecord.Should().ThrowAsync<Exception>();

            // then
            Metric requestSpan = recordedMetrics.Should().ContainSingle(metric =>
                metric.Type == MetricType.Request).Subject;

            requestSpan.Id.Should().Be(requestSpanId);
            requestSpan.ParentId.Should().BeNull();
            requestSpan.Status.Should().Be(expectedStatus);
            requestSpan.ErrorCode.Should().NotBeNullOrWhiteSpace();
        }

        public static TheoryData<Exception, MetricStatus> FailureClassifications()
        {
            var operationCanceledException = new OperationCanceledException();
            var timeoutException = new TimeoutException();

            return new TheoryData<Exception, MetricStatus>
            {
                // Thrown directly.
                { operationCanceledException, MetricStatus.Cancelled },
                { timeoutException, MetricStatus.TimedOut },

                // Already localised by a lower layer, so the cause is only in the inner chain -
                // this is what actually reaches the coordination service in production.
                {
                    new InvalidOperationException("localised", operationCanceledException),
                    MetricStatus.Cancelled
                },
                {
                    new InvalidOperationException("localised", timeoutException),
                    MetricStatus.TimedOut
                },

                // Buried two levels down, which is the shape a provider timeout arrives in.
                {
                    new InvalidOperationException(
                        "outer",
                        new InvalidOperationException("inner", timeoutException)),
                    MetricStatus.TimedOut
                },

                // An ordinary fault stays Failed.
                { new InvalidOperationException("something broke"), MetricStatus.Failed }
            };
        }
    }
}
