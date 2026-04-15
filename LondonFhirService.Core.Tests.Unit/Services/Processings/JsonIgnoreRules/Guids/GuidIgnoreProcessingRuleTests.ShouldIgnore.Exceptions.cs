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

namespace LondonFhirService.Core.Tests.Unit.Services.Processings.JsonIgnoreRules.Guids
{
    public partial class GuidIgnoreProcessingRuleTests
    {
        [Fact]
        public async Task ShouldThrowServiceExceptionOnShouldIgnoreIfServiceFailureOccursAsync()
        {
            // given
            JsonElement randomElement = new();
            string randomPath = GetRandomString();
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
                service.ValidateOnShouldIgnore(
                    randomElement,
                    randomPath))
                        .Throws(serviceException);

            // when
            ValueTask<bool> shouldIgnoreTask =
                guidIgnoreProcessingRuleMock.Object.ShouldIgnoreAsync(
                    randomElement,
                    randomPath);

            GuidIgnoreProcessingServiceException actualGuidIgnoreProcessingServiceException =
                await Assert.ThrowsAsync<GuidIgnoreProcessingServiceException>(
                    shouldIgnoreTask.AsTask);

            // then
            actualGuidIgnoreProcessingServiceException.Should()
                .BeEquivalentTo(expectedGuidIgnoreProcessingServiceException);

            guidIgnoreProcessingRuleMock.Verify(service =>
                service.ValidateOnShouldIgnore(
                    randomElement,
                    randomPath),
                        Times.Once);

            guidIgnoreProcessingRuleMock.Verify(service =>
                service.ShouldIgnoreAsync(
                    randomElement,
                    randomPath),
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