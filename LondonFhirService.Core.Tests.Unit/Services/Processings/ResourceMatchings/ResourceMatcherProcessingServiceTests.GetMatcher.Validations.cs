// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Models.Processings.ResourceMatchings.Exceptions;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Processings.ResourceMatchings
{
    public partial class ResourceMatcherProcessingServiceTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ShouldThrowValidationExceptionOnGetMatcherIfResourceTypeIsInvalidAndLogItAsync(
            string invalidResourceType)
        {
            // given
            var invalidArgumentResourceMatcherProcessingException =
                new InvalidArgumentResourceMatcherProcessingException(
                    message: "Invalid resource matcher processing arguments. " +
                        "Please correct the errors and try again.");

            invalidArgumentResourceMatcherProcessingException.AddData(
                key: "resourceType",
                values: "Text is invalid.");

            var expectedResourceMatcherProcessingValidationException =
                new ResourceMatcherProcessingValidationException(
                    message: "Resource matcher processing validation error occurred, " +
                        "please fix errors and try again.",
                    innerException: invalidArgumentResourceMatcherProcessingException);

            // when
            ValueTask<IResourceMatcherService?> getMatcherTask =
                this.resourceMatcherProcessingService.GetMatcherAsync(invalidResourceType);

            // then
            ResourceMatcherProcessingValidationException actualException =
                await Assert.ThrowsAsync<ResourceMatcherProcessingValidationException>(
                    getMatcherTask.AsTask);

            actualException.Should()
                .BeEquivalentTo(expectedResourceMatcherProcessingValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedResourceMatcherProcessingValidationException))),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
