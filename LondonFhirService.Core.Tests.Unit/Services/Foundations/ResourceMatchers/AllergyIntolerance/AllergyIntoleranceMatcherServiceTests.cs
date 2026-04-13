// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.AllergyIntolerances;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.AllergyIntolerances
{
  public partial class AllergyIntoleranceMatcherServiceTests
  {
      private readonly Mock<ILoggingBroker> loggingBrokerMock;
      private readonly AllergyIntoleranceMatcherService allergyIntoleranceMatcherService;

      public AllergyIntoleranceMatcherServiceTests()
      {
          this.loggingBrokerMock = new Mock<ILoggingBroker>();
          

          this.allergyIntoleranceMatcherService =
              new AllergyIntoleranceMatcherService(
                  loggingBroker: this.loggingBrokerMock.Object);
      }

      private static Expression<Func<Xeption, bool>> SameExceptionAs(Xeption expectedException) =>
          actualException => actualException.SameExceptionAs(expectedException);

      private static Dictionary<string, JsonElement> CreateResourceIndex() =>
          new();

      private static string GetRandomSnomedCode() =>
          new IntRange(min: 1000000, max: 9999999).GetValue().ToString();

      private static string GetRandomDateString() =>
          new DateTimeRange(earliestDate: new DateTime(2000, 1, 1)).GetValue().ToString("yyyy-MM-dd");

      private static JsonElement CreateAllergyIntoleranceResource(
          string snomedCode,
          string onsetDateTime,
          string id = "allergy-1")
      {
          string json = $$"""
          {
            "resourceType": "AllergyIntolerance",
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

      private static JsonElement CreateNonSnomedAllergyIntoleranceResource(string onsetDateTime)
      {
          string json = $$"""
          {
            "resourceType": "AllergyIntolerance",
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
            "resourceType": "AllergyIntolerance",
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
            "resourceType": "AllergyIntolerance",
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
  }
}