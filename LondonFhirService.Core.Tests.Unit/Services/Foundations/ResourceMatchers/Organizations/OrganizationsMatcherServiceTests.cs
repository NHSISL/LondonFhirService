// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.Organizations;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Organizations
{
    public partial class OrganizationMatcherServiceTests
    {
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly OrganizationMatcherService organizationMatcherService;

        public OrganizationMatcherServiceTests()
        {
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.organizationMatcherService =
                new OrganizationMatcherService(
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

        private static string GetRandomOdsOrganizationCodeValue() =>
            $"A{new IntRange(min: 10000, max: 99999).GetValue()}";

        private static JsonElement CreateOrganizationResource(
            string odsOrganizationCode,
            string id)
        {
            string json = $$"""
              {
                "resourceType": "Organization",
                "id": "{{id}}",
                "identifier": [
                  {
                    "system": "https://fhir.nhs.uk/Id/ods-organization-code",
                    "value": "{{odsOrganizationCode}}"
                  }
                ],
                "active": true
              }
              """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateNonOdsOrganizationResource(string id)
        {
            string json = $$"""
              {
                "resourceType": "Organization",
                "id": "{{id}}",
                "identifier": [
                  {
                    "system": "http://example.org/system",
                    "value": "ORG-1"
                  }
                ],
                "active": true
              }
              """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateOrganizationResourceWithoutIdentifierProperty(string id)
        {
            string json = $$"""
              {
                "resourceType": "Organization",
                "id": "{{id}}",
                "active": true
              }
              """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateComprehensiveOrganizationResource(
            string odsOrganizationCode,
            string id)
        {
            string json = $$"""
              {
                "resourceType": "Organization",
                "id": "{{id}}",
                "meta": {
                  "versionId": "1",
                  "lastUpdated": "2024-09-01T08:00:00+01:00",
                  "profile": [
                    "https://fhir.hl7.org.uk/STU3/StructureDefinition/CareConnect-Organization-1"
                  ]
                },
                "text": {
                  "status": "generated",
                  "div": "<div xmlns=\"http://www.w3.org/1999/xhtml\"><p>Castlefield Medical Practice.</p></div>"
                },
                "identifier": [
                  {
                    "use": "official",
                    "system": "https://fhir.nhs.uk/Id/ods-organization-code",
                    "value": "{{odsOrganizationCode}}"
                  },
                  {
                    "use": "secondary",
                    "system": "https://fhir.nhs.uk/Id/ods-site-code",
                    "value": "{{odsOrganizationCode}}-1"
                  },
                  {
                    "use": "secondary",
                    "system": "https://fhir.hl7.org.uk/Id/companies-house-number",
                    "value": "CH-12345678"
                  },
                  {
                    "use": "usual",
                    "system": "https://fhir.hl7.org.uk/Id/dds",
                    "value": "ORG-56000299"
                  }
                ],
                "active": true,
                "type": [
                  {
                    "coding": [
                      {
                        "system": "http://hl7.org/fhir/v3/organization-type",
                        "code": "prov",
                        "display": "Healthcare Provider"
                      }
                    ]
                  },
                  {
                    "coding": [
                      {
                        "system": "http://hl7.org/fhir/v3/organization-type",
                        "code": "team",
                        "display": "Organizational Team"
                      }
                    ]
                  }
                ],
                "name": "Castlefield Medical Practice",
                "alias": [
                  "Castlefield GP"
                ],
                "telecom": [
                  {
                    "system": "phone",
                    "value": "020 7946 0000",
                    "use": "work"
                  },
                  {
                    "system": "email",
                    "value": "info@castlefield.example.nhs.uk",
                    "use": "work"
                  }
                ],
                "address": [
                  {
                    "use": "work",
                    "type": "physical",
                    "line": [
                      "1 Castlefield Road"
                    ],
                    "city": "London",
                    "postalCode": "E1 6AN",
                    "country": "GB"
                  }
                ],
                "contact": [
                  {
                    "purpose": {
                      "coding": [
                        {
                          "system": "http://hl7.org/fhir/contactentity-type",
                          "code": "ADMIN",
                          "display": "Administrative"
                        }
                      ]
                    },
                    "name": {
                      "use": "official",
                      "text": "Practice Manager"
                    },
                    "telecom": [
                      {
                        "system": "phone",
                        "value": "020 7946 0001",
                        "use": "work"
                      }
                    ]
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
