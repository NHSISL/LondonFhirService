// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Models.Foundations.ConsumerAccesses.Exceptions;
using LondonFhirService.Core.Models.Foundations.Consumers.Exceptions;
using LondonFhirService.Core.Models.Foundations.PdsDatas.Exceptions;
using LondonFhirService.Core.Services.Foundations.JsonElements;
using LondonFhirService.Core.Services.Processings.JsonIgnoreRules;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Processings.JsonIgnoreRules.ArrayOrders
{
    public partial class ArrayOrderIgnoreProcessingRuleTests
    {
        private readonly Mock<JsonElementService> jsonElementServiceMock;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly ArrayOrderIgnoreProcessingRule arrayOrderIgnoreProcessingRule;

        public ArrayOrderIgnoreProcessingRuleTests()
        {
            this.jsonElementServiceMock = new Mock<JsonElementService>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.arrayOrderIgnoreProcessingRule =
                new ArrayOrderIgnoreProcessingRule(
                    jsonElementService: this.jsonElementServiceMock.Object,
                    loggingBroker: this.loggingBrokerMock.Object);
        }
        private static string GetRandomString() =>
            new MnemonicString().GetValue();

        private static Expression<Func<Xeption, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => actualException.SameExceptionAs(expectedException);

        private static JsonElement ParseJsonElement(string json) =>
            JsonDocument.Parse(json).RootElement.Clone();


        public static TheoryData<Xeption> DependencyValidationExceptions()
        {
            string randomMessage = GetRandomString();
            string exceptionMessage = randomMessage;
            var innerException = new Xeption(exceptionMessage);

            return new TheoryData<Xeption>
            {
                new ConsumerServiceValidationException(
                    message: "Consumer validation errors occurred, please try again",
                    innerException),

                new ConsumerServiceValidationException(
                    message: "Consumer dependency validation occurred, please try again.",
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
                new ConsumerServiceDependencyException(
                    message: "Consumer dependency error occurred, please contact support.",
                    innerException),

                new ConsumerServiceException(
                    message: "Consumer service error occurred, please contact support.",
                    innerException),
            };
        }
    }
}