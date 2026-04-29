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

        private static JsonElement ParseJsonElement(string json) =>
            JsonDocument.Parse(json).RootElement.Clone();
    }
}
