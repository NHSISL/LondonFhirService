// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.Appointments;
using Moq;
using Tynamix.ObjectFiller;
using Xeptions;

namespace LondonFhirService.Core.Tests.Unit.Services.Foundations.ResourceMatchers.Appointments
{
    public partial class AppointmentMatcherServiceTests
    {
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly AppointmentMatcherService appointmentMatcherService;

        public AppointmentMatcherServiceTests()
        {
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.appointmentMatcherService =
                new AppointmentMatcherService(
                    loggingBroker: this.loggingBrokerMock.Object);
        }

        private static Expression<Func<Xeption, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => actualException.SameExceptionAs(expectedException);

        private static Dictionary<string, JsonElement> CreateResourceIndex() =>
            new();

        private static string GetRandomDdsIdentifierValue() =>
            $"APP-{new IntRange(min: 1000, max: 9999).GetValue()}";

        private static JsonElement CreateAppointmentWithDdsIdentifier(
            string ddsIdentifierValue,
            string id = "appointment-1")
        {
            string json = $$"""
            {
              "resourceType": "Appointment",
              "id": "{{id}}",
              "identifier": [
                {
                  "system": "https://fhir.hl7.org.uk/Id/dds",
                  "value": "{{ddsIdentifierValue}}"
                }
              ],
              "status": "fulfilled"
            }
            """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateAppointmentWithoutIdentifier(string id = "appointment-1")
        {
            string json = $$"""
            {
              "resourceType": "Appointment",
              "id": "{{id}}",
              "status": "fulfilled"
            }
            """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateAppointmentWithMultipleIdentifiers(
            string ddsIdentifierValue,
            string id = "appointment-1")
        {
            string json = $$"""
            {
              "resourceType": "Appointment",
              "id": "{{id}}",
              "identifier": [
                {
                  "system": "https://fhir.nhs.uk/Id/appointment-id",
                  "value": "{{ddsIdentifierValue}}"
                },
                {
                  "system": "https://fhir.hl7.org.uk/Id/dds",
                  "value": "{{ddsIdentifierValue}}"
                }
              ],
              "status": "fulfilled"
            }
            """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateAppointmentWithNonDdsIdentifier(string id = "appointment-1")
        {
            string json = $$"""
            {
              "resourceType": "Appointment",
              "id": "{{id}}",
              "identifier": [
                {
                  "system": "http://example.org/system",
                  "value": "12345"
                }
              ],
              "status": "fulfilled"
            }
            """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateAppointmentWithDdsIdentifierMissingValue(string id = "appointment-1")
        {
            string json = $$"""
            {
              "resourceType": "Appointment",
              "id": "{{id}}",
              "identifier": [
                {
                  "system": "https://fhir.hl7.org.uk/Id/dds"
                }
              ],
              "status": "fulfilled"
            }
            """;

            return ParseJsonElement(json);
        }

        private static JsonElement CreateComprehensiveAppointmentResource(
            string ddsIdentifierValue,
            string id)
        {
            string json = $$"""
              {
                "resourceType": "Appointment",
                "id": "{{id}}",
                "meta": {
                  "versionId": "1",
                  "lastUpdated": "2024-09-12T09:30:00+01:00",
                  "profile": [
                    "https://fhir.nhs.uk/STU3/StructureDefinition/CareConnect-Appointment-1"
                  ]
                },
                "text": {
                  "status": "generated",
                  "div": "<div xmlns=\"http://www.w3.org/1999/xhtml\"><p>Routine GP follow-up appointment.</p></div>"
                },
                "identifier": [
                  {
                    "use": "official",
                    "system": "https://fhir.nhs.uk/Id/appointment-id",
                    "value": "{{ddsIdentifierValue}}"
                  },
                  {
                    "use": "usual",
                    "system": "https://fhir.hl7.org.uk/Id/dds",
                    "value": "{{ddsIdentifierValue}}"
                  }
                ],
                "status": "fulfilled",
                "serviceCategory": {
                  "coding": [
                    {
                      "system": "http://snomed.info/sct",
                      "code": "394814009",
                      "display": "General practice (specialty)"
                    }
                  ]
                },
                "serviceType": [
                  {
                    "coding": [
                      {
                        "system": "https://fhir.hl7.org.uk/STU3/CodeSystem/CareConnect-AppointmentServiceType-1",
                        "code": "124",
                        "display": "GP Surgery Appointment"
                      }
                    ]
                  }
                ],
                "appointmentType": {
                  "coding": [
                    {
                      "system": "http://hl7.org/fhir/v2/0276",
                      "code": "ROUTINE",
                      "display": "Routine appointment"
                    }
                  ]
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
                "priority": 5,
                "description": "Routine follow-up appointment",
                "start": "2024-09-12T09:00:00+01:00",
                "end": "2024-09-12T09:25:00+01:00",
                "minutesDuration": 25,
                "created": "2024-09-01",
                "comment": "Patient confirmed attendance.",
                "participant": [
                  {
                    "actor": {
                      "reference": "Patient/6e6c53cb-fca3-4e34-9cbc-476c32f1eb3c"
                    },
                    "status": "accepted",
                    "required": "required"
                  },
                  {
                    "actor": {
                      "reference": "Practitioner/8a77e616-2d12-4bc9-b0a1-2f1576ec1c05"
                    },
                    "status": "accepted",
                    "required": "required"
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
