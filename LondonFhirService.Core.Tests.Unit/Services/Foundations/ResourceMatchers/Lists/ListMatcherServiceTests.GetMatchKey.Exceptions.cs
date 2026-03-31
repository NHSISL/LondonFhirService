// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers.Lists.Exceptions;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.Lists;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Lists
{
    public partial class ListMatcherServiceTests
    {
        [Fact]
        public async Task ShouldThrowServiceExceptionOnGetMatchKeysIfServiceErrorOccursAndLogItAsync()
        {
            // given
            JsonElement resource = new();
            Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();
            var serviceException = new Exception();

            var failedListMatcherServiceException =
                new FailedListMatcherServiceException(
                    message: "Failed list matcher service occurred, please contact support",
                    innerException: serviceException);

            var expectedListMatcherServiceException =
                new ListMatcherServiceException(
                    message: "List matcher service error occurred, contact support.",
                    innerException: failedListMatcherServiceException);

            var listMatcherServiceMock = 
                new Mock<ListMatcherService>(loggingBrokerMock.Object) { CallBase = true };

            listMatcherServiceMock.Setup(service =>
                service.ValidateOnGetMatchKeyArguments(
                    resource,
                    resourceIndex))
                        .Throws(serviceException);

            // when
            ValueTask<string> matchTask =
                listMatcherServiceMock.Object.GetMatchKeyAsync(
                    resource,
                    resourceIndex);

            ListMatcherServiceException actualListMatcherServiceException =
                await Assert.ThrowsAsync<ListMatcherServiceException>(
                    matchTask.AsTask);

            // then
            actualListMatcherServiceException.Should()
                .BeEquivalentTo(expectedListMatcherServiceException);

            listMatcherServiceMock.Verify(service =>
                service.ValidateOnGetMatchKeyArguments(
                    resource,
                    resourceIndex),
                        Times.Once);

            listMatcherServiceMock.Verify(service =>
                service.GetMatchKeyAsync(
                    resource,
                    resourceIndex),
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