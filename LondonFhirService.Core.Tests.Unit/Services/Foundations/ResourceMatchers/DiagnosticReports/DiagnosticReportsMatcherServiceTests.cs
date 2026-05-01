// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.DiagnosticReports;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.DiagnosticReports
{
    public partial class DiagnosticReportMatcherServiceTests
    {
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly DiagnosticReportMatcherService diagnosticReportMatcherService;

        public DiagnosticReportMatcherServiceTests()
        {
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.diagnosticReportMatcherService =
                new DiagnosticReportMatcherService(
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
            $"DR-{new IntRange(min: 1000, max: 9999).GetValue()}";

        private static JsonElement CreateDiagnosticReportResource(
            string ddsIdentifierValue,
            string id)
        {
            string json = $$"""
              {
                "resourceType": "DiagnosticReport",
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

        private static JsonElement CreateNonDdsDiagnosticReportResource(string id)
        {
            string json = $$"""
              {
                "resourceType": "DiagnosticReport",
                "id": "{{id}}",
                "identifier": [
                  {
                    "system": "http://example.org/system",
                    "value": "DR-1"
                  }
                ],
                "status": "final"
              }
              """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateDiagnosticReportResourceWithoutIdentifierProperty(string id)
        {
            string json = $$"""
              {
                "resourceType": "DiagnosticReport",
                "id": "{{id}}",
                "status": "final"
              }
              """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateComprehensiveDiagnosticReportResource(
            string ddsIdentifierValue,
            string id)
        {
            string json = $$"""
              {
                "resourceType": "DiagnosticReport",
                "id": "{{id}}",
                "meta": {
                  "versionId": "1",
                  "lastUpdated": "2024-09-12T11:00:00+01:00",
                  "profile": [
                    "https://fhir.hl7.org.uk/STU3/StructureDefinition/CareConnect-DiagnosticReport-1"
                  ]
                },
                "text": {
                  "status": "generated",
                  "div": "<div xmlns=\"http://www.w3.org/1999/xhtml\"><p>Blood test results.</p></div>"
                },
                "identifier": [
                  {
                    "use": "official",
                    "system": "https://fhir.nhs.uk/Id/diagnostic-report-id",
                    "value": "{{ddsIdentifierValue}}"
                  },
                  {
                    "use": "usual",
                    "system": "https://fhir.hl7.org.uk/Id/dds",
                    "value": "{{ddsIdentifierValue}}"
                  }
                ],
                "basedOn": [
                  { "reference": "ProcedureRequest/80a1c739-39d5-4b4d-bf21-1e2c89aa9982" }
                ],
                "status": "final",
                "category": {
                  "coding": [
                    {
                      "system": "http://hl7.org/fhir/v2/0074",
                      "code": "LAB",
                      "display": "Laboratory"
                    }
                  ]
                },
                "code": {
                  "coding": [
                    {
                      "system": "http://snomed.info/sct",
                      "code": "396550006",
                      "display": "Blood test (procedure)"
                    }
                  ],
                  "text": "Complete blood count"
                },
                "subject": {
                  "reference": "Patient/6e6c53cb-fca3-4e34-9cbc-476c32f1eb3c"
                },
                "context": {
                  "reference": "Encounter/d9a5732b-3af5-4fa6-8cda-b91e76823cdd"
                },
                "effectiveDateTime": "2024-09-12T10:30:00+01:00",
                "issued": "2024-09-12T11:00:00+01:00",
                "performer": [
                  {
                    "actor": {
                      "reference": "Practitioner/8a77e616-2d12-4bc9-b0a1-2f1576ec1c05"
                    },
                    "role": {
                      "coding": [
                        {
                          "system": "http://hl7.org/fhir/v2/0912",
                          "code": "PRF",
                          "display": "Performer"
                        }
                      ]
                    }
                  }
                ],
                "specimen": [
                  { "reference": "Specimen/sp1c2d3e-4f56-4789-90ab-cdef01234567" }
                ],
                "result": [
                  { "reference": "Observation/b5e8c4a0-e1f4-4b5d-9c5d-bb91a4d12345" }
                ],
                "conclusion": "Results within expected range. No abnormalities detected.",
                "codedDiagnosis": [
                  {
                    "coding": [
                      {
                        "system": "http://snomed.info/sct",
                        "code": "44054006",
                        "display": "Type 2 diabetes mellitus (disorder)"
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
