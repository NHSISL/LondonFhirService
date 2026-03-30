// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.Conditions;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Conditions
{
    public partial class ConditionMatcherServiceTests
    {
        private readonly Mock<LoggingBroker> loggingBrokerMock;
        private readonly ConditionMatcherService conditionMatcherService;

        public ConditionMatcherServiceTests()
        {
            this.loggingBrokerMock = new Mock<LoggingBroker>();

            this.conditionMatcherService =
              new ConditionMatcherService(
                  loggingBroker: this.loggingBrokerMock.Object);
        }


        private static Expression<Func<Xeption, bool>> SameExceptionAs(Xeption expectedException) =>
        actualException => actualException.SameExceptionAs(expectedException);

        private static Dictionary<string, JsonElement> CreateResourceIndex() =>
            new();

        private static int GetRandomNumber() =>
                new IntRange(min: 2, max: 10).GetValue();

        private static string GetRandomString() =>
        new MnemonicString(wordCount: GetRandomNumber()).GetValue();

        private static string GetRandomStringWithLengthOf(int length)
        {
            string result = new MnemonicString(wordCount: 1, wordMinLength: length, wordMaxLength: length).GetValue();

            return result.Length > length ? result.Substring(0, length) : result;
        }

        private static JsonElement CreateConditionResource(
            string snomedCode,
            string onsetDateTime,
            string id = "allergy-1")
        {
            string json = $$"""
        {
          "resourceType": "Condition",
          "id": "{{id}}",
          "code": {
            "coding": [
              {
                "system": "http://snomed.info/sct",
                "code": "{{snomedCode}}"
              }
            ]
          },
          "onsetDateTime": "{{onsetDateTime}}"
        }
        """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateNonSnomedConditionResource(string onsetDateTime)
        {
            string json = $$"""
        {
          "resourceType": "Condition",
          "id": "allergy-1",
          "code": {
            "coding": [
              {
                "system": "http://example.org/system",
                "code": "123456"
              }
            ]
          },
          "onsetDateTime": "{{onsetDateTime}}"
        }
        """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateResourceWithoutOnsetDateTime(string snomedCode)
        {
            string json = $$"""
        {
          "resourceType": "Condition",
          "id": "allergy-1",
          "code": {
            "coding": [
              {
                "system": "http://snomed.info/sct",
                "code": "{{snomedCode}}"
              }
            ]
          }
        }
        """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateMalformedCodingResource()
        {
            string json = """
        {
          "resourceType": "Condition",
          "id": "allergy-1",
          "code": {
            "coding": {
              "system": "http://snomed.info/sct",
              "code": "91936005"
            }
          },
          "onsetDateTime": "2024-01-01"
        }
        """;

            return ParseJsonElement(json);
        }

        private static JsonElement ParseJsonElement(string json) =>
            JsonDocument.Parse(json).RootElement.Clone();

        private static JsonElement CreateRandomSourceResources()
        {
            string randomSnomedCode = GetRandomNumber().ToString();
            string randomOnsetDateTime = $"2024-{GetRandomNumber():D2}-{GetRandomNumber():D2}";
            string randomId = $"allergy-{GetRandomString()}";

            return CreateConditionResource(
                snomedCode: randomSnomedCode,
                onsetDateTime: randomOnsetDateTime,
                id: randomId);
        }
    }
}
