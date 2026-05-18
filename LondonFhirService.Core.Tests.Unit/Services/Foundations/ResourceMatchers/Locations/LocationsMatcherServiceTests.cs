// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.Locations;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Locations
{
    public partial class LocationMatcherServiceTests
    {
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly LocationMatcherService locationMatcherService;

        public LocationMatcherServiceTests()
        {
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.locationMatcherService =
                new LocationMatcherService(
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

        private static string GetRandomOdsSiteCodeValue() =>
            $"A{new IntRange(min: 10000, max: 99999).GetValue()}-1";

        private static JsonElement CreateLocationResource(
            string odsSiteCode,
            string id)
        {
            string json = $$"""
              {
                "resourceType": "Location",
                "id": "{{id}}",
                "identifier": [
                  {
                    "system": "https://fhir.nhs.uk/Id/ods-site-code",
                    "value": "{{odsSiteCode}}"
                  }
                ],
                "status": "active"
              }
              """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateNonOdsLocationResource(string id)
        {
            string json = $$"""
              {
                "resourceType": "Location",
                "id": "{{id}}",
                "identifier": [
                  {
                    "system": "http://example.org/system",
                    "value": "LOC-1"
                  }
                ],
                "status": "active"
              }
              """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateLocationResourceWithoutIdentifierProperty(string id)
        {
            string json = $$"""
              {
                "resourceType": "Location",
                "id": "{{id}}",
                "status": "active"
              }
              """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateComprehensiveLocationResource(
            string odsSiteCode,
            string id)
        {
            string json = $$"""
              {
                "resourceType": "Location",
                "id": "{{id}}",
                "meta": {
                  "versionId": "1",
                  "lastUpdated": "2024-09-12T08:00:00+01:00",
                  "profile": [
                    "https://fhir.hl7.org.uk/STU3/StructureDefinition/CareConnect-Location-1"
                  ]
                },
                "text": {
                  "status": "generated",
                  "div": "<div xmlns=\"http://www.w3.org/1999/xhtml\"><p>Consultation Room.</p></div>"
                },
                "identifier": [
                  {
                    "use": "official",
                    "system": "https://fhir.nhs.uk/Id/ods-site-code",
                    "value": "{{odsSiteCode}}"
                  },
                  {
                    "use": "usual",
                    "system": "https://fhir.hl7.org.uk/Id/dds",
                    "value": "LOC-34ff7bfc"
                  }
                ],
                "status": "active",
                "name": "Castlefield Surgery — Consultation Room 2",
                "alias": [
                  "Castlefield CR2"
                ],
                "description": "General practice consultation room.",
                "mode": "instance",
                "type": [
                  {
                    "coding": [
                      {
                        "system": "http://hl7.org/fhir/v3/v3-RoleCode",
                        "code": "GACH",
                        "display": "Hospitals; General Acute Care Hospital"
                      }
                    ]
                  }
                ],
                "telecom": [
                  {
                    "system": "phone",
                    "value": "020 7946 0000",
                    "use": "work"
                  }
                ],
                "address": {
                  "use": "work",
                  "type": "physical",
                  "line": [
                    "1 Castlefield Road"
                  ],
                  "city": "London",
                  "postalCode": "E1 6AN",
                  "country": "GB"
                },
                "physicalType": {
                  "coding": [
                    {
                      "system": "http://hl7.org/fhir/location-physical-type",
                      "code": "ro",
                      "display": "Room"
                    }
                  ]
                },
                "position": {
                  "longitude": -0.072,
                  "latitude": 51.515,
                  "altitude": 10.0
                },
                "managingOrganization": {
                  "reference": "Organization/org12345-6789-4def-0123-456789abcdef"
                }
              }
              """;

            return ParseJsonElement(json);
        }

        private static JsonElement ParseJsonElement(string json) =>
            JsonDocument.Parse(json).RootElement.Clone();
    }
}
