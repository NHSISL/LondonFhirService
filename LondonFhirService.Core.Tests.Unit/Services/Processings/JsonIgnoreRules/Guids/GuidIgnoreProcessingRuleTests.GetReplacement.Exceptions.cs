// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Processings.JsonIgnoreRules.GuidIgnoreRules.Exceptions;
using LondonFhirService.Core.Services.Processings.JsonIgnoreRules;
using Moq;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Processings.JsonIgnoreRules.Guids
{
    public partial class GuidIgnoreProcessingRuleTests
    {
        [Theory]
        [MemberData(nameof(DependencyExceptions))]
        public async Task ShouldThrowDependencyExceptionOnGetReplacementAndLogItAsync(
            Xeption dependencyException)
        {
            // given
            JsonElement randomElement = new();

            var expectedGuidIgnoreProcessingDependencyException =
                new GuidIgnoreProcessingDependencyException(
                    message: "Guid ignore processing dependency error occurred, contact support.",
                    innerException: dependencyException);

            var guidIgnoreProcessingRuleMock = new Mock<GuidIgnoreProcessingRule>(
                jsonElementServiceMock.Object,
                loggingBrokerMock.Object)
            { CallBase = true };

            guidIgnoreProcessingRuleMock.Setup(service =>
                service.ValidateOnGetReplacement(
                    It.IsAny<JsonElement>()))
                        .Throws(dependencyException);

            // when
            ValueTask<JsonElement> getReplacementTask =
                guidIgnoreProcessingRuleMock.Object.GetReplacementAsync(randomElement);

            GuidIgnoreProcessingDependencyException actualGuidIgnoreProcessingDependencyException =
                await Assert.ThrowsAsync<GuidIgnoreProcessingDependencyException>(
                    testCode: getReplacementTask.AsTask);

            // then
            actualGuidIgnoreProcessingDependencyException
                .Should().BeEquivalentTo(expectedGuidIgnoreProcessingDependencyException);

            guidIgnoreProcessingRuleMock.Verify(service =>
                service.ValidateOnGetReplacement(
                    randomElement),
                        Times.Once);

            guidIgnoreProcessingRuleMock.Verify(service =>
                service.GetReplacementAsync(
                    randomElement),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
               broker.LogErrorAsync(It.Is(SameExceptionAs(
                   expectedGuidIgnoreProcessingDependencyException))),
                       Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            guidIgnoreProcessingRuleMock.VerifyNoOtherCalls();
            jsonElementServiceMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(DependencyValidationExceptions))]
        public async Task ShouldThrowDependencyValidationOnGetReplacementAndLogItAsync(
            Xeption dependencyValidationException)
        {
            // given
            JsonElement randomElement = new();

            var expectedGuidIgnoreProcessingDependencyValidationException =
                new GuidIgnoreProcessingDependencyValidationException(
                    message: "Guid ignore processing dependency validation error occurred, contact support.",
                    innerException: dependencyValidationException);

            var guidIgnoreProcessingRuleMock = new Mock<GuidIgnoreProcessingRule>(
                jsonElementServiceMock.Object,
                loggingBrokerMock.Object)
            { CallBase = true };

            guidIgnoreProcessingRuleMock.Setup(service =>
                service.ValidateOnGetReplacement(
                    It.IsAny<JsonElement>()))
                        .Throws(dependencyValidationException);

            // when
            ValueTask<JsonElement> getReplacementTask =
                guidIgnoreProcessingRuleMock.Object.GetReplacementAsync(randomElement);

            GuidIgnoreProcessingDependencyValidationException
                actualGuidIgnoreProcessingDependencyValidationException =
                    await Assert.ThrowsAsync<GuidIgnoreProcessingDependencyValidationException>(
                        testCode: getReplacementTask.AsTask);

            // then
            actualGuidIgnoreProcessingDependencyValidationException
                .Should().BeEquivalentTo(expectedGuidIgnoreProcessingDependencyValidationException);

            guidIgnoreProcessingRuleMock.Verify(service =>
                service.ValidateOnGetReplacement(
                    randomElement),
                        Times.Once);

            guidIgnoreProcessingRuleMock.Verify(service =>
                service.GetReplacementAsync(
                    randomElement),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
               broker.LogErrorAsync(It.Is(SameExceptionAs(
                   expectedGuidIgnoreProcessingDependencyValidationException))),
                       Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            guidIgnoreProcessingRuleMock.VerifyNoOtherCalls();
            jsonElementServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnGetReplacementIfResourceIsInvalidAsync()
        {
            // given
            JsonElement randomElement = new();
            var serviceException = new Exception();

            var failedGuidIgnoreProcessingException =
                new FailedGuidIgnoreProcessingException(
                    message: "Failed guid ignore processing exception occurred, please contact support",
                    innerException: serviceException,
                    data: serviceException.Data);

            var expectedGuidIgnoreProcessingServiceException =
                new GuidIgnoreProcessingServiceException(
                    message: "Guid ignore processing service error occurred, contact support.",
                    innerException: failedGuidIgnoreProcessingException);

            var guidIgnoreProcessingRuleMock = new Mock<GuidIgnoreProcessingRule>(
                jsonElementServiceMock.Object,
                loggingBrokerMock.Object)
            { CallBase = true };

            guidIgnoreProcessingRuleMock.Setup(service =>
                service.ValidateOnGetReplacement(
                    randomElement))
                        .Throws(serviceException);

            // when
            ValueTask<JsonElement> getReplacementTask =
                guidIgnoreProcessingRuleMock.Object.GetReplacementAsync(
                    randomElement);

            GuidIgnoreProcessingServiceException actualGuidIgnoreProcessingServiceException =
                await Assert.ThrowsAsync<GuidIgnoreProcessingServiceException>(
                    getReplacementTask.AsTask);

            // then
            actualGuidIgnoreProcessingServiceException.Should()
                .BeEquivalentTo(expectedGuidIgnoreProcessingServiceException);

            guidIgnoreProcessingRuleMock.Verify(service =>
                service.ValidateOnGetReplacement(
                    randomElement),
                        Times.Once);

            guidIgnoreProcessingRuleMock.Verify(service =>
                service.GetReplacementAsync(
                    randomElement),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedGuidIgnoreProcessingServiceException))),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            guidIgnoreProcessingRuleMock.VerifyNoOtherCalls();
            jsonElementServiceMock.VerifyNoOtherCalls();
        }
    }
}