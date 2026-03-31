// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.Patients;
using Moq;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Patients
{
  public partial class PatientMatcherServiceTests
  {
      private readonly Mock<ILoggingBroker> loggingBrokerMock;
      private readonly PatientMatcherService patientMatcherService;

      public PatientMatcherServiceTests()
      {
          this.loggingBrokerMock = new Mock<ILoggingBroker>();
          

          this.patientMatcherService =
              new PatientMatcherService(
                  loggingBroker: this.loggingBrokerMock.Object);
      }

      private static Expression<Func<Xeption, bool>> SameExceptionAs(Xeption expectedException) =>
          actualException => actualException.SameExceptionAs(expectedException);

      private static Dictionary<string, JsonElement> CreateResourceIndex() =>
          new();

      private static JsonElement CreatePatientResource(
          string snomedCode,
          string onsetDateTime,
          string id = "allergy-1")
      {
          string json = $$"""
          {
            "resourceType": "Patient",
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

      private static JsonElement CreateNonSnomedPatientResource(string onsetDateTime)
      {
          string json = $$"""
          {
            "resourceType": "Patient",
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
            "resourceType": "Patient",
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
            "resourceType": "Patient",
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