// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Orchestrations.Accesses.Exceptions;
using Moq;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Orchestrations.Accesses
{
    public partial class AccessOrchestrationServiceTests
    {
        [Theory]
        [MemberData(nameof(DependencyValidationExceptions))]
        public async Task ShouldThrowDependencyValidationOnValidateAccessAndLogItAsync(
            Xeption dependencyException)
        {
            // given
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentUserAsync())
                    .ThrowsAsync(dependencyException);

            var expectedAccessOrchestrationDependencyValidationException =
                new AccessOrchestrationDependencyValidationException(
                    message: "Access orchestration dependency validation error occurred, " +
                        "fix the errors and try again.",
                    innerException: dependencyException.InnerException as Xeption);

            // when
            ValueTask validateAccessTask = accessOrchestrationService.ValidateAccess(inputNhsNumber);

            AccessOrchestrationDependencyValidationException actualAccessOrchestrationDependencyValidationException =
                await Assert.ThrowsAsync<AccessOrchestrationDependencyValidationException>(
                    testCode: validateAccessTask.AsTask);

            // then
            actualAccessOrchestrationDependencyValidationException
                .Should().BeEquivalentTo(expectedAccessOrchestrationDependencyValidationException);

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentUserAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
               broker.LogErrorAsync(It.Is(SameExceptionAs(
                   expectedAccessOrchestrationDependencyValidationException))),
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

        [Theory]
        [MemberData(nameof(DependencyExceptions))]
        public async Task ShouldThrowDependencyExceptionOnValidateAccessAndLogItAsync(
            Xeption dependencyException)
        {
            // given
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentUserAsync())
                    .ThrowsAsync(dependencyException);

            var expectedAccessOrchestrationDependencyException =
                new AccessOrchestrationDependencyException(
                    message: "Access orchestration dependency error occurred, " +
                        "fix the errors and try again.",
                    innerException: dependencyException.InnerException as Xeption);

            // when
            ValueTask validateAccessTask = accessOrchestrationService.ValidateAccess(inputNhsNumber);

            AccessOrchestrationDependencyException actualAccessOrchestrationDependencyException =
                await Assert.ThrowsAsync<AccessOrchestrationDependencyException>(
                    testCode: validateAccessTask.AsTask);

            // then
            actualAccessOrchestrationDependencyException
                .Should().BeEquivalentTo(expectedAccessOrchestrationDependencyException);

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentUserAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
               broker.LogErrorAsync(It.Is(SameExceptionAs(
                   expectedAccessOrchestrationDependencyException))),
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

        [Fact]
        public async Task ShouldThrowServiceExceptionOnValidateAccessAndLogItAsync()
        {
            // given
            var serviceException = new Exception();
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;

            this.securityBrokerMock.Setup(broker =>
                broker.GetCurrentUserAsync())
                    .ThrowsAsync(serviceException);

            var failedServiceAccessOrchestrationException =
                new FailedServiceAccessOrchestrationException(
                    message: "Failed access orchestration service error occurred, please contact support.",
                    innerException: serviceException,
                    data: serviceException.Data);

            var expectedAccessOrchestrationServiceException =
                new AccessOrchestrationServiceException(
                    message: "Access orchestration service error occurred, please contact support.",
                    innerException: failedServiceAccessOrchestrationException);

            // when
            ValueTask validateAccessTask = accessOrchestrationService.ValidateAccess(inputNhsNumber);

            AccessOrchestrationServiceException actualAccessOrchestrationServiceException =
                await Assert.ThrowsAsync<AccessOrchestrationServiceException>(
                    testCode: validateAccessTask.AsTask);

            // then
            actualAccessOrchestrationServiceException
                .Should().BeEquivalentTo(expectedAccessOrchestrationServiceException);

            this.securityBrokerMock.Verify(broker =>
                broker.GetCurrentUserAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
               broker.LogErrorAsync(It.Is(SameExceptionAs(
                   expectedAccessOrchestrationServiceException))),
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
