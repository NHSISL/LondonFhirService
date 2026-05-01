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
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly ConditionMatcherService conditionMatcherService;

        public ConditionMatcherServiceTests()
        {
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.conditionMatcherService =
                new ConditionMatcherService(
                    loggingBroker: this.loggingBrokerMock.Object);
        }

        private static Expression<Func<Xeption, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => actualException.SameExceptionAs(expectedException);

        private static Dictionary<string, JsonElement> CreateResourceIndex() =>
            new Dictionary<string, JsonElement>();

        private static string GetRandomString() =>
            new MnemonicString(wordCount: GetRandomNumber()).GetValue();

        private static string GetRandomSnomedCode() =>
            new IntRange(min: 1000000, max: 9999999).GetValue().ToString();

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static DateTimeOffset GetRandomDateTimeOffset() =>
            new DateTimeRange(earliestDate: new DateTime()).GetValue();

        private static JsonElement CreateConditionResource(
            string snomedCode,
            string onsetDateTime,
            string id)
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

        private static JsonElement CreateComprehensiveConditionResource(
            string snomedCode,
            string onsetDateTime,
            string id)
        {
            string json = $$"""
              {
                "resourceType": "Condition",
                "id": "{{id}}",
                "meta": {
                  "versionId": "1",
                  "lastUpdated": "2024-09-12T09:25:00+01:00",
                  "profile": [
                    "https://fhir.nhs.uk/STU3/StructureDefinition/CareConnect-ProblemHeader-Condition-1"
                  ]
                },
                "text": {
                  "status": "generated",
                  "div": "<div xmlns=\"http://www.w3.org/1999/xhtml\"><p>Type 2 diabetes mellitus.</p></div>"
                },
                "identifier": [
                  {
                    "use": "official",
                    "system": "https://fhir.nhs.uk/Id/condition-id",
                    "value": "COND-c0a91a7d"
                  },
                  {
                    "use": "usual",
                    "system": "https://fhir.hl7.org.uk/Id/dds",
                    "value": "COND-c0a91a7d"
                  }
                ],
                "clinicalStatus": "active",
                "verificationStatus": "confirmed",
                "category": [
                  {
                    "coding": [
                      {
                        "system": "https://fhir.hl7.org.uk/STU3/CodeSystem/CareConnect-ConditionCategory-1",
                        "code": "problem-list-item",
                        "display": "Problem List Item"
                      }
                    ]
                  }
                ],
                "severity": {
                  "coding": [
                    {
                      "system": "http://snomed.info/sct",
                      "code": "6736007",
                      "display": "Moderate (severity modifier)"
                    }
                  ]
                },
                "code": {
                  "coding": [
                    {
                      "system": "http://snomed.info/sct",
                      "code": "{{snomedCode}}",
                      "display": "Type 2 diabetes mellitus (disorder)"
                    }
                  ],
                  "text": "Type 2 diabetes mellitus"
                },
                "subject": {
                  "reference": "Patient/6e6c53cb-fca3-4e34-9cbc-476c32f1eb3c"
                },
                "context": {
                  "reference": "Encounter/d9a5732b-3af5-4fa6-8cda-b91e76823cdd"
                },
                "onsetDateTime": "{{onsetDateTime}}",
                "assertedDate": "2022-04-12",
                "asserter": {
                  "reference": "Practitioner/8a77e616-2d12-4bc9-b0a1-2f1576ec1c05"
                },
                "note": [
                  {
                    "text": "Patient managing well with current medication and diet."
                  }
                ]
              }
              """;

            return ParseJsonElement(json);
        }

        private static JsonElement ParseJsonElement(string json) =>
            JsonDocument.Parse(json).RootElement.Clone();
    }
}