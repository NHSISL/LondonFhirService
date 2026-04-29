// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.ReferralRequests;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.ReferralRequests
{
    public partial class ReferralRequestMatcherServiceTests
    {
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly ReferralRequestMatcherService referralRequestMatcherService;

        public ReferralRequestMatcherServiceTests()
        {
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.referralRequestMatcherService =
                new ReferralRequestMatcherService(
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
            $"RR-{new IntRange(min: 1000, max: 9999).GetValue()}";

        private static JsonElement CreateReferralRequestResource(
            string ddsIdentifierValue,
            string id)
        {
            string json = $$"""
              {
                "resourceType": "ReferralRequest",
                "id": "{{id}}",
                "identifier": [
                  {
                    "system": "https://fhir.hl7.org.uk/Id/dds",
                    "value": "{{ddsIdentifierValue}}"
                  }
                ],
                "status": "active",
                "intent": "order"
              }
              """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateNonDdsReferralRequestResource(string id)
        {
            string json = $$"""
              {
                "resourceType": "ReferralRequest",
                "id": "{{id}}",
                "identifier": [
                  {
                    "system": "http://example.org/system",
                    "value": "RR-1"
                  }
                ],
                "status": "active",
                "intent": "order"
              }
              """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateReferralRequestResourceWithoutIdentifierProperty(string id)
        {
            string json = $$"""
              {
                "resourceType": "ReferralRequest",
                "id": "{{id}}",
                "status": "active",
                "intent": "order"
              }
              """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateComprehensiveReferralRequestResource(
            string ddsIdentifierValue,
            string id)
        {
            string json = $$"""
              {
                "resourceType": "ReferralRequest",
                "id": "{{id}}",
                "meta": {
                  "versionId": "1",
                  "lastUpdated": "2024-09-12T12:00:00+01:00",
                  "profile": [
                    "https://fhir.hl7.org.uk/STU3/StructureDefinition/CareConnect-ReferralRequest-1"
                  ]
                },
                "text": {
                  "status": "generated",
                  "div": "<div xmlns=\"http://www.w3.org/1999/xhtml\"><p>Referral to diabetes specialist.</p></div>"
                },
                "identifier": [
                  {
                    "use": "official",
                    "system": "https://fhir.nhs.uk/Id/referral-request-id",
                    "value": "{{ddsIdentifierValue}}"
                  },
                  {
                    "use": "usual",
                    "system": "https://fhir.hl7.org.uk/Id/dds",
                    "value": "{{ddsIdentifierValue}}"
                  }
                ],
                "status": "active",
                "intent": "order",
                "type": {
                  "coding": [
                    {
                      "system": "https://fhir.hl7.org.uk/STU3/CodeSystem/CareConnect-ReferralType-1",
                      "code": "GP",
                      "display": "GP referral"
                    }
                  ]
                },
                "priority": "routine",
                "serviceRequested": [
                  {
                    "coding": [
                      {
                        "system": "http://snomed.info/sct",
                        "code": "183523002",
                        "display": "Referral to diabetes specialist"
                      }
                    ]
                  }
                ],
                "subject": {
                  "reference": "Patient/6e6c53cb-fca3-4e34-9cbc-476c32f1eb3c"
                },
                "context": {
                  "reference": "Encounter/d9a5732b-3af5-4fa6-8cda-b91e76823cdd"
                },
                "occurrenceDateTime": "2024-09-12T12:00:00+01:00",
                "authoredOn": "2024-09-12T12:00:00+01:00",
                "requester": {
                  "agent": {
                    "reference": "Practitioner/8a77e616-2d12-4bc9-b0a1-2f1576ec1c05"
                  }
                },
                "specialty": {
                  "coding": [
                    {
                      "system": "http://snomed.info/sct",
                      "code": "394583002",
                      "display": "Endocrinology"
                    }
                  ]
                },
                "recipient": [
                  {
                    "reference": "Practitioner/8a77e616-2d12-4bc9-b0a1-2f1576ec1c05"
                  }
                ],
                "reasonCode": [
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
                "description": "Referral for further specialist diabetes care.",
                "note": [
                  {
                    "text": "Patient has had elevated HbA1c despite medication."
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
