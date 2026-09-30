// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using ISL.Security.Client.Models.Foundations.Users;
using LondonFhirService.Core.Models.Brokers.ConsumerAccesses;
using LondonFhirService.Core.Models.Foundations.Providers;
using LondonFhirService.Core.Models.Orchestrations.Accesses;
using LondonFhirService.Core.Models.Orchestrations.Patients;
using LondonFhirService.Core.Services.Orchestrations.Patients.STU3;
using Moq;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Core.Tests.Unit.Services.Orchestrations.Patients.STU3
{
    /// <summary>
    /// Cancellation is never wrapped. Whichever dependency the caller's cancellation surfaces
    /// from, it reaches the caller as the same exception, and nothing is logged as an error.
    /// </summary>
    public partial class Stu3PatientOrchestrationServiceTests
    {
        [Theory]
        [MemberData(nameof(CancellationExceptions))]
        public async Task ShouldPassCancellationThroughOnGetStructuredRecordIfAccessCheckIsCancelledAsync(
            Exception cancellationException)
        {
            // given
            string userId = GetRandomString();
            User randomUser = CreateRandomUser(userId);
            string randomNhsNumber = GetRandomStringWithLengthOf(10);
            string inputNhsNumber = randomNhsNumber;
            Guid correlationId = Guid.NewGuid();

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentUserAsync())
                    .ReturnsAsync(randomUser);

            this.consumerAccessServiceMock.Setup(service =>
                service.CheckConsumerAccessAsync(
                    It.IsAny<ValidateAccessRequest>(),
                    It.IsAny<CancellationToken>()))
                        .ThrowsAsync(cancellationException);

            // when
            ValueTask<StructuredRecordsResponse> getStructuredRecordTask =
                this.patientOrchestrationService.GetStructuredRecordSerialisedAsync(
                    correlationId,
                    inputNhsNumber,
                    cancellationToken: TestContext.Current.CancellationToken);

            OperationCanceledException actualOperationCanceledException =
                await Assert.ThrowsAnyAsync<OperationCanceledException>(getStructuredRecordTask.AsTask);

            // then
            actualOperationCanceledException.Should().BeSameAs(cancellationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.IsAny<Exception>()),
                    Times.Never);

            this.providerServiceMock.VerifyNoOtherCalls();
            this.patientServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(CancellationExceptions))]
        public async Task ShouldPassCancellationThroughOnGetStructuredRecordIfProviderCallsAreCancelledAsync(
            Exception cancellationException)
        {
            // given
            using var cancellationTokenSource = new CancellationTokenSource();
            CancellationToken inputCancellationToken = cancellationTokenSource.Token;
            string randomNhsNumber = GetRandomStringWithLengthOf(10);
            string inputNhsNumber = randomNhsNumber;
            Guid correlationId = Guid.NewGuid();
            Provider randomPrimaryProvider = CreateRandomPrimaryProvider();
            var allProviders = new List<Provider> { randomPrimaryProvider };

            IStu3PatientOrchestrationService patientOrchestrationService =
                CreateOrchestrationService(new AccessConfigurations { CheckAccessPermissions = false });

            // The token the caller passed, not a default one. A dropped token would leave this
            // setup unmatched and the discovery call returning nothing.
            this.providerServiceMock.Setup(service =>
                service.RetrieveAllProvidersAsListAsync(inputCancellationToken))
                    .ReturnsAsync(allProviders);

            this.patientServiceMock.Setup(service =>
                service.GetStructuredRecordSerialisedAsync(
                    It.IsAny<List<Provider>>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<Guid?>(),
                    inputCancellationToken))
                        .ThrowsAsync(cancellationException);

            // when
            ValueTask<StructuredRecordsResponse> getStructuredRecordTask =
                patientOrchestrationService.GetStructuredRecordSerialisedAsync(
                    correlationId,
                    inputNhsNumber,
                    cancellationToken: inputCancellationToken);

            OperationCanceledException actualOperationCanceledException =
                await Assert.ThrowsAnyAsync<OperationCanceledException>(getStructuredRecordTask.AsTask);

            // then
            actualOperationCanceledException.Should().BeSameAs(cancellationException);

            this.providerServiceMock.Verify(service =>
                service.RetrieveAllProvidersAsListAsync(inputCancellationToken),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.IsAny<Exception>()),
                    Times.Never);

            this.providerServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldPassCancellationThroughOnGetStructuredRecordIfAlreadyCancelledAsync()
        {
            // given
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();
            CancellationToken cancelledToken = cancellationTokenSource.Token;
            string randomNhsNumber = GetRandomStringWithLengthOf(10);
            string inputNhsNumber = randomNhsNumber;
            Guid correlationId = Guid.NewGuid();

            // when
            ValueTask<StructuredRecordsResponse> getStructuredRecordTask =
                this.patientOrchestrationService.GetStructuredRecordSerialisedAsync(
                    correlationId,
                    inputNhsNumber,
                    cancellationToken: cancelledToken);

            OperationCanceledException actualOperationCanceledException =
                await Assert.ThrowsAnyAsync<OperationCanceledException>(getStructuredRecordTask.AsTask);

            // then
            actualOperationCanceledException.CancellationToken.Should().Be(cancelledToken);

            this.securityBrokerMock.VerifyNoOtherCalls();
            this.consumerAccessServiceMock.VerifyNoOtherCalls();
            this.providerServiceMock.VerifyNoOtherCalls();
            this.patientServiceMock.VerifyNoOtherCalls();
            this.auditAndMetricBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(CancellationExceptions))]
        public async Task ShouldPassCancellationThroughOnValidateAccessAsync(Exception cancellationException)
        {
            // given
            string userId = GetRandomString();
            User randomUser = CreateRandomUser(userId);
            string randomNhsNumber = GetRandomStringWithLengthOf(10);
            string inputNhsNumber = randomNhsNumber;
            Guid correlationId = Guid.NewGuid();

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentUserAsync())
                    .ReturnsAsync(randomUser);

            this.consumerAccessServiceMock.Setup(service =>
                service.CheckConsumerAccessAsync(
                    It.IsAny<ValidateAccessRequest>(),
                    It.IsAny<CancellationToken>()))
                        .ThrowsAsync(cancellationException);

            // when
            ValueTask validateAccessTask =
                this.patientOrchestrationService.ValidateAccess(
                    inputNhsNumber, correlationId, TestContext.Current.CancellationToken);

            OperationCanceledException actualOperationCanceledException =
                await Assert.ThrowsAnyAsync<OperationCanceledException>(validateAccessTask.AsTask);

            // then
            actualOperationCanceledException.Should().BeSameAs(cancellationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.IsAny<Exception>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldPassCancellationThroughOnValidateAccessIfAlreadyCancelledAsync()
        {
            // given
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();
            CancellationToken cancelledToken = cancellationTokenSource.Token;
            string randomNhsNumber = GetRandomStringWithLengthOf(10);
            string inputNhsNumber = randomNhsNumber;
            Guid correlationId = Guid.NewGuid();

            // when
            ValueTask validateAccessTask =
                this.patientOrchestrationService.ValidateAccess(
                    inputNhsNumber, correlationId, cancelledToken);

            OperationCanceledException actualOperationCanceledException =
                await Assert.ThrowsAnyAsync<OperationCanceledException>(validateAccessTask.AsTask);

            // then
            actualOperationCanceledException.CancellationToken.Should().Be(cancelledToken);

            this.securityBrokerMock.VerifyNoOtherCalls();
            this.consumerAccessServiceMock.VerifyNoOtherCalls();
            this.auditAndMetricBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
