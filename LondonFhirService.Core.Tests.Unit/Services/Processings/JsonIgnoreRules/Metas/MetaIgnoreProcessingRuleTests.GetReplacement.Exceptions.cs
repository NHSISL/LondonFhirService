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

namespace LondonFhirService.Core.Tests.Unit.Services.Processings.JsonIgnoreRules.Metas
{
    public partial class MetaIgnoreProcessingRuleTests
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
                    message: "Meta ignore processing dependency error occurred, contact support.",
                    innerException: dependencyException);

            var metaIgnoreProcessingRuleMock = new Mock<MetaIgnoreProcessingRule>(
                jsonElementServiceMock.Object,
                loggingBrokerMock.Object)
            { CallBase = true };

            metaIgnoreProcessingRuleMock.Setup(service =>
                service.ValidateOnGetReplacement(
                    It.IsAny<JsonElement>()))
                        .Throws(dependencyException);

            // when
            ValueTask<JsonElement> getReplacementTask =
                metaIgnoreProcessingRuleMock.Object.GetReplacementAsync(randomElement);

            JsonIgnoreRulesProcessingDependencyException actualJsonIgnoreRulesProcessingDependencyException =
                await Assert.ThrowsAsync<JsonIgnoreRulesProcessingDependencyException>(
                    testCode: getReplacementTask.AsTask);

            // then
            actualJsonIgnoreRulesProcessingDependencyException
                .Should().BeEquivalentTo(expectedJsonIgnoreRulesProcessingDependencyException);

            metaIgnoreProcessingRuleMock.Verify(service =>
                service.ValidateOnGetReplacement(
                    randomElement),
                        Times.Once);

            metaIgnoreProcessingRuleMock.Verify(service =>
                service.GetReplacementAsync(
                    randomElement),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
               broker.LogErrorAsync(It.Is(SameExceptionAs(
                   expectedJsonIgnoreRulesProcessingDependencyException))),
                       Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            metaIgnoreProcessingRuleMock.VerifyNoOtherCalls();
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
                    message: "Meta ignore processing dependency validation error occurred, contact support.",
                    innerException: dependencyValidationException);

            var metaIgnoreProcessingRuleMock = new Mock<MetaIgnoreProcessingRule>(
                jsonElementServiceMock.Object,
                loggingBrokerMock.Object)
            { CallBase = true };

            metaIgnoreProcessingRuleMock.Setup(service =>
                service.ValidateOnGetReplacement(
                    It.IsAny<JsonElement>()))
                        .Throws(dependencyValidationException);

            // when
            ValueTask<JsonElement> getReplacementTask =
                metaIgnoreProcessingRuleMock.Object.GetReplacementAsync(randomElement);

            JsonIgnoreRulesProcessingDependencyValidationException
                actualJsonIgnoreRulesProcessingDependencyValidationException =
                    await Assert.ThrowsAsync<JsonIgnoreRulesProcessingDependencyValidationException>(
                        testCode: getReplacementTask.AsTask);

            // then
            actualJsonIgnoreRulesProcessingDependencyValidationException
                .Should().BeEquivalentTo(expectedJsonIgnoreRulesProcessingDependencyValidationException);

            metaIgnoreProcessingRuleMock.Verify(service =>
                service.ValidateOnGetReplacement(
                    randomElement),
                        Times.Once);

            metaIgnoreProcessingRuleMock.Verify(service =>
                service.GetReplacementAsync(
                    randomElement),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
               broker.LogErrorAsync(It.Is(SameExceptionAs(
                   expectedJsonIgnoreRulesProcessingDependencyValidationException))),
                       Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            metaIgnoreProcessingRuleMock.VerifyNoOtherCalls();
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
                    message: "Failed meta ignore processing exception occurred, please contact support",
                    innerException: serviceException,
                    data: serviceException.Data);

            var expectedJsonIgnoreRulesProcessingServiceException =
                new JsonIgnoreRulesProcessingServiceException(
                    message: "Meta ignore processing service error occurred, contact support.",
                    innerException: failedJsonIgnoreRulesProcessingException);

            var metaIgnoreProcessingRuleMock = new Mock<MetaIgnoreProcessingRule>(
                jsonElementServiceMock.Object,
                loggingBrokerMock.Object)
            { CallBase = true };

            metaIgnoreProcessingRuleMock.Setup(service =>
                service.ValidateOnGetReplacement(
                    randomElement))
                        .Throws(serviceException);

            // when
            ValueTask<JsonElement> getReplacementTask =
                metaIgnoreProcessingRuleMock.Object.GetReplacementAsync(
                    randomElement);

            JsonIgnoreRulesProcessingServiceException actualJsonIgnoreRulesProcessingServiceException =
                await Assert.ThrowsAsync<JsonIgnoreRulesProcessingServiceException>(
                    getReplacementTask.AsTask);

            // then
            actualJsonIgnoreRulesProcessingServiceException.Should()
                .BeEquivalentTo(expectedJsonIgnoreRulesProcessingServiceException);

            metaIgnoreProcessingRuleMock.Verify(service =>
                service.ValidateOnGetReplacement(
                    randomElement),
                        Times.Once);

            metaIgnoreProcessingRuleMock.Verify(service =>
                service.GetReplacementAsync(
                    randomElement),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedJsonIgnoreRulesProcessingServiceException))),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            metaIgnoreProcessingRuleMock.VerifyNoOtherCalls();
            jsonElementServiceMock.VerifyNoOtherCalls();
        }
    }
}