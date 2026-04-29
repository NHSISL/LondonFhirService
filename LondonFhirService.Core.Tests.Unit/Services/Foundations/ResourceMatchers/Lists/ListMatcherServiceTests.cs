// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.Lists;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Lists
{
    public partial class ListMatcherServiceTests
    {
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly ListMatcherService listMatcherService;

        public ListMatcherServiceTests()
        {
            this.loggingBrokerMock = new Mock<ILoggingBroker>();


            this.listMatcherService =
                new ListMatcherService(
                    loggingBroker: this.loggingBrokerMock.Object);
        }

        private static Expression<Func<Xeption, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => actualException.SameExceptionAs(expectedException);

        private static Dictionary<string, JsonElement> CreateResourceIndex() =>
            new();

        private static JsonElement CreateListResource(
            string snomedCode,
            string onsetDateTime,
            string id = "allergy-1")
        {
            string json = $$"""
          {
            "resourceType": "List",
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

        private static JsonElement CreateNonSnomedListResource(string onsetDateTime)
        {
            string json = $$"""
          {
            "resourceType": "List",
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
            "resourceType": "List",
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
            "resourceType": "List",
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

        private static string GetRandomString() =>
            new MnemonicString(wordCount: GetRandomNumber()).GetValue();

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static DateTimeOffset GetRandomDateTimeOffset() =>
            new DateTimeRange(earliestDate: new DateTime()).GetValue();

        private static string GetRandomTitle() =>
            GetRandomString();

        private static JsonElement CreateListResourceWithTitle(
            string title,
            string id = "list-1")
        {
            string json = $$"""
          {
            "resourceType": "List",
            "id": "{{id}}",
            "title": "{{title}}",
            "status": "current"
          }
          """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateListResourceWithoutTitle()
        {
            string json = """
          {
            "resourceType": "List",
            "id": "list-no-title",
            "status": "current"
          }
          """;

            return ParseJsonElement(json);
        }

        private static JsonElement ParseJsonElement(string json) =>
            JsonDocument.Parse(json).RootElement.Clone();
    }
}