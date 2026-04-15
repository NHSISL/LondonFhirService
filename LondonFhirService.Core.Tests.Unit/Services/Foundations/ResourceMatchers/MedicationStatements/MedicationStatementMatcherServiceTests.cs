// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.MedicationStatements;
using Moq;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.MedicationStatements
{
    public partial class MedicationStatementMatcherServiceTests
    {
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly MedicationStatementMatcherService medicationStatementMatcherService;

        public MedicationStatementMatcherServiceTests()
        {
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.medicationStatementMatcherService =
                new MedicationStatementMatcherService(
                    loggingBroker: this.loggingBrokerMock.Object);
        }

        private static Expression<Func<Xeption, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => actualException.SameExceptionAs(expectedException);

        private static Dictionary<string, JsonElement> CreateResourceIndex() =>
            new();

        private static JsonElement CreateMedicationStatementResource(
            string snomedCode,
            string onsetDateTime,
            string id = "allergy-1")
        {
            string json = $$"""
          {
            "resourceType": "MedicationStatement",
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

        private static JsonElement CreateNonSnomedMedicationStatementResource(string onsetDateTime)
        {
            string json = $$"""
          {
            "resourceType": "MedicationStatement",
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
            "resourceType": "MedicationStatement",
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
            "resourceType": "MedicationStatement",
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

        private static JsonElement CreateMedicationStatementWithCodeableConcept(
            string snomedCode,
            string dateAsserted,
            string id = "med-stmt-1")
        {
            string json = $$"""
          {
            "resourceType": "MedicationStatement",
            "id": "{{id}}",
            "medicationCodeableConcept": {
              "coding": [
                {
                  "system": "http://snomed.info/sct",
                  "code": "{{snomedCode}}"
                }
              ]
            },
            "dateAsserted": "{{dateAsserted}}"
          }
          """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateMedicationStatementWithReference(
            string reference,
            string dateAsserted,
            string id = "med-stmt-1")
        {
            string json = $$"""
          {
            "resourceType": "MedicationStatement",
            "id": "{{id}}",
            "medicationReference": {
              "reference": "{{reference}}"
            },
            "dateAsserted": "{{dateAsserted}}"
          }
          """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateMedicationForIndex(
            string snomedCode,
            string id = "medication-1")
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
            }
          }
          """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateNonSnomedMedicationStatement(
            string dateAsserted,
            string id = "med-stmt-1")
        {
            string json = $$"""
          {
            "resourceType": "MedicationStatement",
            "id": "{{id}}",
            "medicationCodeableConcept": {
              "coding": [
                {
                  "system": "http://example.org/system",
                  "code": "12345"
                }
              ]
            },
            "dateAsserted": "{{dateAsserted}}"
          }
          """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateMedicationStatementWithCodeableConceptWithoutDateAsserted(
            string snomedCode,
            string id = "med-stmt-1")
        {
            string json = $$"""
          {
            "resourceType": "MedicationStatement",
            "id": "{{id}}",
            "medicationCodeableConcept": {
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

        private static JsonElement CreateMedicationStatementWithReferenceAndCodeableConcept(
            string reference,
            string snomedCode,
            string dateAsserted,
            string id = "med-stmt-1")
        {
            string json = $$"""
          {
            "resourceType": "MedicationStatement",
            "id": "{{id}}",
            "medicationReference": {
              "reference": "{{reference}}"
            },
            "medicationCodeableConcept": {
              "coding": [
                {
                  "system": "http://snomed.info/sct",
                  "code": "{{snomedCode}}"
                }
              ]
            },
            "dateAsserted": "{{dateAsserted}}"
          }
          """;

            return ParseJsonElement(json);
        }

        private static JsonElement ParseJsonElement(string json) =>
            JsonDocument.Parse(json).RootElement.Clone();
    }
}