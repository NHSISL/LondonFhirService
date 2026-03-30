// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers.Lists.Exceptions;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.Lists;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Lists
{
    public partial class ListMatcherServiceTests
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

            var failedListMatcherServiceException =
                new FailedListMatcherServiceException(
                    message: "Failed list matcher service occurred, please contact support",
                    innerException: serviceException);

            var expectedListMatcherServiceException =
                new ListMatcherServiceException(
                    message: "List matcher service error occurred, contact support.",
                    innerException: failedListMatcherServiceException);

            var listMatcherServiceMock = new Mock<ListMatcherService>(loggingBrokerMock.Object)
                { CallBase = true };

            listMatcherServiceMock.Setup(service =>
                service.ValidateOnMatchArguments(
                    invalidSource1Resources,
                    invalidSource2Resources,
                    invalidSource1ResourceIndex,
                    invalidSource2ResourceIndex))
                        .Throws(serviceException);

            // when
            ValueTask<ResourceMatch> matchTask =
                listMatcherServiceMock.Object.MatchAsync(
                    invalidSource1Resources,
                    invalidSource2Resources,
                    invalidSource1ResourceIndex,
                    invalidSource2ResourceIndex);

            ListMatcherServiceException actualListMatcherServiceException =
                await Assert.ThrowsAsync<ListMatcherServiceException>(
                    matchTask.AsTask);

            // then
            actualListMatcherServiceException.Should()
                .BeEquivalentTo(expectedListMatcherServiceException);

            listMatcherServiceMock.Verify(service =>
                service.ValidateOnMatchArguments(
                    invalidSource1Resources,
                    invalidSource2Resources,
                    invalidSource1ResourceIndex,
                    invalidSource2ResourceIndex),
                        Times.Once);

            listMatcherServiceMock.Verify(service =>
                service.MatchAsync(
                    invalidSource1Resources,
                    invalidSource2Resources,
                    invalidSource1ResourceIndex,
                    invalidSource2ResourceIndex),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedListMatcherServiceException))),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            listMatcherServiceMock.VerifyNoOtherCalls();
        }
    }
}