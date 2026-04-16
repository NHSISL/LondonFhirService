// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers;
using LondonFhirService.Core.Services.Processings.ResourceMatchings;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Processings.ResourceMatchings
{
    public partial class ResourceMatcherProcessingServiceTests
    {
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly Mock<IResourceMatcherService> resourceMatcherServiceMock;
        private readonly ResourceMatcherProcessingService resourceMatcherProcessingService;

        public ResourceMatcherProcessingServiceTests()
        {
            this.loggingBrokerMock = new Mock<ILoggingBroker>();
            this.resourceMatcherServiceMock = new Mock<IResourceMatcherService>();

            this.resourceMatcherServiceMock
                .Setup(matcher => matcher.ResourceType)
                .Returns(GetRandomString());

            this.resourceMatcherProcessingService =
                new ResourceMatcherProcessingService(
                    matchers: new List<IResourceMatcherService>
                    {
                        this.resourceMatcherServiceMock.Object
                    },
                    loggingBroker: this.loggingBrokerMock.Object);
        }

        private static Expression<Func<Xeption, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => actualException.SameExceptionAs(expectedException);

        private static string GetRandomString() =>
            new MnemonicString().GetValue();
    }
}
