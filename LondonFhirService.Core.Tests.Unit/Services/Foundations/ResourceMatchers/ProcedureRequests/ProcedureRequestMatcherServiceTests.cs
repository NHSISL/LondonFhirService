// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.ProcedureRequests;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.ProcedureRequests
{
    public partial class ProcedureRequestMatcherServiceTests
    {
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly ProcedureRequestMatcherService procedureRequestMatcherService;

        public ProcedureRequestMatcherServiceTests()
        {
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.procedureRequestMatcherService =
                new ProcedureRequestMatcherService(
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
            $"PRREQ-{new IntRange(min: 1000, max: 9999).GetValue()}";

        private static JsonElement CreateProcedureRequestResource(
            string ddsIdentifierValue,
            string id)
        {
            string json = $$"""
              {
                "resourceType": "ProcedureRequest",
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

        private static JsonElement CreateNonDdsProcedureRequestResource(string id)
        {
            string json = $$"""
              {
                "resourceType": "ProcedureRequest",
                "id": "{{id}}",
                "identifier": [
                  {
                    "system": "http://example.org/system",
                    "value": "PRREQ-1"
                  }
                ],
                "status": "active",
                "intent": "order"
              }
              """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateProcedureRequestResourceWithoutIdentifierProperty(string id)
        {
            string json = $$"""
              {
                "resourceType": "ProcedureRequest",
                "id": "{{id}}",
                "status": "active",
                "intent": "order"
              }
              """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateComprehensiveProcedureRequestResource(
            string ddsIdentifierValue,
            string id)
        {
            string json = $$"""
              {
                "resourceType": "ProcedureRequest",
                "id": "{{id}}",
                "meta": {
                  "versionId": "1",
                  "lastUpdated": "2024-09-12T09:30:00+01:00",
                  "profile": [
                    "https://fhir.hl7.org.uk/STU3/StructureDefinition/CareConnect-ProcedureRequest-1"
                  ]
                },
                "text": {
                  "status": "generated",
                  "div": "<div xmlns=\"http://www.w3.org/1999/xhtml\"><p>Request for blood test.</p></div>"
                },
                "identifier": [
                  {
                    "use": "official",
                    "system": "https://fhir.nhs.uk/Id/procedure-request-id",
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
                "priority": "routine",
                "category": [
                  {
                    "coding": [
                      {
                        "system": "http://snomed.info/sct",
                        "code": "108252007",
                        "display": "Laboratory procedure"
                      }
                    ]
                  }
                ],
                "code": {
                  "coding": [
                    {
                      "system": "http://snomed.info/sct",
                      "code": "167252002",
                      "display": "Complete blood count"
                    }
                  ],
                  "text": "Full blood count"
                },
                "subject": {
                  "reference": "Patient/6e6c53cb-fca3-4e34-9cbc-476c32f1eb3c"
                },
                "context": {
                  "reference": "Encounter/d9a5732b-3af5-4fa6-8cda-b91e76823cdd"
                },
                "occurrenceDateTime": "2024-09-12T09:30:00+01:00",
                "authoredOn": "2024-09-12T09:30:00+01:00",
                "requester": {
                  "agent": {
                    "reference": "Practitioner/8a77e616-2d12-4bc9-b0a1-2f1576ec1c05"
                  }
                },
                "performer": {
                  "reference": "Practitioner/8a77e616-2d12-4bc9-b0a1-2f1576ec1c05"
                },
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
                "note": [
                  {
                    "text": "Routine annual review."
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
