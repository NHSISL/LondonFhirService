// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Core.Brokers.Loggings;
using LondonFhirService.Core.Models.Orchestrations.Comparisons;
using LondonFhirService.Core.Services.Foundations.JsonElements;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.Patients;
using LondonFhirService.Core.Services.Foundations.ResourceMatchers.Practitioners;
using LondonFhirService.Core.Services.Orchestrations.Comparisons;
using LondonFhirService.Core.Services.Processings.JsonIgnoreRules;
using LondonFhirService.Core.Services.Processings.ListEntryComparisons;
using LondonFhirService.Core.Services.Processings.ResourceMatchings;
using Moq;

namespace LondonFhirService.Core.Tests.Unit.Services.Orchestrations.Comparisons
{
    /// <summary>
    /// The rest of the suite injects an empty rule list and a mocked IJsonElementService, which
    /// leaves the ignore rules themselves untested through the orchestration. These wire the real
    /// rules in the order the host registers them, against a real JsonElementService, because the
    /// bug they cover was an interaction between two rules rather than a fault in either: whether
    /// a value reaches the rule that should mask it depended on whether an array sat above it.
    /// </summary>
    public partial class ComparisonOrchestrationServiceTests
    {
        private static IComparisonOrchestrationService CreateServiceWithRealIgnoreRules(
            Mock<ILoggingBroker> loggingBrokerMock)
        {
            var jsonElementService = new JsonElementService();

            // Registration order matters and is asserted by proxy here: ArrayOrderIgnoreProcessing
            // Rule claims every array, so registering it first is exactly the arrangement that
            // used to stop anything inside an array reaching the rules below it.
            var ignoreRules = new List<IJsonIgnoreProcessingRule>
            {
                new ArrayOrderIgnoreProcessingRule(jsonElementService, loggingBrokerMock.Object),
                new GuidIgnoreProcessingRule(jsonElementService, loggingBrokerMock.Object),
                new IdIgnoreProcessingRule(jsonElementService, loggingBrokerMock.Object),
                new MetaIgnoreProcessingRule(jsonElementService, loggingBrokerMock.Object),
                new ReferenceIgnoreProcessingRule(jsonElementService, loggingBrokerMock.Object)
            };

            var resourceMatcherProcessingService = new ResourceMatcherProcessingService(
                matchers: new List<IResourceMatcherService>
                {
                    new PatientMatcherService(loggingBrokerMock.Object),
                    new PractitionerMatcherService(loggingBrokerMock.Object)
                },
                loggingBroker: loggingBrokerMock.Object);

            return new ComparisonOrchestrationService(
                ignoreRules: ignoreRules,
                resourceMatcherProcessingService: resourceMatcherProcessingService,
                listEntryComparisonProcessingService:
                    new Mock<IListEntryComparisonProcessingService>().Object,
                jsonElementService: jsonElementService,
                loggingBroker: loggingBrokerMock.Object);
        }

        private static string CreatePatientBundle(
            string managingOrganizationGuid,
            string generalPractitionerGuid) =>
            "{\"resourceType\":\"Bundle\",\"entry\":[{\"resource\":{" +
            "\"resourceType\":\"Patient\"," +
            $"\"id\":\"{managingOrganizationGuid}\"," +
            "\"identifier\":[{\"system\":\"https://fhir.hl7.org.uk/Id/nhs-number\"," +
            "\"value\":\"9660979622\"}]," +
            $"\"managingOrganization\":{{\"reference\":\"Organization/{managingOrganizationGuid}\"}}," +
            $"\"generalPractitioner\":[{{\"reference\":\"Practitioner/{generalPractitionerGuid}\"}}]" +
            "}}]}";

        /// <summary>
        /// generalPractitioner is an array and managingOrganization is not. Before the ignore
        /// rules descended into children first, only the second of those was masked and the array
        /// one reported a difference - the same value treated two ways.
        /// </summary>
        [Fact]
        public async Task ShouldIgnoreGuidReferencesInsideAndOutsideArraysOnCompareAsync()
        {
            // given
            var loggingBrokerMock = new Mock<ILoggingBroker>();

            IComparisonOrchestrationService comparisonOrchestrationService =
                CreateServiceWithRealIgnoreRules(loggingBrokerMock);

            string randomCorrelationId = GetRandomString();
            string inputCorrelationId = randomCorrelationId;

            string source1Json = CreatePatientBundle(
                managingOrganizationGuid: "92f740a2-88ad-4e76-b948-9583b85ecb15",
                generalPractitionerGuid: "a1b2c3d4-e5f6-7890-abcd-ef1234567890");

            string source2Json = CreatePatientBundle(
                managingOrganizationGuid: "ffa73264-d1e2-4c3b-9a8f-1b2c3d4e5f60",
                generalPractitionerGuid: "0badc0de-1111-2222-3333-444455556666");

            // when
            ComparisonResult actualComparisonResult =
                await comparisonOrchestrationService.CompareAsync(
                    inputCorrelationId,
                    source1Json,
                    source2Json);

            // then
            actualComparisonResult.Diffs.Should().BeEmpty();
            actualComparisonResult.DiffCount.Should().Be(0);
        }

        /// <summary>
        /// The masking keeps the resource type, so a reference that changes what it points at is
        /// still a difference - including one inside an array, which is the half that used to be
        /// skipped entirely.
        /// </summary>
        [Fact]
        public async Task ShouldStillReportReferenceDifferenceWhenResourceTypeDiffersOnCompareAsync()
        {
            // given
            var loggingBrokerMock = new Mock<ILoggingBroker>();

            IComparisonOrchestrationService comparisonOrchestrationService =
                CreateServiceWithRealIgnoreRules(loggingBrokerMock);

            string randomCorrelationId = GetRandomString();
            string inputCorrelationId = randomCorrelationId;

            string source1Json = CreatePatientBundle(
                managingOrganizationGuid: "92f740a2-88ad-4e76-b948-9583b85ecb15",
                generalPractitionerGuid: "a1b2c3d4-e5f6-7890-abcd-ef1234567890");

            string source2Json = source1Json
                .Replace(
                    "Practitioner/a1b2c3d4-e5f6-7890-abcd-ef1234567890",
                    "Organization/0badc0de-1111-2222-3333-444455556666");

            // when
            ComparisonResult actualComparisonResult =
                await comparisonOrchestrationService.CompareAsync(
                    inputCorrelationId,
                    source1Json,
                    source2Json);

            // then
            actualComparisonResult.Diffs.Should().ContainSingle();

            actualComparisonResult.Diffs.Single().Path
                .Should().EndWith("generalPractitioner[0].reference");

            actualComparisonResult.Diffs.Single().OldValue.Should().Be("Practitioner/<GUID>");
            actualComparisonResult.Diffs.Single().NewValue.Should().Be("Organization/<GUID>");
        }

        /// <summary>
        /// A reference whose id is a business identifier carries real meaning, so it keeps
        /// comparing as itself whether or not an array sits above it.
        /// </summary>
        [Fact]
        public async Task ShouldStillReportNonGuidReferenceDifferenceInsideArrayOnCompareAsync()
        {
            // given
            var loggingBrokerMock = new Mock<ILoggingBroker>();

            IComparisonOrchestrationService comparisonOrchestrationService =
                CreateServiceWithRealIgnoreRules(loggingBrokerMock);

            string randomCorrelationId = GetRandomString();
            string inputCorrelationId = randomCorrelationId;

            string source1Json = CreatePatientBundle(
                managingOrganizationGuid: "92f740a2-88ad-4e76-b948-9583b85ecb15",
                generalPractitionerGuid: "a1b2c3d4-e5f6-7890-abcd-ef1234567890")
                    .Replace("Practitioner/a1b2c3d4-e5f6-7890-abcd-ef1234567890", "Practitioner/G123456");

            string source2Json = CreatePatientBundle(
                managingOrganizationGuid: "ffa73264-d1e2-4c3b-9a8f-1b2c3d4e5f60",
                generalPractitionerGuid: "0badc0de-1111-2222-3333-444455556666")
                    .Replace("Practitioner/0badc0de-1111-2222-3333-444455556666", "Practitioner/G654321");

            // when
            ComparisonResult actualComparisonResult =
                await comparisonOrchestrationService.CompareAsync(
                    inputCorrelationId,
                    source1Json,
                    source2Json);

            // then
            actualComparisonResult.Diffs.Should().ContainSingle();
            actualComparisonResult.Diffs.Single().OldValue.Should().Be("Practitioner/G123456");
            actualComparisonResult.Diffs.Single().NewValue.Should().Be("Practitioner/G654321");
        }
    }
}
