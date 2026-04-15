// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Processings.JsonIgnoreRules.ArrayOrderIgnoreRules.Exceptions;
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

            var expectedJsonIgnoreRulesProcessingDependencyException =
                new JsonIgnoreRulesProcessingDependencyException(
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

            JsonIgnoreRulesProcessingDependencyException actualJsonIgnoreRulesProcessingDependencyException =
                await Assert.ThrowsAsync<JsonIgnoreRulesProcessingDependencyException>(
                    testCode: getReplacementTask.AsTask);

            // then
            actualJsonIgnoreRulesProcessingDependencyException
                .Should().BeEquivalentTo(expectedJsonIgnoreRulesProcessingDependencyException);

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
                   expectedJsonIgnoreRulesProcessingDependencyException))),
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

            var expectedJsonIgnoreRulesProcessingDependencyValidationException =
                new JsonIgnoreRulesProcessingDependencyValidationException(
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

            JsonIgnoreRulesProcessingDependencyValidationException
                actualJsonIgnoreRulesProcessingDependencyValidationException =
                    await Assert.ThrowsAsync<JsonIgnoreRulesProcessingDependencyValidationException>(
                        testCode: getReplacementTask.AsTask);

            // then
            actualJsonIgnoreRulesProcessingDependencyValidationException
                .Should().BeEquivalentTo(expectedJsonIgnoreRulesProcessingDependencyValidationException);

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
                   expectedJsonIgnoreRulesProcessingDependencyValidationException))),
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

            var failedJsonIgnoreRulesProcessingException =
                new FailedJsonIgnoreRulesProcessingException(
                    message: "Failed guid ignore processing exception occurred, please contact support",
                    innerException: serviceException,
                    data: serviceException.Data);

            var expectedJsonIgnoreRulesProcessingServiceException =
                new JsonIgnoreRulesProcessingServiceException(
                    message: "Guid ignore processing service error occurred, contact support.",
                    innerException: failedJsonIgnoreRulesProcessingException);

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

            JsonIgnoreRulesProcessingServiceException actualJsonIgnoreRulesProcessingServiceException =
                await Assert.ThrowsAsync<JsonIgnoreRulesProcessingServiceException>(
                    getReplacementTask.AsTask);

            // then
            actualJsonIgnoreRulesProcessingServiceException.Should()
                .BeEquivalentTo(expectedJsonIgnoreRulesProcessingServiceException);

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
                    expectedJsonIgnoreRulesProcessingServiceException))),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            guidIgnoreProcessingRuleMock.VerifyNoOtherCalls();
            jsonElementServiceMock.VerifyNoOtherCalls();
        }
    }
}