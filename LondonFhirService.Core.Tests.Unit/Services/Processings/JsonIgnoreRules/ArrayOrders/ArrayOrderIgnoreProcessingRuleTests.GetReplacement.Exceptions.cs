// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
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
        [MemberData(nameof(DependencyValidationExceptions))]
        public async Task ShouldThrowDependencyValidationOnGetReplacementAndLogItAsync(
            Xeption dependencyException)
        {
            // given
            JsonElement randomElement = CreateNestedArrayElement(2);

            this.jsonElementServiceMock.Setup(service =>
                service.CreateObjectElement(It.IsAny<Dictionary<string, JsonElement>>()))
                    .ThrowsAsync(dependencyException);

            var expectedArrayOrderIgnoreProcessingDependencyException =
                new ArrayOrderIgnoreProcessingDependencyException(
                    message: "Array order ignore processing dependency error occurred, contact support.",
                    innerException: dependencyException.InnerException as Xeption);

            // when
            ValueTask<JsonElement> getReplacementTask =
                arrayOrderIgnoreProcessingRule.GetReplacementAsync(randomElement);

            ArrayOrderIgnoreProcessingDependencyException actualArrayOrderIgnoreProcessingDependencyException =
                await Assert.ThrowsAsync<ArrayOrderIgnoreProcessingDependencyException>(
                    testCode: getReplacementTask.AsTask);

            // then
            actualArrayOrderIgnoreProcessingDependencyException
                .Should().BeEquivalentTo(expectedArrayOrderIgnoreProcessingDependencyException);

            this.loggingBrokerMock.Verify(broker =>
               broker.LogErrorAsync(It.Is(SameExceptionAs(
                   expectedArrayOrderIgnoreProcessingDependencyException))),
                       Times.Once);
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

            var arrayOrderMatcherServiceMock = new Mock<ArrayOrderIgnoreProcessingRule>(
                jsonElementServiceMock.Object,
                loggingBrokerMock.Object)
            { CallBase = true };

            arrayOrderMatcherServiceMock.Setup(service =>
                service.ValidateOnShouldIgnore(
                    randomElement,
                    randomPath))
                        .Throws(serviceException);

            // when
            ValueTask<bool> shouldIgnoreTask =
                arrayOrderMatcherServiceMock.Object.ShouldIgnoreAsync(
                    randomElement,
                    randomPath);

            ArrayOrderIgnoreProcessingServiceException actualArrayOrderIgnoreProcessingServiceException =
                await Assert.ThrowsAsync<ArrayOrderIgnoreProcessingServiceException>(
                    shouldIgnoreTask.AsTask);

            // then
            actualArrayOrderIgnoreProcessingServiceException.Should()
                .BeEquivalentTo(expectedArrayOrderIgnoreProcessingServiceException);

            arrayOrderMatcherServiceMock.Verify(service =>
                service.ValidateOnShouldIgnore(
                    randomElement,
                    randomPath),
                        Times.Once);

            arrayOrderMatcherServiceMock.Verify(service =>
                service.ShouldIgnoreAsync(
                    randomElement,
                    randomPath),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedArrayOrderIgnoreProcessingServiceException))),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            arrayOrderMatcherServiceMock.VerifyNoOtherCalls();
            jsonElementServiceMock.VerifyNoOtherCalls();
        }
    }
}