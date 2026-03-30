// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers.Conditions.Exceptions;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.Conditions;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Conditions
{
    public partial class ConditionMatcherServiceTests
    {
        [Fact]
        public async Task ShouldThrowServiceExceptionOnMatchIfServiceErrorOccursAndLogItAsync()
        {
            // given
            List<JsonElement> invalidSource1Resources = new();
            List<JsonElement> invalidSource2Resources = new();
            Dictionary<string, JsonElement> invalidSource1ResourceIndex = CreateResourceIndex();
            Dictionary<string, JsonElement> invalidSource2ResourceIndex = CreateResourceIndex();
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
                service.ValidateOnMatchArguments(
                    invalidSource1Resources,
                    invalidSource2Resources,
                    invalidSource1ResourceIndex,
                    invalidSource2ResourceIndex))
                        .Throws(serviceException);

            // when
            ValueTask<ResourceMatch> matchTask =
                conditionMatcherServiceMock.Object.MatchAsync(
                    invalidSource1Resources,
                    invalidSource2Resources,
                    invalidSource1ResourceIndex,
                    invalidSource2ResourceIndex);

            ConditionMatcherServiceException actualConditionMatcherServiceException =
                await Assert.ThrowsAsync<ConditionMatcherServiceException>(
                    matchTask.AsTask);

            // then
            actualConditionMatcherServiceException.Should()
                .BeEquivalentTo(expectedConditionMatcherServiceException);

            conditionMatcherServiceMock.Verify(service =>
                service.ValidateOnMatchArguments(
                    invalidSource1Resources,
                    invalidSource2Resources,
                    invalidSource1ResourceIndex,
                    invalidSource2ResourceIndex),
                        Times.Once);

            conditionMatcherServiceMock.Verify(service =>
                service.MatchAsync(
                    invalidSource1Resources,
                    invalidSource2Resources,
                    invalidSource1ResourceIndex,
                    invalidSource2ResourceIndex),
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