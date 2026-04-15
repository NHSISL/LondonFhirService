// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Processings.JsonIgnoreRules.MetaIgnoreRules.Exceptions;
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

            var failedMetaIgnoreProcessingException =
                new FailedMetaIgnoreProcessingException(
                    message: "Failed meta ignore processing exception occurred, please contact support",
                    innerException: serviceException,
                    data: serviceException.Data);

            var expectedMetaIgnoreProcessingServiceException =
                new MetaIgnoreProcessingServiceException(
                    message: "Meta ignore processing service error occurred, contact support.",
                    innerException: failedMetaIgnoreProcessingException);

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

            MetaIgnoreProcessingServiceException actualMetaIgnoreProcessingServiceException =
                await Assert.ThrowsAsync<MetaIgnoreProcessingServiceException>(
                    shouldIgnoreTask.AsTask);

            // then
            actualMetaIgnoreProcessingServiceException.Should()
                .BeEquivalentTo(expectedMetaIgnoreProcessingServiceException);

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
                    expectedMetaIgnoreProcessingServiceException))),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            metaIgnoreProcessingRuleMock.VerifyNoOtherCalls();
            jsonElementServiceMock.VerifyNoOtherCalls();
        }
    }
}