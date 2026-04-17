// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Models.Processings.JsonIgnoreRules.ArrayOrderIgnoreRules.Exceptions;
using LondonFhirService.Core.Models.Processings.ListEntryComparisons.Exceptions;
using LondonFhirService.Core.Models.Processings.ResourceMatchings.Exceptions;
using LondonFhirService.Core.Services.Foundations.JsonElements;
using LondonFhirService.Core.Services.Orchestrations.Comparisons;
using LondonFhirService.Core.Services.Processings.JsonIgnoreRules;
using LondonFhirService.Core.Services.Processings.ListEntryComparisons;
using LondonFhirService.Core.Services.Processings.ResourceMatchings;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Orchestrations.Comparisons
{
    public partial class ComparisonOrchestrationServiceTests
    {
        private readonly Mock<IResourceMatcherProcessingService> resourceMatcherProcessingServiceMock;
        private readonly Mock<IListEntryComparisonProcessingService> listEntryComparisonProcessingServiceMock;
        private readonly Mock<IJsonElementService> jsonElementServiceMock;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly IComparisonOrchestrationService comparisonOrchestrationService;

        public ComparisonOrchestrationServiceTests()
        {
            this.resourceMatcherProcessingServiceMock = new Mock<IResourceMatcherProcessingService>();
            this.listEntryComparisonProcessingServiceMock = new Mock<IListEntryComparisonProcessingService>();
            this.jsonElementServiceMock = new Mock<IJsonElementService>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.comparisonOrchestrationService = new ComparisonOrchestrationService(
                ignoreRules: new List<IJsonIgnoreProcessingRule>(),
                resourceMatcherProcessingService: this.resourceMatcherProcessingServiceMock.Object,
                listEntryComparisonProcessingService: this.listEntryComparisonProcessingServiceMock.Object,
                jsonElementService: this.jsonElementServiceMock.Object,
                loggingBroker: this.loggingBrokerMock.Object);
        }

        private static string GetRandomString() =>
            new MnemonicString().GetValue();

        private static string GetRandomJson() =>
            "{\"resourceType\":\"Bundle\",\"entry\":[]}";

        private static string GetRandomJsonWithResources() =>
            "{\"resourceType\":\"Bundle\"," +
            "\"entry\":[{\"resource\":{\"resourceType\":\"Patient\",\"id\":\"1\"}}]}";

        private static Expression<Func<Xeption, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => actualException.SameExceptionAs(expectedException);

        public static TheoryData<Xeption> DependencyValidationExceptions()
        {
            string randomMessage = GetRandomString();
            string exceptionMessage = randomMessage;
            var innerException = new Xeption(exceptionMessage);

            return new TheoryData<Xeption>
            {
                new ResourceMatcherProcessingValidationException(
                    message: "Resource matcher processing validation errors occurred, please try again.",
                    innerException),

                new ListEntryComparisonProcessingValidationException(
                    message: "List entry comparison processing validation errors occurred, please try again.",
                    innerException),

                new JsonIgnoreRulesProcessingValidationException(
                    message: "Json ignore rules processing validation errors occurred, please try again.",
                    innerException),

                new JsonIgnoreRulesProcessingDependencyValidationException(
                    message: "Json ignore rules processing dependency validation errors occurred, please try again.",
                    innerException)
            };
        }

        public static TheoryData<Xeption> DependencyExceptions()
        {
            string randomMessage = GetRandomString();
            string exceptionMessage = randomMessage;
            var innerException = new Xeption(exceptionMessage);

            return new TheoryData<Xeption>
            {
                new ResourceMatcherProcessingServiceException(
                    message: "Resource matcher processing service error occurred, please contact support.",
                    innerException),

                new ListEntryComparisonProcessingServiceException(
                    message: "List entry comparison processing service error occurred, please contact support.",
                    innerException),

                new JsonIgnoreRulesProcessingServiceException(
                    message: "Json ignore rules processing service error occurred, please contact support.",
                    innerException),

                new JsonIgnoreRulesProcessingDependencyException(
                    message: "Json ignore rules processing dependency error occurred, please contact support.",
                    innerException)
            };
        }
    }
}
