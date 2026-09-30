// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Core.Tests.Unit.Services.Coordinations.Patients.STU3
{
    /// <summary>
    /// Cancellation is never wrapped. A caller that cancels gets back the cancellation exception
    /// itself, and nothing is logged as an error, because nothing went wrong. The root span still
    /// records the abort as Cancelled - that is covered by the span classification tests.
    /// </summary>
    public partial class Stu3PatientCoordinationServiceTests
    {
        [Theory]
        [MemberData(nameof(CancellationExceptions))]
        public async Task ShouldPassCancellationThroughOnGetStructuredRecordSerialisedAsync(
            Exception cancellationException)
        {
            // given
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;
            Guid inputCorrelationId = Guid.NewGuid();

            this.patientOrchestrationServiceMock.Setup(service =>
                service.GetStructuredRecordSerialisedAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<CancellationToken>()))
                        .ThrowsAsync(cancellationException);

            // when
            ValueTask<string> getStructuredRecordSerialisedTask =
                this.patientCoordinationService.GetStructuredRecordSerialisedAsync(
                    correlationId: inputCorrelationId,
                    nhsNumber: inputNhsNumber,
                    cancellationToken: TestContext.Current.CancellationToken);

            OperationCanceledException actualOperationCanceledException =
                await Assert.ThrowsAnyAsync<OperationCanceledException>(
                    getStructuredRecordSerialisedTask.AsTask);

            // then
            actualOperationCanceledException.Should().BeSameAs(cancellationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.IsAny<Exception>()),
                    Times.Never);

            this.fhirReconciliationServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldPassCancellationThroughOnGetStructuredRecordSerialisedIfAlreadyCancelledAsync()
        {
            // given
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();
            CancellationToken cancelledToken = cancellationTokenSource.Token;
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;
            Guid inputCorrelationId = Guid.NewGuid();

            // when
            ValueTask<string> getStructuredRecordSerialisedTask =
                this.patientCoordinationService.GetStructuredRecordSerialisedAsync(
                    correlationId: inputCorrelationId,
                    nhsNumber: inputNhsNumber,
                    cancellationToken: cancelledToken);

            OperationCanceledException actualOperationCanceledException =
                await Assert.ThrowsAnyAsync<OperationCanceledException>(
                    getStructuredRecordSerialisedTask.AsTask);

            // then
            // Checked before any dependency is touched, so an abandoned request does no work.
            actualOperationCanceledException.CancellationToken.Should().Be(cancelledToken);

            this.patientOrchestrationServiceMock.VerifyNoOtherCalls();
            this.fhirReconciliationServiceMock.VerifyNoOtherCalls();
            this.auditAndMetricBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
