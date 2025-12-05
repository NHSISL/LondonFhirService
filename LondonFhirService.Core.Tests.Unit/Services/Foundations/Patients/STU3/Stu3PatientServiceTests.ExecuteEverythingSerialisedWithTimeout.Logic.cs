// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Moq;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Patients.STU3
{
    public partial class Stu3PatientServiceTests
    {
        [Fact]
        public async Task ShouldExecuteEverythingSerialisedWithTimeout()
        {
            // given
            Bundle randomBundle = CreateRandomBundle();
            Bundle outputBundle = randomBundle.DeepClone();
            Bundle expectedBundle = outputBundle.DeepClone();
            string randomId = GetRandomString();
            string inputId = randomId;
            var fhirProvider = this.ddsFhirProviderMock.Object;
            var fhirProviderCopy = this.ddsFhirProviderMock.Object.DeepClone();
            Guid correlationId = Guid.NewGuid();

            expectedBundle.Meta.Extension = new List<Extension>
            {
                new Extension
                {
                    Url = "http://example.org/fhir/StructureDefinition/meta-source",
                    Value = new FhirUri(fhirProviderCopy.Source)
                }
            };

            expectedBundle.Meta.Tag = new List<Coding>
            {
                new Coding
                {
                    System = fhirProviderCopy.System,
                    Code = fhirProviderCopy.Code,
                    Display = fhirProviderCopy.ProviderName
                }
            };

            string rawOutputBundle = this.fhirJsonSerializer.SerializeToString(expectedBundle);
            string rawExpectedBundle = rawOutputBundle;

            (string Json, Exception Exception) expectedResult = (rawExpectedBundle, null);

            this.ddsFhirProviderMock.Setup(p => p.Patients.EverythingSerialisedAsync(
                inputId,
                null,
                null,
                null,
                null,
                null,
                It.IsAny<CancellationToken>()))
                    .ReturnsAsync(rawOutputBundle);

            // when
            (string Json, Exception Exception) actualResult =
                await this.patientService.ExecuteEverythingSerialisedWithTimeoutAsync(
                    fhirProvider,
                    default,
                    correlationId,
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null);

            // then
            actualResult.Should().BeEquivalentTo(expectedResult);

            this.ddsFhirProviderMock.Verify(p => p.Patients.EverythingSerialisedAsync(
                inputId,
                null,
                null,
                null,
                null,
                null,
                It.IsAny<CancellationToken>()),
                    Times.Once());

            this.ddsFhirProviderMock.Verify(provider =>
                provider.DisplayName,
                    Times.AtLeastOnce);

            //this.ddsFhirProviderMock.Verify(provider =>
            //    provider.System,
            //        Times.Once);

            //this.ddsFhirProviderMock.Verify(provider =>
            //    provider.Code,
            //        Times.Once);

            //this.ddsFhirProviderMock.Verify(provider =>
            //    provider.ProviderName,
            //        Times.Once);

            //this.ddsFhirProviderMock.Verify(provider =>
            //    provider.Source,
            //        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.ddsFhirProviderMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task
            ShouldReturnNullAndOperationCancelledExceptionOnExecuteEverythingSerialisedWithTimeoutWhenTokenCancelled()
        {
            // given
            Bundle randomBundle = CreateRandomBundle();
            string randomId = GetRandomString();
            string inputId = randomId;
            CancellationToken alreadyCanceledToken = new CancellationToken(true);
            Guid correlationId = Guid.NewGuid();

            OperationCanceledException operationCanceledException =
                new OperationCanceledException(alreadyCanceledToken);

            var fhirProvider = this.ddsFhirProviderMock.Object;

            (string Json, Exception Exception) expectedResult = (null, operationCanceledException);

            // when
            (string Json, Exception Exception) actualResult =
                await this.patientService.ExecuteEverythingSerialisedWithTimeoutAsync(
                    fhirProvider,
                    alreadyCanceledToken,
                    correlationId,
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null);

            // then
            actualResult.Should().BeEquivalentTo(expectedResult);

            this.ddsFhirProviderMock.Verify(p => p.Patients.EverythingSerialisedAsync(
                inputId,
                null,
                null,
                null,
                null,
                null,
                alreadyCanceledToken),
                    Times.Never());

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.ddsFhirProviderMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task
            ShouldReturnNullAndOperationCancelledExceptionOnExecuteEverythingSerialisedWithTimeoutWhenCancelled()
        {
            // given
            string randomId = GetRandomString();
            string inputId = randomId;
            OperationCanceledException operationCanceledException = new OperationCanceledException();
            var fhirProvider = this.ddsFhirProviderMock.Object;
            Guid correlationId = Guid.NewGuid();

            (string Json, Exception Exception) expectedResult = (null, operationCanceledException);

            this.ddsFhirProviderMock.Setup(p => p.Patients.EverythingSerialisedAsync(
                inputId,
                null,
                null,
                null,
                null,
                null,
                It.IsAny<CancellationToken>()))
                    .ThrowsAsync(operationCanceledException);

            // when
            (string Json, Exception Exception) actualResult =
                await this.patientService.ExecuteEverythingSerialisedWithTimeoutAsync(
                    fhirProvider,
                    default,
                    correlationId,
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null);

            // then
            actualResult.Should().BeEquivalentTo(expectedResult);

            this.ddsFhirProviderMock.Verify(p => p.Patients.EverythingSerialisedAsync(
                inputId,
                null,
                null,
                null,
                null,
                null,
                It.IsAny<CancellationToken>()),
                    Times.Once());

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.ddsFhirProviderMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnNullAndExceptionOnExecuteEverythingSerialisedWithTimeoutWhenException()
        {
            // given
            string randomId = GetRandomString();
            string inputId = randomId;
            Exception exception = new Exception(GetRandomString());
            var fhirProvider = this.ddsFhirProviderMock.Object;
            Guid correlationId = Guid.NewGuid();

            (string Json, Exception Exception) expectedResult = (null, exception);

            this.ddsFhirProviderMock.Setup(p => p.Patients.EverythingSerialisedAsync(
                inputId,
                null,
                null,
                null,
                null,
                null,
                It.IsAny<CancellationToken>()))
                    .ThrowsAsync(exception);

            // when
            (string Json, Exception Exception) actualResult =
                await this.patientService.ExecuteEverythingSerialisedWithTimeoutAsync(
                    fhirProvider,
                    default,
                    correlationId,
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null);

            // then
            actualResult.Should().BeEquivalentTo(expectedResult);

            this.ddsFhirProviderMock.Verify(p => p.Patients.EverythingSerialisedAsync(
                inputId,
                null,
                null,
                null,
                null,
                null,
                It.IsAny<CancellationToken>()),
                    Times.Once());

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.ddsFhirProviderMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task
            ShouldReturnNullAndTimeoutExceptionOnExecuteEverythingSerialisedWithTimeoutWhenEverythingTimesOut()
        {
            // given
            var timeoutMilliseconds = 1;
            this.patientServiceConfig.MaxProviderWaitTimeMilliseconds = timeoutMilliseconds;
            string randomId = GetRandomString();
            string inputId = randomId;
            Guid correlationId = Guid.NewGuid();

            OperationCanceledException operationCanceledException =
                new OperationCanceledException("A task was canceled.");

            var fhirProvider = this.ddsFhirProviderMock.Object;

            this.ddsFhirProviderMock.Setup(p => p.Patients.EverythingSerialisedAsync(
                inputId,
                null,
                null,
                null,
                null,
                null,
                It.IsAny<CancellationToken>()))
                     .Returns(async (string id,
                        DateTimeOffset? start,
                        DateTimeOffset? end,
                        string typeFiler,
                        DateTimeOffset? since,
                        int? count,
                        CancellationToken token) =>
                         {
                             await Task.Delay(Timeout.Infinite, token);
                             return default(string);
                         });

            // when
            (string Json, Exception Exception) actualResult =
                await this.patientService.ExecuteEverythingSerialisedWithTimeoutAsync(
                    fhirProvider,
                    default,
                    correlationId,
                    inputId,
                    null,
                    null,
                    null,
                    null,
                    null);

            // then
            actualResult.Json.Should().BeNull();
            actualResult.Exception.Should().BeOfType<TimeoutException>();

            actualResult.Exception.Message.Should().Be(
                $"Provider call exceeded {timeoutMilliseconds} milliseconds.");

            actualResult.Exception.InnerException.Should().BeOfType<TaskCanceledException>();

            this.ddsFhirProviderMock.Verify(p => p.Patients.EverythingSerialisedAsync(
                inputId,
                null,
                null,
                null,
                null,
                null,
                It.IsAny<CancellationToken>()),
                    Times.Once());

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.ddsFhirProviderMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
        }
    }
}
