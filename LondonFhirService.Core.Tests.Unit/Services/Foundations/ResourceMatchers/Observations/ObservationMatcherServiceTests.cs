// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.Observations;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Observations
{
    public partial class ObservationMatcherServiceTests
    {
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly ObservationMatcherService observationMatcherService;

        public ObservationMatcherServiceTests()
        {
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.observationMatcherService =
                new ObservationMatcherService(
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
            $"OBS-{new IntRange(min: 1000, max: 9999).GetValue()}";

        private static JsonElement CreateObservationResource(
            string ddsIdentifierValue,
            string id)
        {
            string json = $$"""
              {
                "resourceType": "Observation",
                "id": "{{id}}",
                "identifier": [
                  {
                    "system": "https://fhir.hl7.org.uk/Id/dds",
                    "value": "{{ddsIdentifierValue}}"
                  }
                ],
                "status": "final"
              }
              """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateNonDdsObservationResource(string id)
        {
            string json = $$"""
              {
                "resourceType": "Observation",
                "id": "{{id}}",
                "identifier": [
                  {
                    "system": "http://example.org/system",
                    "value": "OBS-1"
                  }
                ],
                "status": "final"
              }
              """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateObservationResourceWithoutIdentifierProperty(string id)
        {
            string json = $$"""
              {
                "resourceType": "Observation",
                "id": "{{id}}",
                "status": "final"
              }
              """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateComprehensiveObservationResource(
            string ddsIdentifierValue,
            string id)
        {
            string json = $$"""
              {
                "resourceType": "Observation",
                "id": "{{id}}",
                "meta": {
                  "versionId": "1",
                  "lastUpdated": "2024-09-12T10:45:00+01:00",
                  "profile": [
                    "https://fhir.hl7.org.uk/STU3/StructureDefinition/CareConnect-Observation-1"
                  ]
                },
                "text": {
                  "status": "generated",
                  "div": "<div xmlns=\"http://www.w3.org/1999/xhtml\"><p>HbA1c level: 7.2%.</p></div>"
                },
                "identifier": [
                  {
                    "use": "official",
                    "system": "https://fhir.nhs.uk/Id/observation-id",
                    "value": "{{ddsIdentifierValue}}"
                  },
                  {
                    "use": "usual",
                    "system": "https://fhir.hl7.org.uk/Id/dds",
                    "value": "{{ddsIdentifierValue}}"
                  }
                ],
                "status": "final",
                "category": [
                  {
                    "coding": [
                      {
                        "system": "http://hl7.org/fhir/observation-category",
                        "code": "laboratory",
                        "display": "Laboratory"
                      }
                    ]
                  }
                ],
                "code": {
                  "coding": [
                    {
                      "system": "http://snomed.info/sct",
                      "code": "75367002",
                      "display": "Blood pressure (observable entity)"
                    }
                  ],
                  "text": "Blood pressure"
                },
                "subject": {
                  "reference": "Patient/6e6c53cb-fca3-4e34-9cbc-476c32f1eb3c"
                },
                "context": {
                  "reference": "Encounter/d9a5732b-3af5-4fa6-8cda-b91e76823cdd"
                },
                "effectiveDateTime": "2024-09-12T10:45:00+01:00",
                "issued": "2024-09-12T11:00:00+01:00",
                "performer": [
                  {
                    "reference": "Practitioner/8a77e616-2d12-4bc9-b0a1-2f1576ec1c05"
                  }
                ],
                "valueQuantity": {
                  "value": 122,
                  "unit": "mmHg",
                  "system": "http://unitsofmeasure.org",
                  "code": "mm[Hg]"
                },
                "interpretation": {
                  "coding": [
                    {
                      "system": "http://hl7.org/fhir/v2/0078",
                      "code": "N",
                      "display": "Normal"
                    }
                  ]
                },
                "comment": "Within normal range.",
                "bodySite": {
                  "coding": [
                    {
                      "system": "http://snomed.info/sct",
                      "code": "368208006",
                      "display": "Left upper arm"
                    }
                  ]
                },
                "method": {
                  "coding": [
                    {
                      "system": "http://snomed.info/sct",
                      "code": "37931006",
                      "display": "Auscultation"
                    }
                  ]
                },
                "referenceRange": [
                  {
                    "low": {
                      "value": 90,
                      "unit": "mmHg",
                      "system": "http://unitsofmeasure.org",
                      "code": "mm[Hg]"
                    },
                    "high": {
                      "value": 140,
                      "unit": "mmHg",
                      "system": "http://unitsofmeasure.org",
                      "code": "mm[Hg]"
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
