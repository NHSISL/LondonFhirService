// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.Immunizations;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Immunizations
{
    public partial class ImmunizationMatcherServiceTests
    {
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly ImmunizationMatcherService immunizationMatcherService;

        public ImmunizationMatcherServiceTests()
        {
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.immunizationMatcherService =
                new ImmunizationMatcherService(
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
            $"IMM-{new IntRange(min: 1000, max: 9999).GetValue()}";

        private static JsonElement CreateImmunizationResource(
            string ddsIdentifierValue,
            string id)
        {
            string json = $$"""
              {
                "resourceType": "Immunization",
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

        private static JsonElement CreateNonDdsImmunizationResource(string id)
        {
            string json = $$"""
              {
                "resourceType": "Immunization",
                "id": "{{id}}",
                "identifier": [
                  {
                    "system": "http://example.org/system",
                    "value": "IMM-1"
                  }
                ],
                "status": "completed"
              }
              """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateImmunizationResourceWithoutIdentifierProperty(string id)
        {
            string json = $$"""
              {
                "resourceType": "Immunization",
                "id": "{{id}}",
                "status": "completed"
              }
              """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateComprehensiveImmunizationResource(
            string ddsIdentifierValue,
            string id)
        {
            string json = $$"""
              {
                "resourceType": "Immunization",
                "id": "{{id}}",
                "meta": {
                  "versionId": "1",
                  "lastUpdated": "2024-12-12T10:30:00+00:00",
                  "profile": [
                    "https://fhir.hl7.org.uk/STU3/StructureDefinition/CareConnect-Immunization-1"
                  ]
                },
                "text": {
                  "status": "generated",
                  "div": "<div xmlns=\"http://www.w3.org/1999/xhtml\"><p>Influenza vaccine.</p></div>"
                },
                "identifier": [
                  {
                    "use": "official",
                    "system": "https://fhir.nhs.uk/Id/immunisation-id",
                    "value": "{{ddsIdentifierValue}}"
                  },
                  {
                    "use": "usual",
                    "system": "https://fhir.hl7.org.uk/Id/dds",
                    "value": "{{ddsIdentifierValue}}"
                  }
                ],
                "status": "completed",
                "notGiven": false,
                "vaccineCode": {
                  "coding": [
                    {
                      "system": "http://snomed.info/sct",
                      "code": "871873002",
                      "display": "Quadrivalent influenza vaccine (procedure)"
                    }
                  ]
                },
                "patient": {
                  "reference": "Patient/6e6c53cb-fca3-4e34-9cbc-476c32f1eb3c"
                },
                "encounter": {
                  "reference": "Encounter/d9a5732b-3af5-4fa6-8cda-b91e76823cdd"
                },
                "date": "2024-12-12T10:30:00+00:00",
                "primarySource": true,
                "lotNumber": "ABC123",
                "expirationDate": "2025-06-30",
                "site": {
                  "coding": [
                    {
                      "system": "http://hl7.org/fhir/v3/ActSite",
                      "code": "LA",
                      "display": "Left arm"
                    }
                  ]
                },
                "route": {
                  "coding": [
                    {
                      "system": "http://hl7.org/fhir/v3/RouteOfAdministration",
                      "code": "IM",
                      "display": "Intramuscular"
                    }
                  ]
                },
                "doseQuantity": {
                  "value": 0.5,
                  "unit": "mL",
                  "system": "http://unitsofmeasure.org",
                  "code": "mL"
                },
                "practitioner": [
                  {
                    "actor": {
                      "reference": "Practitioner/8a77e616-2d12-4bc9-b0a1-2f1576ec1c05"
                    },
                    "role": {
                      "coding": [
                        {
                          "system": "http://hl7.org/fhir/v2/0443",
                          "code": "AP",
                          "display": "Administering Provider"
                        }
                      ]
                    }
                  }
                ],
                "note": [
                  {
                    "text": "Annual flu jab administered without complications."
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
