// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Models.Foundations.JsonElements.Exceptions;
using LondonFhirService.Core.Services.Foundations.JsonElements;
using LondonFhirService.Core.Services.Processings.JsonIgnoreRules;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Processings.JsonIgnoreRules.Guids
{
    public partial class GuidIgnoreProcessingRuleTests
    {
        private readonly Mock<IJsonElementService> jsonElementServiceMock;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly GuidIgnoreProcessingRule guidIgnoreProcessingRule;

        public GuidIgnoreProcessingRuleTests()
        {
            this.jsonElementServiceMock = new Mock<IJsonElementService>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.guidIgnoreProcessingRule =
                new GuidIgnoreProcessingRule(
                    jsonElementService: this.jsonElementServiceMock.Object,
                    loggingBroker: this.loggingBrokerMock.Object);
        }
        private static string GetRandomString() =>
            new MnemonicString().GetValue();

        private static Expression<Func<Xeption, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => actualException.SameExceptionAs(expectedException);

        private static JsonElement ParseJsonElement(string json) =>
            JsonDocument.Parse(json).RootElement.Clone();

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static JsonElement CreateNestedArrayElement(int depth)
        {
            int count = GetRandomNumber();

            if (depth <= 1)
            {
                string leafJson = 
                    $"[{string.Join(",", Enumerable.Range(0, count).Select(_ => GetRandomNumber()))}]";
                    
                return ParseJsonElement(leafJson);
            }

            string json = $"[{string.Join(",", Enumerable.Range(0, count)
                .Select(_ => CreateNestedArrayElement(depth - 1).GetRawText()))}]";

            return ParseJsonElement(json);
        }


        public static TheoryData<Xeption> DependencyValidationExceptions()
        {
            string randomMessage = GetRandomString();
            string exceptionMessage = randomMessage;
            var innerException = new Xeption(exceptionMessage);

            return new TheoryData<Xeption>
            {
                new JsonElementServiceValidationException(
                    message: "Json element service validation error occurred, please contact support.",
                    innerException),

                new JsonElementServiceDependencyValidationException(
                    message: "Json element service dependency validation error occurred, please contact support.",
                    innerException),
            };
        }

        public static TheoryData<Xeption> DependencyExceptions()
        {
            string randomMessage = GetRandomString();
            string exceptionMessage = randomMessage;
            var innerException = new Xeption(exceptionMessage);

            return new TheoryData<Xeption>
            {
                new JsonElementServiceDependencyException(
                    message: "Json element service dependency error occurred, please try again",
                    innerException),

                new JsonElementServiceException(
                    message: "Json element service error occurred, please try again.",
                    innerException),
            };
        }
    }
}