// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.Encounters;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Encounters
{
    public partial class EncounterMatcherServiceTests
    {
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly EncounterMatcherService encounterMatcherService;

        public EncounterMatcherServiceTests()
        {
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.encounterMatcherService =
                new EncounterMatcherService(
                    loggingBroker: this.loggingBrokerMock.Object);
        }

        private static Expression<Func<Xeption, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => actualException.SameExceptionAs(expectedException);

        private static Dictionary<string, JsonElement> CreateResourceIndex() =>
            new();

        private static string GetRandomString() =>
            new MnemonicString(wordCount: GetRandomNumber()).GetValue();

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static string GetRandomDdsIdentifierValue() =>
            $"ENC-{new IntRange(min: 1000, max: 9999).GetValue()}";

        private static JsonElement CreateEncounterResource(
            string ddsIdentifierValue,
            string id)
        {
            string json = $$"""
              {
                "resourceType": "Encounter",
                "id": "{{id}}",
                "identifier": [
                  {
                    "system": "https://fhir.hl7.org.uk/Id/dds",
                    "value": "{{ddsIdentifierValue}}"
                  }
                ],
                "status": "finished"
              }
              """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateNonDdsEncounterResource(string id)
        {
            string json = $$"""
              {
                "resourceType": "Encounter",
                "id": "{{id}}",
                "identifier": [
                  {
                    "system": "http://example.org/system",
                    "value": "ENC-1"
                  }
                ],
                "status": "finished"
              }
              """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateEncounterResourceWithoutIdentifierProperty(string id)
        {
            string json = $$"""
              {
                "resourceType": "Encounter",
                "id": "{{id}}",
                "status": "finished"
              }
              """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateComprehensiveEncounterResource(
            string ddsIdentifierValue,
            string id)
        {
            string json = $$"""
              {
                "resourceType": "Encounter",
                "id": "{{id}}",
                "meta": {
                  "versionId": "1",
                  "lastUpdated": "2024-09-12T09:00:00+01:00",
                  "profile": [
                    "https://fhir.hl7.org.uk/STU3/StructureDefinition/CareConnect-Encounter-1"
                  ]
                },
                "text": {
                  "status": "generated",
                  "div": "<div xmlns=\"http://www.w3.org/1999/xhtml\"><p>Outpatient encounter.</p></div>"
                },
                "identifier": [
                  {
                    "use": "official",
                    "system": "https://fhir.nhs.uk/Id/encounter-id",
                    "value": "{{ddsIdentifierValue}}"
                  },
                  {
                    "use": "usual",
                    "system": "https://fhir.hl7.org.uk/Id/dds",
                    "value": "{{ddsIdentifierValue}}"
                  }
                ],
                "status": "finished",
                "class": {
                  "system": "http://hl7.org/fhir/v3/v3-ActCode",
                  "code": "AMB",
                  "display": "ambulatory"
                },
                "type": [
                  {
                    "coding": [
                      {
                        "system": "http://snomed.info/sct",
                        "code": "185349003",
                        "display": "Encounter for check up (procedure)"
                      }
                    ]
                  }
                ],
                "subject": {
                  "reference": "Patient/6e6c53cb-fca3-4e34-9cbc-476c32f1eb3c"
                },
                "participant": [
                  {
                    "individual": {
                      "reference": "Practitioner/8a77e616-2d12-4bc9-b0a1-2f1576ec1c05"
                    }
                  }
                ],
                "period": {
                  "start": "2024-09-12T09:00:00+01:00",
                  "end": "2024-09-12T09:30:00+01:00"
                },
                "reason": [
                  {
                    "coding": [
                      {
                        "system": "http://snomed.info/sct",
                        "code": "44054006",
                        "display": "Type 2 diabetes mellitus (disorder)"
                      }
                    ]
                  }
                ],
                "location": [
                  {
                    "location": {
                      "reference": "Location/abc12345-6789-4def-0123-456789abcdef"
                    }
                  }
                ],
                "serviceProvider": {
                  "reference": "Organization/org12345-6789-4def-0123-456789abcdef"
                }
              }
              """;

            return ParseJsonElement(json);
        }

        private static JsonElement ParseJsonElement(string json)
        {
            using JsonDocument jsonDocument = JsonDocument.Parse(json);

            return jsonDocument.RootElement.Clone();
        }
    }
}
