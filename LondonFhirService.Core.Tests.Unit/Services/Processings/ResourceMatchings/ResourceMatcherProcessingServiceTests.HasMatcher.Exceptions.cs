// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Processings.ResourceMatchings.Exceptions;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers;
using LondonFhirService.Core.Services.Processings.ResourceMatchings;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Processings.ResourceMatchings
{
    public partial class ResourceMatcherProcessingServiceTests
    {
        [Fact]
        public async Task ShouldThrowServiceExceptionOnHasMatcherIfServiceFailureOccursAndLogItAsync()
        {
            // given
            string randomResourceType = GetRandomString();
            var serviceException = new Exception();

            var failedResourceMatcherProcessingException =
                new FailedResourceMatcherProcessingException(
                    message: "Failed resource matcher processing exception occurred, please contact support",
                    innerException: serviceException,
                    data: serviceException.Data);

            var expectedResourceMatcherProcessingServiceException =
                new ResourceMatcherProcessingServiceException(
                    message: "Resource matcher processing service error occurred, contact support.",
                    innerException: failedResourceMatcherProcessingException);

            var resourceMatcherProcessingServiceMock =
                new Mock<ResourceMatcherProcessingService>(
                    new List<IResourceMatcherService>(),
                    loggingBrokerMock.Object)
                { CallBase = true };

            resourceMatcherProcessingServiceMock
                .Setup(service => service.ValidateResourceType(randomResourceType))
                .Throws(serviceException);

            // when
            ValueTask<bool> hasMatcherTask =
                resourceMatcherProcessingServiceMock.Object.HasMatcherAsync(randomResourceType);

            // then
            ResourceMatcherProcessingServiceException actualException =
                await Assert.ThrowsAsync<ResourceMatcherProcessingServiceException>(
                    hasMatcherTask.AsTask);

            actualException.Should()
                .BeEquivalentTo(expectedResourceMatcherProcessingServiceException);

            resourceMatcherProcessingServiceMock.Verify(service =>
                service.ValidateResourceType(randomResourceType),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedResourceMatcherProcessingServiceException))),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            resourceMatcherProcessingServiceMock.VerifyNoOtherCalls();
        }
    }
}
