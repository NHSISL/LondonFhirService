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

        private static JsonElement CreateComprehensiveListResource(
            string title,
            string id)
        {
            string json = $$"""
              {
                "resourceType": "List",
                "id": "{{id}}",
                "meta": {
                  "versionId": "1",
                  "lastUpdated": "2024-09-12T09:25:00+01:00",
                  "profile": [
                    "https://fhir.hl7.org.uk/STU3/StructureDefinition/CareConnect-List-1"
                  ]
                },
                "text": {
                  "status": "generated",
                  "div": "<div xmlns=\"http://www.w3.org/1999/xhtml\"><p>Active problem list.</p></div>"
                },
                "identifier": [
                  {
                    "use": "official",
                    "system": "https://fhir.nhs.uk/Id/list-id",
                    "value": "LIST-c0a91a7d"
                  },
                  {
                    "use": "usual",
                    "system": "https://fhir.hl7.org.uk/Id/dds",
                    "value": "LIST-c0a91a7d"
                  }
                ],
                "status": "current",
                "mode": "snapshot",
                "title": "{{title}}",
                "code": {
                  "coding": [
                    {
                      "system": "http://snomed.info/sct",
                      "code": "1102421000000108",
                      "display": "Problem list"
                    }
                  ]
                },
                "subject": {
                  "reference": "Patient/6e6c53cb-fca3-4e34-9cbc-476c32f1eb3c"
                },
                "encounter": {
                  "reference": "Encounter/d9a5732b-3af5-4fa6-8cda-b91e76823cdd"
                },
                "date": "2024-09-12T09:25:00+01:00",
                "source": {
                  "reference": "Practitioner/8a77e616-2d12-4bc9-b0a1-2f1576ec1c05"
                },
                "orderedBy": {
                  "coding": [
                    {
                      "system": "http://hl7.org/fhir/list-order",
                      "code": "system",
                      "display": "Sorted by System"
                    }
                  ]
                },
                "note": [
                  {
                    "text": "Reviewed at annual diabetes appointment."
                  }
                ],
                "entry": [
                  {
                    "item": {
                      "reference": "Condition/c0a91a7d-8a49-4d20-93b1-3e6a0d29c0a1"
                    }
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