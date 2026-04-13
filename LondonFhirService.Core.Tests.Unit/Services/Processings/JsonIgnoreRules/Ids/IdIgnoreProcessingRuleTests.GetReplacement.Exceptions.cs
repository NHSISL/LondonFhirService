// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Processings.JsonIgnoreRules.IdIgnoreRules.Exceptions;
using LondonFhirService.Core.Services.Processings.JsonIgnoreRules;
using Moq;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Processings.JsonIgnoreRules.Ids
{
    public partial class IdIgnoreProcessingRuleTests
    {
        [Theory]
        [MemberData(nameof(DependencyExceptions))]
        public async Task ShouldThrowDependencyExceptionOnGetReplacementAndLogItAsync(
            Xeption dependencyException)
        {
            // given
            JsonElement randomElement = new();

            var expectedIdIgnoreProcessingDependencyException =
                new IdIgnoreProcessingDependencyException(
                    message: "Id ignore processing dependency error occurred, contact support.",
                    innerException: dependencyException);

            var idIgnoreProcessingRuleMock = new Mock<IdIgnoreProcessingRule>(
                jsonElementServiceMock.Object,
                loggingBrokerMock.Object)
            { CallBase = true };

            idIgnoreProcessingRuleMock.Setup(service =>
                service.ValidateOnGetReplacement(
                    It.IsAny<JsonElement>()))
                        .Throws(dependencyException);

            // when
            ValueTask<JsonElement> getReplacementTask =
                idIgnoreProcessingRuleMock.Object.GetReplacementAsync(randomElement);

            IdIgnoreProcessingDependencyException actualIdIgnoreProcessingDependencyException =
                await Assert.ThrowsAsync<IdIgnoreProcessingDependencyException>(
                    testCode: getReplacementTask.AsTask);

            // then
            actualIdIgnoreProcessingDependencyException
                .Should().BeEquivalentTo(expectedIdIgnoreProcessingDependencyException);

            idIgnoreProcessingRuleMock.Verify(service =>
                service.ValidateOnGetReplacement(
                    randomElement),
                        Times.Once);

            idIgnoreProcessingRuleMock.Verify(service =>
                service.GetReplacementAsync(
                    randomElement),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
               broker.LogErrorAsync(It.Is(SameExceptionAs(
                   expectedIdIgnoreProcessingDependencyException))),
                       Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            idIgnoreProcessingRuleMock.VerifyNoOtherCalls();
            jsonElementServiceMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(DependencyValidationExceptions))]
        public async Task ShouldThrowDependencyValidationOnGetReplacementAndLogItAsync(
            Xeption dependencyValidationException)
        {
            // given
            JsonElement randomElement = new();

            var expectedIdIgnoreProcessingDependencyValidationException =
                new IdIgnoreProcessingDependencyValidationException(
                    message: "Id ignore processing dependency validation error occurred, contact support.",
                    innerException: dependencyValidationException);

            var idIgnoreProcessingRuleMock = new Mock<IdIgnoreProcessingRule>(
                jsonElementServiceMock.Object,
                loggingBrokerMock.Object)
            { CallBase = true };

            idIgnoreProcessingRuleMock.Setup(service =>
                service.ValidateOnGetReplacement(
                    It.IsAny<JsonElement>()))
                        .Throws(dependencyValidationException);

            // when
            ValueTask<JsonElement> getReplacementTask =
                idIgnoreProcessingRuleMock.Object.GetReplacementAsync(randomElement);

            IdIgnoreProcessingDependencyValidationException 
                actualIdIgnoreProcessingDependencyValidationException =
                    await Assert.ThrowsAsync<IdIgnoreProcessingDependencyValidationException>(
                        testCode: getReplacementTask.AsTask);

            // then
            actualIdIgnoreProcessingDependencyValidationException
                .Should().BeEquivalentTo(expectedIdIgnoreProcessingDependencyValidationException);

            idIgnoreProcessingRuleMock.Verify(service =>
                service.ValidateOnGetReplacement(
                    randomElement),
                        Times.Once);

            idIgnoreProcessingRuleMock.Verify(service =>
                service.GetReplacementAsync(
                    randomElement),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
               broker.LogErrorAsync(It.Is(SameExceptionAs(
                   expectedIdIgnoreProcessingDependencyValidationException))),
                       Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            idIgnoreProcessingRuleMock.VerifyNoOtherCalls();
            jsonElementServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnGetReplacementIfResourceIsInvalidAsync()
        {
            // given
            JsonElement randomElement = new();
            var serviceException = new Exception();

            var failedIdIgnoreProcessingException =
                new FailedIdIgnoreProcessingException(
                    message: "Failed id ignore processing exception occurred, please contact support",
                    innerException: serviceException,
                    data: serviceException.Data);

            var expectedIdIgnoreProcessingServiceException =
                new IdIgnoreProcessingServiceException(
                    message: "Id ignore processing service error occurred, contact support.",
                    innerException: failedIdIgnoreProcessingException);

            var idIgnoreProcessingRuleMock = new Mock<IdIgnoreProcessingRule>(
                jsonElementServiceMock.Object,
                loggingBrokerMock.Object)
            { CallBase = true };

            idIgnoreProcessingRuleMock.Setup(service =>
                service.ValidateOnGetReplacement(
                    randomElement))
                        .Throws(serviceException);

            // when
            ValueTask<JsonElement> getReplacementTask =
                idIgnoreProcessingRuleMock.Object.GetReplacementAsync(
                    randomElement);

            IdIgnoreProcessingServiceException actualIdIgnoreProcessingServiceException =
                await Assert.ThrowsAsync<IdIgnoreProcessingServiceException>(
                    getReplacementTask.AsTask);

            // then
            actualIdIgnoreProcessingServiceException.Should()
                .BeEquivalentTo(expectedIdIgnoreProcessingServiceException);

            idIgnoreProcessingRuleMock.Verify(service =>
                service.ValidateOnGetReplacement(
                    randomElement),
                        Times.Once);

            idIgnoreProcessingRuleMock.Verify(service =>
                service.GetReplacementAsync(
                    randomElement),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedIdIgnoreProcessingServiceException))),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            idIgnoreProcessingRuleMock.VerifyNoOtherCalls();
            jsonElementServiceMock.VerifyNoOtherCalls();
        }
    }
}