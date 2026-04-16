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

namespace LondonFhirService.Core.Tests.Unit.Services.Processings.JsonIgnoreRules.Metas
{
    public partial class MetaIgnoreProcessingRuleTests
    {
        [Fact]
        public async Task ShouldThrowServiceExceptionOnShouldIgnoreIfServiceFailureOccursAsync()
        {
            // given
            JsonElement randomElement = new();
            string randomPath = GetRandomString();
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
                service.ValidateOnShouldIgnore(
                    randomElement,
                    randomPath))
                        .Throws(serviceException);

            // when
            ValueTask<bool> shouldIgnoreTask =
                metaIgnoreProcessingRuleMock.Object.ShouldIgnoreAsync(
                    randomElement,
                    randomPath);

            JsonIgnoreRulesProcessingServiceException actualJsonIgnoreRulesProcessingServiceException =
                await Assert.ThrowsAsync<JsonIgnoreRulesProcessingServiceException>(
                    shouldIgnoreTask.AsTask);

            // then
            actualJsonIgnoreRulesProcessingServiceException.Should()
                .BeEquivalentTo(expectedJsonIgnoreRulesProcessingServiceException);

            metaIgnoreProcessingRuleMock.Verify(service =>
                service.ValidateOnShouldIgnore(
                    randomElement,
                    randomPath),
                        Times.Once);

            metaIgnoreProcessingRuleMock.Verify(service =>
                service.ShouldIgnoreAsync(
                    randomElement,
                    randomPath),
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