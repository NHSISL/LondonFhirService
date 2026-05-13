// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers.Exceptions;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.Practitioners;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Practitioners
{
    public partial class PractitionerMatcherServiceTests
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
                    message: "Failed practitioner matcher service occurred, please contact support",
                    innerException: serviceException);

            var expectedResourceMatcherServiceException =
                new ResourceMatcherServiceException(
                    message: "Practitioner matcher service error occurred, contact support.",
                    innerException: failedResourceMatcherServiceException);

            var practitionerMatcherServiceMock = new Mock<PractitionerMatcherService>(loggingBrokerMock.Object)
                { CallBase = true };

            practitionerMatcherServiceMock.Setup(service =>
                service.ValidateOnGetMatchKeyArguments(
                    resource,
                    resourceIndex))
                        .Throws(serviceException);

            // when
            ValueTask<string> matchTask =
                practitionerMatcherServiceMock.Object.GetMatchKeyAsync(
                    resource,
                    resourceIndex);

            ResourceMatcherServiceException actualResourceMatcherServiceException =
                await Assert.ThrowsAsync<ResourceMatcherServiceException>(
                    matchTask.AsTask);

            // then
            actualResourceMatcherServiceException.Should()
                .BeEquivalentTo(expectedResourceMatcherServiceException);

            practitionerMatcherServiceMock.Verify(service =>
                service.ValidateOnGetMatchKeyArguments(
                    resource,
                    resourceIndex),
                        Times.Once);

            practitionerMatcherServiceMock.Verify(service =>
                service.GetMatchKeyAsync(
                    resource,
                    resourceIndex),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedResourceMatcherServiceException))),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            practitionerMatcherServiceMock.VerifyNoOtherCalls();
        }
    }
}
