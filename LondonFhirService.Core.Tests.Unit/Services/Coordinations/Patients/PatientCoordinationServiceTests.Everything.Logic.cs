﻿// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using FluentAssertions;
using Force.DeepCloner;
using Hl7.Fhir.Model;
using Moq;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Core.Tests.Unit.Services.Coordinations.Patients
{
    public partial class PatientCoordinationServiceTests
    {
        [Fact]
        public async Task ShouldCallEverythingAsync()
        {
            // given
            string randomId = GetRandomString();
            string inputId = randomId;
            DateTimeOffset? inputStart = GetRandomDateTimeOffset();
            DateTimeOffset? inputEnd = GetRandomDateTimeOffset();
            string inputTypeFilter = GetRandomString();
            DateTimeOffset? inputSince = GetRandomDateTimeOffset();
            int? inputCount = GetRandomNumber();
            CancellationToken cancellationToken = CancellationToken.None;
            Bundle randomBundle = CreateRandomBundle();
            Bundle expectedBundle = randomBundle.DeepClone();

            this.accessOrchestrationServiceMock.Setup(service =>
                service.ValidateAccess(inputId));

            this.patientOrchestrationServiceMock.Setup(service =>
                service.Everything(
                    inputId,
                    inputStart,
                    inputEnd,
                    inputTypeFilter,
                    inputSince,
                    inputCount,
                    cancellationToken))
                    .ReturnsAsync(expectedBundle);

            // when
            Bundle actualBundle = await this.patientCoordinationService.Everything(
                inputId,
                inputStart,
                inputEnd,
                inputTypeFilter,
                inputSince,
                inputCount,
                cancellationToken);

            // then
            actualBundle.Should().BeEquivalentTo(expectedBundle);

            this.accessOrchestrationServiceMock.Verify(service =>
                service.ValidateAccess(inputId),
                    Times.Once);

            this.patientOrchestrationServiceMock.Verify(service =>
                service.Everything(
                    inputId,
                    inputStart,
                    inputEnd,
                    inputTypeFilter,
                    inputSince,
                    inputCount,
                    cancellationToken),
                    Times.Once);

            this.accessOrchestrationServiceMock.VerifyNoOtherCalls();
            this.patientOrchestrationServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
