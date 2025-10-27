// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Orchestrations.Accesses.Exceptions;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Orchestrations.Accesses
{
    public partial class AccessOrchestrationServiceTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ShouldThrowValidationExceptionOnValidateAccess(string invalidText)
        {
            // given
            var invalidArgumentAccessOrchestrationException =
                new InvalidArgumentAccessOrchestrationException(
                    message: "Invalid argument access orchestration exception, " +
                        "please correct the errors and try again.");

            invalidArgumentAccessOrchestrationException.AddData(
                key: "nhsNumber",
                values: "Text is invalid");

            var expectedAccessOrchestrationValidationException =
                new AccessOrchestrationValidationException(
                    message: "Access orchestration validation error occurred, " +
                        "fix the errors and try again.",
                    innerException: invalidArgumentAccessOrchestrationException);

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentUserAsync())
                    .ThrowsAsync(invalidArgumentAccessOrchestrationException);

            // when
            ValueTask validateAccessTask = accessOrchestrationService.ValidateAccess(invalidText);

            AccessOrchestrationValidationException actualAccessOrchestrationValidationException =
                await Assert.ThrowsAsync<AccessOrchestrationValidationException>(
                    testCode: validateAccessTask.AsTask);

            // then
            actualAccessOrchestrationValidationException
                .Should().BeEquivalentTo(expectedAccessOrchestrationValidationException);

            this.loggingBrokerMock.Verify(broker =>
               broker.LogErrorAsync(It.Is(SameExceptionAs(
                   expectedAccessOrchestrationValidationException))),
                       Times.Once);

            this.consumerServiceMock.VerifyNoOtherCalls();
            this.consumerAccessServiceMock.VerifyNoOtherCalls();
            this.pdsDataServiceMock.VerifyNoOtherCalls();
            this.auditBrokerMock.VerifyNoOtherCalls();
            this.securityBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.identifierBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
