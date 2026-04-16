// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers;
using LondonFhirService.Core.Services.Processings.ResourceMatchings;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Processings.ResourceMatchings
{
    public partial class ResourceMatcherProcessingServiceTests
    {
        [Fact]
        public async Task ShouldReturnTrueOnHasMatcherWhenMatcherExistsAsync()
        {
            // given
            string randomResourceType = GetRandomString();
            var matcherMock = new Mock<IResourceMatcherService>();

            matcherMock
                .Setup(matcher => matcher.ResourceType)
                .Returns(randomResourceType);

            var service = new ResourceMatcherProcessingService(
                matchers: new List<IResourceMatcherService> { matcherMock.Object },
                loggingBroker: this.loggingBrokerMock.Object);

            bool expectedResult = true;

            // when
            bool actualResult = await service.HasMatcherAsync(randomResourceType);

            // then
            actualResult.Should().Be(expectedResult);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnFalseOnHasMatcherWhenMatcherDoesNotExistAsync()
        {
            // given
            string registeredResourceType = GetRandomString();
            string unknownResourceType = GetRandomString();
            var matcherMock = new Mock<IResourceMatcherService>();

            matcherMock
                .Setup(matcher => matcher.ResourceType)
                .Returns(registeredResourceType);

            var service = new ResourceMatcherProcessingService(
                matchers: new List<IResourceMatcherService> { matcherMock.Object },
                loggingBroker: this.loggingBrokerMock.Object);

            bool expectedResult = false;

            // when
            bool actualResult = await service.HasMatcherAsync(unknownResourceType);

            // then
            actualResult.Should().Be(expectedResult);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
