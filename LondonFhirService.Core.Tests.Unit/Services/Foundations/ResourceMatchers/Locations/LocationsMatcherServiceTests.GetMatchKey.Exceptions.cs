// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers.Exceptions;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.Locations;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Locations
{
    public partial class LocationMatcherServiceTests
    {
        [Fact]
        public async Task ShouldThrowServiceExceptionOnGetMatchKeyIfServiceErrorOccursAndLogItAsync()
        {
            // given
            JsonElement resource = new();
            Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();
            var serviceException = new Exception();

            var failedResourceMatcherServiceException =
                new FailedResourceMatcherServiceException(
                    message: "Failed location matcher service occurred, please contact support",
                    innerException: serviceException);

            var expectedResourceMatcherServiceException =
                new ResourceMatcherServiceException(
                    message: "Location matcher service error occurred, contact support.",
                    innerException: failedResourceMatcherServiceException);

            var locationMatcherServiceMock = new Mock<LocationMatcherService>(loggingBrokerMock.Object)
                { CallBase = true };

            locationMatcherServiceMock.Setup(service =>
                service.ValidateOnGetMatchKeyArguments(
                    resource,
                    resourceIndex))
                        .Throws(serviceException);

            // when
            ValueTask<string> matchTask =
                locationMatcherServiceMock.Object.GetMatchKeyAsync(
                    resource,
                    resourceIndex);

            ResourceMatcherServiceException actualResourceMatcherServiceException =
                await Assert.ThrowsAsync<ResourceMatcherServiceException>(
                    matchTask.AsTask);

            // then
            actualResourceMatcherServiceException.Should()
                .BeEquivalentTo(expectedResourceMatcherServiceException);

            locationMatcherServiceMock.Verify(service =>
                service.ValidateOnGetMatchKeyArguments(
                    resource,
                    resourceIndex),
                        Times.Once);

            locationMatcherServiceMock.Verify(service =>
                service.GetMatchKeyAsync(
                    resource,
                    resourceIndex),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedResourceMatcherServiceException))),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            locationMatcherServiceMock.VerifyNoOtherCalls();
        }
    }
}
