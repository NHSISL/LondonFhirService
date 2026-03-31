// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers.Conditions.Exceptions;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.Conditions;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Conditions
{
    public partial class ConditionMatcherServiceTests
    {
        [Fact]
        public async Task ShouldThrowServiceExceptionOnGetMatchKeysIfServiceErrorOccursAndLogItAsync()
        {
            // given
            JsonElement resource = new();
            Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();
            var serviceException = new Exception();

            var failedConditionMatcherServiceException =
                new FailedConditionMatcherServiceException(
                    message: "Failed condition matcher service occurred, please contact support",
                    innerException: serviceException);

            var expectedConditionMatcherServiceException =
                new ConditionMatcherServiceException(
                    message: "Condition matcher service error occurred, contact support.",
                    innerException: failedConditionMatcherServiceException);

            var conditionMatcherServiceMock = new Mock<ConditionMatcherService>(loggingBrokerMock.Object)
                { CallBase = true };

            conditionMatcherServiceMock.Setup(service =>
                service.ValidateOnGetMatchKeyArguments(
                    resource,
                    resourceIndex))
                        .Throws(serviceException);

            // when
            ValueTask<string> matchTask =
                conditionMatcherServiceMock.Object.GetMatchKeyAsync(
                    resource,
                    resourceIndex);

            ConditionMatcherServiceException actualConditionMatcherServiceException =
                await Assert.ThrowsAsync<ConditionMatcherServiceException>(
                    matchTask.AsTask);

            // then
            actualConditionMatcherServiceException.Should()
                .BeEquivalentTo(expectedConditionMatcherServiceException);

            conditionMatcherServiceMock.Verify(service =>
                service.ValidateOnGetMatchKeyArguments(
                    resource,
                    resourceIndex),
                        Times.Once);

            conditionMatcherServiceMock.Verify(service =>
                service.GetMatchKeyAsync(
                    resource,
                    resourceIndex),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedConditionMatcherServiceException))),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            conditionMatcherServiceMock.VerifyNoOtherCalls();
        }
    }
}    