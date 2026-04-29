// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.Appointments;
using Moq;
using Tynamix.ObjectFiller;

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

        private static JsonElement ParseJsonElement(string json) =>
            JsonDocument.Parse(json).RootElement.Clone();
    }
}
