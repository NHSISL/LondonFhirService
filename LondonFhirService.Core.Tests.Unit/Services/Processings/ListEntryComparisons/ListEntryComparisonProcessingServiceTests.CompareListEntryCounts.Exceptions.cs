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

namespace LondonFhirService.Core.Tests.Unit.Services.Processings.ListEntryComparisons
{
    public partial class ArrayOrderIgnoreProcessingRuleTests
    {
        [Fact]
        public async Task ShouldThrowServiceExceptionOnCompareListEntryIfServiceFailureOccursAsync()
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
                service.ValidateOnShouldIgnore(
                    randomElement,
                    randomPath))
                        .Throws(serviceException);

            // when
            ValueTask<bool> shouldIgnoreTask =
                arrayOrderIgnoreProcessingRuleMock.Object.ShouldIgnoreAsync(
                    randomElement,
                    randomPath);

            ArrayOrderIgnoreProcessingServiceException actualArrayOrderIgnoreProcessingServiceException =
                await Assert.ThrowsAsync<ArrayOrderIgnoreProcessingServiceException>(
                    shouldIgnoreTask.AsTask);

            // then
            actualArrayOrderIgnoreProcessingServiceException.Should()
                .BeEquivalentTo(expectedArrayOrderIgnoreProcessingServiceException);

            arrayOrderIgnoreProcessingRuleMock.Verify(service =>
                service.ValidateOnShouldIgnore(
                    randomElement,
                    randomPath),
                        Times.Once);

            arrayOrderIgnoreProcessingRuleMock.Verify(service =>
                service.ShouldIgnoreAsync(
                    randomElement,
                    randomPath),
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