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

namespace LondonFhirService.Core.Tests.Unit.Services.Processings.JsonIgnoreRules.ArrayOrders
{
    public partial class ArrayOrderIgnoreProcessingRuleTests
    {
        [Theory]
        [MemberData(nameof(DependencyExceptions))]
        public async Task ShouldThrowDependencyExceptionOnGetReplacementAndLogItAsync(
            Xeption dependencyException)
        {
            // given
            JsonElement randomElement = new();

            var expectedArrayOrderIgnoreProcessingDependencyException =
                new ArrayOrderIgnoreProcessingDependencyException(
                    message: "Array order ignore processing dependency error occurred, contact support.",
                    innerException: dependencyException);

            var arrayOrderIgnoreProcessingRuleMock = new Mock<ArrayOrderIgnoreProcessingRule>(
                jsonElementServiceMock.Object,
                loggingBrokerMock.Object)
            { CallBase = true };

            arrayOrderIgnoreProcessingRuleMock.Setup(service =>
                service.ValidateOnGetReplacement(
                    It.IsAny<JsonElement>()))
                        .Throws(dependencyException);

            // when
            ValueTask<JsonElement> getReplacementTask =
                arrayOrderIgnoreProcessingRuleMock.Object.GetReplacementAsync(randomElement);

            ArrayOrderIgnoreProcessingDependencyException actualArrayOrderIgnoreProcessingDependencyException =
                await Assert.ThrowsAsync<ArrayOrderIgnoreProcessingDependencyException>(
                    testCode: getReplacementTask.AsTask);

            // then
            actualArrayOrderIgnoreProcessingDependencyException
                .Should().BeEquivalentTo(expectedArrayOrderIgnoreProcessingDependencyException);

            arrayOrderIgnoreProcessingRuleMock.Verify(service =>
                service.ValidateOnGetReplacement(
                    randomElement),
                        Times.Once);

            arrayOrderIgnoreProcessingRuleMock.Verify(service =>
                service.GetReplacementAsync(
                    randomElement),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
               broker.LogErrorAsync(It.Is(SameExceptionAs(
                   expectedArrayOrderIgnoreProcessingDependencyException))),
                       Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            arrayOrderIgnoreProcessingRuleMock.VerifyNoOtherCalls();
            jsonElementServiceMock.VerifyNoOtherCalls();
        }

        [Theory]
        [MemberData(nameof(DependencyValidationExceptions))]
        public async Task ShouldThrowDependencyValidationOnGetReplacementAndLogItAsync(
            Xeption dependencyValidationException)
        {
            // given
            JsonElement randomElement = new();

            var expectedArrayOrderIgnoreProcessingDependencyValidationException =
                new ArrayOrderIgnoreProcessingDependencyValidationException(
                    message: "Array order ignore processing dependency validation error occurred, contact support.",
                    innerException: dependencyValidationException);

            var arrayOrderIgnoreProcessingRuleMock = new Mock<ArrayOrderIgnoreProcessingRule>(
                jsonElementServiceMock.Object,
                loggingBrokerMock.Object)
            { CallBase = true };

            arrayOrderIgnoreProcessingRuleMock.Setup(service =>
                service.ValidateOnGetReplacement(
                    It.IsAny<JsonElement>()))
                        .Throws(dependencyValidationException);

            // when
            ValueTask<JsonElement> getReplacementTask =
                arrayOrderIgnoreProcessingRuleMock.Object.GetReplacementAsync(randomElement);

            ArrayOrderIgnoreProcessingDependencyValidationException actualArrayOrderIgnoreProcessingDependencyValidationException =
                await Assert.ThrowsAsync<ArrayOrderIgnoreProcessingDependencyValidationException>(
                    testCode: getReplacementTask.AsTask);

            // then
            actualArrayOrderIgnoreProcessingDependencyValidationException
                .Should().BeEquivalentTo(expectedArrayOrderIgnoreProcessingDependencyValidationException);

            arrayOrderIgnoreProcessingRuleMock.Verify(service =>
                service.ValidateOnGetReplacement(
                    randomElement),
                        Times.Once);

            arrayOrderIgnoreProcessingRuleMock.Verify(service =>
                service.GetReplacementAsync(
                    randomElement),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
               broker.LogErrorAsync(It.Is(SameExceptionAs(
                   expectedArrayOrderIgnoreProcessingDependencyValidationException))),
                       Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            arrayOrderIgnoreProcessingRuleMock.VerifyNoOtherCalls();
            jsonElementServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnGetReplacementIfResourceIsInvalidAsync()
        {
            // given
            JsonElement randomElement = new();
            string randomPath = GetRandomString();
            var serviceException = new Exception();

            var failedArrayOrderIgnoreProcessingException =
                new FailedArrayOrderIgnoreProcessingException(
                    message: "Failed array order ignore processing exception occurred, please contact support",
                    innerException: serviceException,
                    data: serviceException.Data);

            var expectedArrayOrderIgnoreProcessingServiceException =
                new ArrayOrderIgnoreProcessingServiceException(
                    message: "Array order ignore processing service error occurred, contact support.",
                    innerException: failedArrayOrderIgnoreProcessingException);

            var arrayOrderIgnoreProcessingRuleMock = new Mock<ArrayOrderIgnoreProcessingRule>(
                jsonElementServiceMock.Object,
                loggingBrokerMock.Object)
            { CallBase = true };

            arrayOrderIgnoreProcessingRuleMock.Setup(service =>
                service.ValidateOnGetReplacement(
                    randomElement))
                        .Throws(serviceException);

            // when
            ValueTask<JsonElement> getReplacementTask =
                arrayOrderIgnoreProcessingRuleMock.Object.GetReplacementAsync(
                    randomElement);

            ArrayOrderIgnoreProcessingServiceException actualArrayOrderIgnoreProcessingServiceException =
                await Assert.ThrowsAsync<ArrayOrderIgnoreProcessingServiceException>(
                    getReplacementTask.AsTask);

            // then
            actualArrayOrderIgnoreProcessingServiceException.Should()
                .BeEquivalentTo(expectedArrayOrderIgnoreProcessingServiceException);

            arrayOrderIgnoreProcessingRuleMock.Verify(service =>
                service.ValidateOnGetReplacement(
                    randomElement),
                        Times.Once);

            arrayOrderIgnoreProcessingRuleMock.Verify(service =>
                service.GetReplacementAsync(
                    randomElement),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedArrayOrderIgnoreProcessingServiceException))),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            arrayOrderIgnoreProcessingRuleMock.VerifyNoOtherCalls();
            jsonElementServiceMock.VerifyNoOtherCalls();
        }
    }
}