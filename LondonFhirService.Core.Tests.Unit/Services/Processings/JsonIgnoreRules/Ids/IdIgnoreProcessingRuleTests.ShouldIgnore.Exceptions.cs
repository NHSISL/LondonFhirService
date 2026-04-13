// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Processings.JsonIgnoreRules.IdIgnoreRules.Exceptions;
using LondonFhirService.Core.Services.Processings.JsonIgnoreRules;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Processings.JsonIgnoreRules.Ids
{
    public partial class IdIgnoreProcessingRuleTests
    {
        [Fact]
        public async Task ShouldThrowServiceExceptionOnShouldIgnoreIfServiceFailureOccursAsync()
        {
            // given
            JsonElement randomElement = new();
            string randomPath = GetRandomString();
            var serviceException = new Exception();

            var failedIdIgnoreProcessingException =
                new FailedIdIgnoreProcessingException(
                    message: "Failed id ignore processing exception occurred, please contact support",
                    innerException: serviceException,
                    data: serviceException.Data);

            var expectedIdIgnoreProcessingServiceException =
                new IdIgnoreProcessingServiceException(
                    message: "Id ignore processing service error occurred, contact support.",
                    innerException: failedIdIgnoreProcessingException);

            var idIgnoreProcessingRuleMock = new Mock<IdIgnoreProcessingRule>(
                jsonElementServiceMock.Object,
                loggingBrokerMock.Object)
            { CallBase = true };

            idIgnoreProcessingRuleMock.Setup(service =>
                service.ValidateOnShouldIgnore(
                    randomElement,
                    randomPath))
                        .Throws(serviceException);

            // when
            ValueTask<bool> shouldIgnoreTask =
                idIgnoreProcessingRuleMock.Object.ShouldIgnoreAsync(
                    randomElement,
                    randomPath);

            IdIgnoreProcessingServiceException actualIdIgnoreProcessingServiceException =
                await Assert.ThrowsAsync<IdIgnoreProcessingServiceException>(
                    shouldIgnoreTask.AsTask);

            // then
            actualIdIgnoreProcessingServiceException.Should()
                .BeEquivalentTo(expectedIdIgnoreProcessingServiceException);

            idIgnoreProcessingRuleMock.Verify(service =>
                service.ValidateOnShouldIgnore(
                    randomElement,
                    randomPath),
                        Times.Once);

            idIgnoreProcessingRuleMock.Verify(service =>
                service.ShouldIgnoreAsync(
                    randomElement,
                    randomPath),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedIdIgnoreProcessingServiceException))),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            idIgnoreProcessingRuleMock.VerifyNoOtherCalls();
            jsonElementServiceMock.VerifyNoOtherCalls();
        }
    }
}