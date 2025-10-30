// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using FluentAssertions;
using Force.DeepCloner;
using Hl7.Fhir.Model;
using Moq;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.Patients
{
    public partial class PatientServiceTests
    {
        [Fact]
        public async Task ShouldExecuteWithTimeout()
        {
            // given

            Bundle randomBundle = CreateRandomBundle();
            Bundle outputBundle = randomBundle.DeepClone();
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;
            var fhirProvider = this.ddsFhirProviderMock.Object;

            (Bundle Bundle, Exception Exception) expectedResult = (outputBundle, null);

            this.ddsFhirProviderMock.Setup(p => p.Patients.Everything(
                inputNhsNumber,
                null,
                null,
                null,
                null,
                null,
                default))
                    .ReturnsAsync(outputBundle);

            // when
            (Bundle Bundle, Exception Exception) actualResult =
                await this.patientService.ExecuteWithTimeoutAsync(
                    fhirProvider.Patients,
                    default,
                    inputNhsNumber,
                    null,
                    null,
                    null,
                    null,
                    null);

            // then
            actualResult.Should().BeEquivalentTo(expectedResult);

            this.ddsFhirProviderMock.Verify(p => p.Patients.Everything(
                inputNhsNumber,
                null,
                null,
                null,
                null,
                null,
                default),
                    Times.Once());

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.ddsFhirProviderMock.VerifyNoOtherCalls();
            this.ldsFhirProviderMock.VerifyNoOtherCalls();
        }
    }
}
