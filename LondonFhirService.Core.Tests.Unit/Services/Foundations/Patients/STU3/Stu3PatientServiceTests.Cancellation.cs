// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.FhirRecords;
using LondonFhirService.Core.Models.Foundations.Providers;
using LondonFhirService.Core.Services.Foundations.Patients.STU3;
using LondonFhirService.Providers.FHIR.STU3.Abstractions;
using Moq;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Patients.STU3
{
    /// <summary>
    /// The caller's cancellation is never wrapped and never logged as an error. This is distinct
    /// from the fan out's own per provider timeout, which stays that provider's failure while the
    /// request carries on - see the ExecuteGetStructuredRecordSerialisedWithTimeout tests.
    /// </summary>
    public partial class Stu3PatientServiceTests
    {
        [Fact]
        public async Task ShouldPassCancellationThroughOnGetStructuredRecordSerialisedIfAlreadyCancelledAsync()
        {
            // given
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();
            CancellationToken cancelledToken = cancellationTokenSource.Token;

            List<Provider> inputProviders = new List<Provider>
            {
                new Provider { FriendlyName = "DDS Provider", FullyQualifiedName = "DDS", IsPrimary = true }
            };

            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;
            Guid correlationId = Guid.NewGuid();

            // when
            ValueTask<List<(string Provider, string Json)>> getStructuredRecordTask =
                this.patientService.GetStructuredRecordSerialisedAsync(
                    activeProviders: inputProviders,
                    correlationId: correlationId,
                    nhsNumber: inputNhsNumber,
                    cancellationToken: cancelledToken);

            OperationCanceledException actualOperationCanceledException =
                await Assert.ThrowsAnyAsync<OperationCanceledException>(
                    testCode: getStructuredRecordTask.AsTask);

            // then
            // Checked before any provider is resolved or called.
            actualOperationCanceledException.CancellationToken.Should().Be(cancelledToken);

            this.ddsFhirProviderMock.VerifyNoOtherCalls();
            this.auditAndMetricBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldPassCancellationThroughOnGetStructuredRecordSerialisedIfCancelledDuringFanOutAsync()
        {
            // given
            using var cancellationTokenSource = new CancellationTokenSource();
            CancellationToken inputCancellationToken = cancellationTokenSource.Token;

            List<Provider> inputProviders = new List<Provider>
            {
                new Provider { FriendlyName = "DDS Provider", FullyQualifiedName = "DDS", IsPrimary = true }
            };

            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;
            Guid correlationId = Guid.NewGuid();

            var patientServiceMock = new Mock<Stu3PatientService>(
                this.fhirBroker,
                this.auditAndMetricBrokerMock.Object,
                this.identifierBrokerMock.Object,
                this.dateTimeBrokerMock.Object,
                this.securityAuditBrokerMock.Object,
                this.storageBrokerFactoryMock.Object,
                this.dispatcherMock.Object,
                this.requestTraceBrokerMock.Object,
                this.loggingBrokerMock.Object,
                this.patientServiceConfig)
            {
                CallBase = true
            };

            // The caller gives up while the provider is in flight. The provider call reports it the
            // way the per provider method does - as that provider's outcome, not a throw.
            patientServiceMock.Setup(service =>
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<IFhirProvider>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<Guid?>(),
                    inputCancellationToken))
                .Returns(() =>
                {
                    cancellationTokenSource.Cancel();

                    return Task.FromResult<(string Provider, string Json, Exception Exception)>(
                        ("DDS Provider", null, new OperationCanceledException(inputCancellationToken)));
                });

            Stu3PatientService mockedPatientService = patientServiceMock.Object;

            // when
            ValueTask<List<(string Provider, string Json)>> getStructuredRecordTask =
                mockedPatientService.GetStructuredRecordSerialisedAsync(
                    activeProviders: inputProviders,
                    correlationId: correlationId,
                    nhsNumber: inputNhsNumber,
                    cancellationToken: inputCancellationToken);

            OperationCanceledException actualOperationCanceledException =
                await Assert.ThrowsAnyAsync<OperationCanceledException>(
                    testCode: getStructuredRecordTask.AsTask);

            // then
            // Nobody is waiting for the bundles any more, so the request stops with the caller's
            // cancellation rather than carrying on - and the cancelled provider is not reported
            // as a failed one.
            actualOperationCanceledException.CancellationToken.Should().Be(inputCancellationToken);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.IsAny<Exception>()),
                    Times.Never);
        }

        [Fact]
        public async Task ShouldPassCancellationThroughOnGetStructuredRecordSerialisedIfFanOutThrowsItAsync()
        {
            // given
            List<Provider> inputProviders = new List<Provider>
            {
                new Provider { FriendlyName = "DDS Provider", FullyQualifiedName = "DDS", IsPrimary = true }
            };

            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;
            Guid correlationId = Guid.NewGuid();
            var operationCanceledException = new OperationCanceledException(GetRandomString());

            var patientServiceMock = new Mock<Stu3PatientService>(
                this.fhirBroker,
                this.auditAndMetricBrokerMock.Object,
                this.identifierBrokerMock.Object,
                this.dateTimeBrokerMock.Object,
                this.securityAuditBrokerMock.Object,
                this.storageBrokerFactoryMock.Object,
                this.dispatcherMock.Object,
                this.requestTraceBrokerMock.Object,
                this.loggingBrokerMock.Object,
                this.patientServiceConfig)
            {
                CallBase = true
            };

            patientServiceMock.Setup(service =>
                service.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<IFhirProvider>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(operationCanceledException);

            Stu3PatientService mockedPatientService = patientServiceMock.Object;

            // when
            ValueTask<List<(string Provider, string Json)>> getStructuredRecordTask =
                mockedPatientService.GetStructuredRecordSerialisedAsync(
                    activeProviders: inputProviders,
                    correlationId: correlationId,
                    nhsNumber: inputNhsNumber,
                    cancellationToken: TestContext.Current.CancellationToken);

            OperationCanceledException actualOperationCanceledException =
                await Assert.ThrowsAnyAsync<OperationCanceledException>(
                    testCode: getStructuredRecordTask.AsTask);

            // then
            actualOperationCanceledException.Should().BeSameAs(operationCanceledException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.IsAny<Exception>()),
                    Times.Never);
        }

        [Fact]
        public async Task ShouldPassTheDispatchTokenToTheQueuedFhirRecordWriteAsync()
        {
            // given
            using var dispatchTokenSource = new CancellationTokenSource();
            CancellationToken dispatchToken = dispatchTokenSource.Token;
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;
            string randomJson = GetRandomString();
            string outputJson = randomJson;
            Guid correlationId = Guid.NewGuid();

            // The queued write runs on the drain worker, under whatever token the worker hands it.
            this.dispatcherMock.Setup(dispatcher =>
                dispatcher.TryDispatch(It.IsAny<Func<CancellationToken, ValueTask>>()))
                    .Returns((Func<CancellationToken, ValueTask> work) =>
                    {
                        work(dispatchToken).AsTask().GetAwaiter().GetResult();

                        return true;
                    });

            this.securityAuditBrokerMock.Setup(broker =>
                broker.ApplyAddAuditValuesAsync(It.IsAny<FhirRecord>()))
                    .ReturnsAsync((FhirRecord fhirRecord) => fhirRecord);

            this.ddsFhirProviderMock.Setup(provider =>
                provider.Patients.GetStructuredRecordSerialisedAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<CancellationToken>()))
                        .ReturnsAsync(outputJson);

            // when
            await this.patientService.ExecuteGetStructuredRecordSerialisedWithTimeoutAsync(
                "DDS Provider",
                isPrimaryProvider: true,
                this.ddsFhirProviderMock.Object,
                correlationId,
                inputNhsNumber,
                cancellationToken: TestContext.Current.CancellationToken);

            // then
            this.storageBrokerMock.Verify(broker =>
                broker.InsertFhirRecordAsync(
                    It.IsAny<FhirRecord>(),
                    dispatchToken),
                        Times.Once);
        }
    }
}
