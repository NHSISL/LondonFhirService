// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers.Exceptions;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.Appointments;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Appointments
{
    public partial class AppointmentMatcherServiceTests
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
                    message: "Failed appointment matcher service occurred, please contact support",
                    innerException: serviceException);

            var expectedResourceMatcherServiceException =
                new ResourceMatcherServiceException(
                    message: "Appointment matcher service error occurred, contact support.",
                    innerException: failedResourceMatcherServiceException);

            var appointmentMatcherServiceMock = new Mock<AppointmentMatcherService>(loggingBrokerMock.Object)
                { CallBase = true };

            appointmentMatcherServiceMock.Setup(service =>
                service.ValidateOnGetMatchKeyArguments(
                    resource,
                    resourceIndex))
                        .Throws(serviceException);

            // when
            ValueTask<string> matchTask =
                appointmentMatcherServiceMock.Object.GetMatchKeyAsync(
                    resource,
                    resourceIndex);

            ResourceMatcherServiceException actualResourceMatcherServiceException =
                await Assert.ThrowsAsync<ResourceMatcherServiceException>(
                    matchTask.AsTask);

            // then
            actualResourceMatcherServiceException.Should()
                .BeEquivalentTo(expectedResourceMatcherServiceException);

            appointmentMatcherServiceMock.Verify(service =>
                service.ValidateOnGetMatchKeyArguments(
                    resource,
                    resourceIndex),
                        Times.Once);

            appointmentMatcherServiceMock.Verify(service =>
                service.GetMatchKeyAsync(
                    resource,
                    resourceIndex),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedResourceMatcherServiceException))),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            appointmentMatcherServiceMock.VerifyNoOtherCalls();
        }
    }
}
