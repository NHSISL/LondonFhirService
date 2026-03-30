// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Foundations.ResourceMatchers.AllergyIntolerances.Exceptions;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.AllergyIntolerances;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.AllergyIntolerances;

public partial class AllergyIntoleranceMatcherServiceTests
{
    [Fact]
    private async Task ShouldThrowServiceExceptionOnGetMatchKeysIfServiceErrorOccursAndLogItAsync()
    {
        // given
        JsonElement resource = new();
        Dictionary<string, JsonElement> resourceIndex = CreateResourceIndex();
        var serviceException = new Exception();

        var failedAllergyIntoleranceMatcherServiceException =
            new FailedAllergyIntoleranceMatcherServiceException(
                message: "Failed allergy intolerance matcher service occurred, please contact support",
                innerException: serviceException);

        var expectedAllergyIntoleranceMatcherServiceException =
            new AllergyIntoleranceMatcherServiceException(
                message: "Allergy intolerance matcher service error occurred, contact support.",
                innerException: failedAllergyIntoleranceMatcherServiceException);

        var allergyIntoleranceMatcherServiceMock = new Mock<AllergyIntoleranceMatcherService>(loggingBrokerMock.Object)
        { CallBase = true };

        allergyIntoleranceMatcherServiceMock.Setup(service =>
            service.ValidateOnGetMatchKeyArguments(
                resource,
                resourceIndex))
                    .Throws(serviceException);

        // when
        ValueTask<string> matchTask =
            allergyIntoleranceMatcherServiceMock.Object.GetMatchKeyAsync(
                resource,
                resourceIndex);

        AllergyIntoleranceMatcherServiceException actualAllergyIntoleranceMatcherServiceException =
            await Assert.ThrowsAsync<AllergyIntoleranceMatcherServiceException>(
                matchTask.AsTask);

        // then
        actualAllergyIntoleranceMatcherServiceException.Should()
            .BeEquivalentTo(expectedAllergyIntoleranceMatcherServiceException);

        allergyIntoleranceMatcherServiceMock.Verify(service =>
            service.ValidateOnGetMatchKeyArguments(
                resource,
                resourceIndex),
                    Times.Once);

        allergyIntoleranceMatcherServiceMock.Verify(service =>
            service.GetMatchKeyAsync(
                resource,
                resourceIndex),
                    Times.Once);

        this.loggingBrokerMock.Verify(broker =>
            broker.LogErrorAsync(It.Is(SameExceptionAs(
                expectedAllergyIntoleranceMatcherServiceException))),
                    Times.Once);

        this.loggingBrokerMock.VerifyNoOtherCalls();
        allergyIntoleranceMatcherServiceMock.VerifyNoOtherCalls();
    }
}