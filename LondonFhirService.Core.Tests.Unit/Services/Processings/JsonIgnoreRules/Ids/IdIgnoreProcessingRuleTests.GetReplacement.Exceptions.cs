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

            var expectedJsonIgnoreRulesProcessingDependencyException =
                new JsonIgnoreRulesProcessingDependencyException(
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

            JsonIgnoreRulesProcessingDependencyException actualJsonIgnoreRulesProcessingDependencyException =
                await Assert.ThrowsAsync<JsonIgnoreRulesProcessingDependencyException>(
                    testCode: getReplacementTask.AsTask);

            // then
            actualJsonIgnoreRulesProcessingDependencyException
                .Should().BeEquivalentTo(expectedJsonIgnoreRulesProcessingDependencyException);

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
                   expectedJsonIgnoreRulesProcessingDependencyException))),
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

            var expectedJsonIgnoreRulesProcessingDependencyValidationException =
                new JsonIgnoreRulesProcessingDependencyValidationException(
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

            JsonIgnoreRulesProcessingDependencyValidationException 
                actualJsonIgnoreRulesProcessingDependencyValidationException =
                    await Assert.ThrowsAsync<JsonIgnoreRulesProcessingDependencyValidationException>(
                        testCode: getReplacementTask.AsTask);

            // then
            actualJsonIgnoreRulesProcessingDependencyValidationException
                .Should().BeEquivalentTo(expectedJsonIgnoreRulesProcessingDependencyValidationException);

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
                   expectedJsonIgnoreRulesProcessingDependencyValidationException))),
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

            var failedJsonIgnoreRulesProcessingException =
                new FailedJsonIgnoreRulesProcessingException(
                    message: "Failed id ignore processing exception occurred, please contact support",
                    innerException: serviceException,
                    data: serviceException.Data);

            var expectedJsonIgnoreRulesProcessingServiceException =
                new JsonIgnoreRulesProcessingServiceException(
                    message: "Id ignore processing service error occurred, contact support.",
                    innerException: failedJsonIgnoreRulesProcessingException);

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

            JsonIgnoreRulesProcessingServiceException actualJsonIgnoreRulesProcessingServiceException =
                await Assert.ThrowsAsync<JsonIgnoreRulesProcessingServiceException>(
                    getReplacementTask.AsTask);

            // then
            actualJsonIgnoreRulesProcessingServiceException.Should()
                .BeEquivalentTo(expectedJsonIgnoreRulesProcessingServiceException);

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
                    expectedJsonIgnoreRulesProcessingServiceException))),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            idIgnoreProcessingRuleMock.VerifyNoOtherCalls();
            jsonElementServiceMock.VerifyNoOtherCalls();
        }
    }
}