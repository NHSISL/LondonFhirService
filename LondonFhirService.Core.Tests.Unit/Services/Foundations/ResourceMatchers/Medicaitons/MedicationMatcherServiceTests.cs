// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.Medications;
using Moq;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Medications
{
  public partial class MedicationMatcherServiceTests
  {
      private readonly Mock<ILoggingBroker> loggingBrokerMock;
      private readonly MedicationMatcherService medicationMatcherService;

      public MedicationMatcherServiceTests()
      {
          this.loggingBrokerMock = new Mock<ILoggingBroker>();

          this.medicationMatcherService =
              new MedicationMatcherService(
                  loggingBroker: this.loggingBrokerMock.Object);
      }

      private static Expression<Func<Xeption, bool>> SameExceptionAs(Xeption expectedException) =>
          actualException => actualException.SameExceptionAs(expectedException);

      private static Dictionary<string, JsonElement> CreateResourceIndex() =>
          new();

      private static JsonElement CreateMedicationResource(
          string snomedCode,
          string onsetDateTime,
          string id = "allergy-1")
      {
          string json = $$"""
          {
            "resourceType": "Medication",
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

      private static JsonElement CreateNonSnomedMedicationResource(string onsetDateTime)
      {
          string json = $$"""
          {
            "resourceType": "Medication",
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
            "resourceType": "Medication",
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
            "resourceType": "Medication",
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