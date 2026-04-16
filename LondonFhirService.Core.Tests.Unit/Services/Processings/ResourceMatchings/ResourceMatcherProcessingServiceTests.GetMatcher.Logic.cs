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
        public async Task ShouldGetMatcherAsync()
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

            IResourceMatcherService expectedMatcher = matcherMock.Object;

            // when
            IResourceMatcherService? actualMatcher =
                await service.GetMatcherAsync(randomResourceType);

            // then
            actualMatcher.Should().BeSameAs(expectedMatcher);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldReturnNullOnGetMatcherWhenNoMatcherIsFoundAsync()
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

            IResourceMatcherService? expectedMatcher = null;

            // when
            IResourceMatcherService? actualMatcher =
                await service.GetMatcherAsync(unknownResourceType);

            // then
            actualMatcher.Should().BeSameAs(expectedMatcher);
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
