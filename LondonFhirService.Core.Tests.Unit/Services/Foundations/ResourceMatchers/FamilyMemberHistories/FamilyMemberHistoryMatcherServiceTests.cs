// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.FamilyMemberHistories;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.FamilyMemberHistories
{
    public partial class FamilyMemberHistoryMatcherServiceTests
    {
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly FamilyMemberHistoryMatcherService familyMemberHistoryMatcherService;

        public FamilyMemberHistoryMatcherServiceTests()
        {
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.familyMemberHistoryMatcherService =
                new FamilyMemberHistoryMatcherService(
                    loggingBroker: this.loggingBrokerMock.Object);
        }

        private static Expression<Func<Xeption, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => actualException.SameExceptionAs(expectedException);

        private static Dictionary<string, JsonElement> CreateResourceIndex() =>
            new Dictionary<string, JsonElement>();

        private static string GetRandomString() =>
            new MnemonicString(wordCount: GetRandomNumber()).GetValue();

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static string GetRandomDdsIdentifierValue() =>
            $"FMH-{new IntRange(min: 1000, max: 9999).GetValue()}";

        private static JsonElement CreateFamilyMemberHistoryResource(
            string ddsIdentifierValue,
            string id)
        {
            string json = $$"""
              {
                "resourceType": "FamilyMemberHistory",
                "id": "{{id}}",
                "identifier": [
                  {
                    "system": "https://fhir.hl7.org.uk/Id/dds",
                    "value": "{{ddsIdentifierValue}}"
                  }
                ],
                "status": "completed"
              }
              """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateNonDdsFamilyMemberHistoryResource(string id)
        {
            string json = $$"""
              {
                "resourceType": "FamilyMemberHistory",
                "id": "{{id}}",
                "identifier": [
                  {
                    "system": "http://example.org/system",
                    "value": "FMH-1"
                  }
                ],
                "status": "completed"
              }
              """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateFamilyMemberHistoryResourceWithoutIdentifierProperty(string id)
        {
            string json = $$"""
              {
                "resourceType": "FamilyMemberHistory",
                "id": "{{id}}",
                "status": "completed"
              }
              """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateComprehensiveFamilyMemberHistoryResource(
            string ddsIdentifierValue,
            string id)
        {
            string json = $$"""
              {
                "resourceType": "FamilyMemberHistory",
                "id": "{{id}}",
                "meta": {
                  "versionId": "1",
                  "lastUpdated": "2024-09-01T08:00:00+01:00",
                  "profile": [
                    "https://fhir.hl7.org.uk/STU3/StructureDefinition/CareConnect-FamilyMemberHistory-1"
                  ]
                },
                "text": {
                  "status": "generated",
                  "div": "<div xmlns=\"http://www.w3.org/1999/xhtml\"><p>Family history.</p></div>"
                },
                "identifier": [
                  {
                    "use": "official",
                    "system": "https://fhir.nhs.uk/Id/family-history-id",
                    "value": "{{ddsIdentifierValue}}"
                  },
                  {
                    "use": "usual",
                    "system": "https://fhir.hl7.org.uk/Id/dds",
                    "value": "{{ddsIdentifierValue}}"
                  }
                ],
                "status": "completed",
                "patient": {
                  "reference": "Patient/6e6c53cb-fca3-4e34-9cbc-476c32f1eb3c"
                },
                "date": "2024-09-01",
                "name": "John Smith Senior",
                "relationship": {
                  "coding": [
                    {
                      "system": "http://hl7.org/fhir/v3/v3-RoleCode",
                      "code": "FTH",
                      "display": "Father"
                    }
                  ]
                },
                "gender": "male",
                "bornDate": "1945-03-14",
                "ageAge": {
                  "value": 79,
                  "unit": "years",
                  "system": "http://unitsofmeasure.org",
                  "code": "a"
                },
                "condition": [
                  {
                    "code": {
                      "coding": [
                        {
                          "system": "http://snomed.info/sct",
                          "code": "44054006",
                          "display": "Type 2 diabetes mellitus (disorder)"
                        }
                      ]
                    },
                    "outcome": {
                      "coding": [
                        {
                          "system": "http://snomed.info/sct",
                          "code": "385669000",
                          "display": "Successful (qualifier value)"
                        }
                      ]
                    },
                    "onsetAge": {
                      "value": 55,
                      "unit": "years",
                      "system": "http://unitsofmeasure.org",
                      "code": "a"
                    },
                    "note": [
                      {
                        "text": "Diagnosed in 2000."
                      }
                    ]
                  }
                ],
                "note": [
                  {
                    "text": "Father has long-term type 2 diabetes managed with medication."
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
